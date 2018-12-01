﻿using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class DatabaseSets : BaseComponent, IDatabaseSets
    {
        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IDatabaseSet<PlaylistItem> PlaylistItem
        {
            get
            {
                return this.Database.Set<PlaylistItem>();
            }
        }

        protected virtual Task OnPlaylistItemChanged()
        {
            return this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("PlaylistItem"));
        }

        public IDatabaseSet<PlaylistColumn> PlaylistColumn
        {
            get
            {
                return this.Database.Set<PlaylistColumn>();
            }
        }

        protected virtual Task OnPlaylistColumnChanged()
        {
            return this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("PlaylistColumn"));
        }

        public IDatabaseSet<LibraryItem> LibraryItem
        {
            get
            {
                return this.Database.Set<LibraryItem>();
            }
        }

        protected virtual Task OnLibraryItemChanged()
        {
            return this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryItem"));
        }

        public IDatabaseSet<LibraryHierarchy> LibraryHierarchy
        {
            get
            {
                return this.Database.Set<LibraryHierarchy>();
            }
        }

        protected virtual Task OnLibraryHierarchyChanged()
        {
            return this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryHierarchy"));
        }

        public IDatabaseSet<LibraryHierarchyLevel> LibraryHierarchyLevel
        {
            get
            {
                return this.Database.Set<LibraryHierarchyLevel>();
            }
        }

        protected virtual Task OnLibraryHierarchyLevelChanged()
        {
            return this.ForegroundTaskRunner.Run(() => this.OnPropertyChanged("LibraryHierarchyLevel"));
        }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.OnPlaylistItemChanged();
                case CommonSignals.HierarchiesUpdated:
                    return this.OnLibraryHierarchyChanged();
            }
            return Task.CompletedTask;
        }
    }
}
