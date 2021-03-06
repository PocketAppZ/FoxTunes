﻿#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class LibraryHierarchyBrowser : StandardComponent, ILibraryHierarchyBrowser, IDisposable
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public LibraryHierarchyBrowser()
        {
            this.Filter = string.Empty;
        }

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IFilterParser FilterParser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private string _Filter { get; set; }

        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                if (string.Equals(this._Filter, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                this._Filter = value;
                this.OnFilterChanged();
            }
        }

        protected virtual void OnFilterChanged()
        {
            if (this.FilterChanged != null)
            {
                this.FilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Filter");
        }

        public event EventHandler FilterChanged;

        private LibraryHierarchyBrowserState _State { get; set; }

        public LibraryHierarchyBrowserState State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
                this.OnStateChanged();
            }
        }

        protected virtual void OnStateChanged()
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("State");
        }

        public event EventHandler StateChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
            this.DatabaseFactory = core.Factories.Database;
            this.FilterParser = core.Components.FilterParser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    if (!string.IsNullOrEmpty(this.Filter))
                    {
                        if (names != null && names.Any() && !this.FilterParser.AppliesTo(this.Filter, names))
                        {
                            //There is a filter but the meta data update does not apply to it.
                            //Nothing to do.
                            break;
                        }
                        Logger.Write(this, LogLevel.Debug, "Meta data was updated and there is an active filter which applies to it, refreshing.");
                        return this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public LibraryHierarchy[] GetHierarchies()
        {
            return this.LibraryHierarchyCache.GetHierarchies(this.GetHierarchiesCore);
        }

        private IEnumerable<LibraryHierarchy> GetHierarchiesCore()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<LibraryHierarchy>(transaction);
                    set.Fetch.Filter.AddColumn(
                        set.Table.GetColumn(ColumnConfig.By("Enabled", ColumnFlags.None))
                    ).With(filter => filter.Right = filter.CreateConstant(1));
                    set.Fetch.Sort.Expressions.Clear();
                    set.Fetch.Sort.AddColumn(set.Table.GetColumn(ColumnConfig.By("Sequence", ColumnFlags.None)));
                    foreach (var element in set)
                    {
                        yield return element;
                    }
                }
            }
        }

        public LibraryHierarchyNode[] GetNodes(LibraryHierarchy libraryHierarchy)
        {
            return this.GetNodes(libraryHierarchy, this.Filter);
        }

        public LibraryHierarchyNode[] GetNodes(LibraryHierarchy libraryHierarchy, string filter)
        {
            var key = new LibraryHierarchyCacheKey(libraryHierarchy, null, filter);
            return this.LibraryHierarchyCache.GetNodes(key, () => this.GetNodesCore(libraryHierarchy, filter));
        }

        public LibraryHierarchyNode[] GetNodes(LibraryHierarchyNode libraryHierarchyNode)
        {
            return this.GetNodes(libraryHierarchyNode, this.Filter);
        }

        public LibraryHierarchyNode[] GetNodes(LibraryHierarchyNode libraryHierarchyNode, string filter)
        {
            var key = new LibraryHierarchyCacheKey(null, libraryHierarchyNode, filter);
            return this.LibraryHierarchyCache.GetNodes(key, () => this.GetNodesCore(libraryHierarchyNode, filter));
        }

        protected virtual IEnumerable<LibraryHierarchyNode> GetNodesCore(LibraryHierarchy libraryHierarchy, string filter)
        {
            this.State |= LibraryHierarchyBrowserState.Loading;
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var nodes = database.ExecuteEnumerator<LibraryHierarchyNode>(database.Queries.GetLibraryHierarchyNodes(filter), (parameters, phase) =>
                        {
                            switch (phase)
                            {
                                case DatabaseParameterPhase.Fetch:
                                    parameters["libraryHierarchyId"] = libraryHierarchy.Id;
                                    break;
                            }
                        }, transaction);
                        foreach (var node in nodes)
                        {
                            node.InitializeComponent(this.Core);
                            yield return node;
                        }
                    }
                }
            }
            finally
            {
                this.State &= ~LibraryHierarchyBrowserState.Loading;
            }
        }

        protected virtual IEnumerable<LibraryHierarchyNode> GetNodesCore(LibraryHierarchyNode libraryHierarchyNode, string filter)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var nodes = database.ExecuteEnumerator<LibraryHierarchyNode>(database.Queries.GetLibraryHierarchyNodes(filter), (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                                break;
                        }
                    }, transaction);
                    foreach (var node in nodes)
                    {
                        node.Parent = libraryHierarchyNode;
                        node.InitializeComponent(this.Core);
                        yield return node;
                    }
                }
            }
        }

        public IEnumerable<LibraryItem> GetItems(LibraryHierarchyNode libraryHierarchyNode, bool loadMetaData)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var items = database.ExecuteEnumerator<LibraryItem>(database.Queries.GetLibraryItems(this.Filter), (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                                parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                                parameters["loadMetaData"] = loadMetaData;
                                parameters["metaDataType"] = META_DATA_TYPE;
                                break;
                        }
                    }, transaction);
                    foreach (var item in items)
                    {
                        item.InitializeComponent(this.Core);
                        yield return item;
                    }
                }
            }
        }

        private LibraryHierarchyNode FindNode(IEnumerable<LibraryHierarchyNode> nodes, string value)
        {
            return nodes.FirstOrDefault(node => string.Equals(node.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~LibraryHierarchyBrowser()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
