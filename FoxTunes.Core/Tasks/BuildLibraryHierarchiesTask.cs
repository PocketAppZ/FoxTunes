﻿using System;
using FoxTunes.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : BackgroundTask
    {
        public const string ID = "B6AF297E-F334-481D-8D60-BD5BE5935BD9";

        public BuildLibraryHierarchiesTask()
            : base(ID)
        {
        }

        public ILibrary Library { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Library = core.Components.Library;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.SetPosition(0);
            this.SetCount(this.Library.LibraryHierarchyQuery.Count());
            foreach (var libraryHierarchy in this.Library.LibraryHierarchyQuery)
            {
                this.SetDescription(libraryHierarchy.Name);
                this.ClearHierarchy(libraryHierarchy);
                var libraryHierarchyItems = this.BuildHierarchy(libraryHierarchy);
                foreach (var libraryHierarchyItem in libraryHierarchyItems)
                {
                    this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => 
                    {
                        libraryHierarchy.Items.Add(libraryHierarchyItem);
                    }));
                }
                this.SetPosition(this.Position + 1);
            }
            this.SetPosition(this.Count);
            this.SaveChanges();
        }

        private void ClearHierarchy(LibraryHierarchy libraryHierarchy)
        {
            this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => libraryHierarchy.Items.Clear()));
        }

        private IEnumerable<LibraryHierarchyItem> BuildHierarchy(LibraryHierarchy libraryHierarchy)
        {
            return this.BuildHierarchy(libraryHierarchy, null, this.Library.LibraryItemQuery, 0);
        }

        private IEnumerable<LibraryHierarchyItem> BuildHierarchy(LibraryHierarchy libraryHierarchy, LibraryHierarchyItem parent, IEnumerable<LibraryItem> libraryItems, int level)
        {
            var isLeaf = level >= libraryHierarchy.Levels.Count - 1;
            var libraryHierarchyLevel = libraryHierarchy.Levels[level];
            var query =
                from libraryItem in libraryItems
                group libraryItem by new
                {
                    Display = this.ExecuteScript(libraryItem, libraryHierarchyLevel.DisplayScript),
                    Sort = this.ExecuteScript(libraryItem, libraryHierarchyLevel.SortScript),
                } into hierarchy
                select new LibraryHierarchyItem(hierarchy.Key.Display, hierarchy.Key.Sort, isLeaf)
                {
                    Parent = parent,
                    Items = new ObservableCollection<LibraryItem>(hierarchy)
                };
            var libraryHierarchyItems = query.ToList();
            if (!isLeaf)
            {
                foreach (var libraryHierarchyItem in libraryHierarchyItems)
                {
                    libraryHierarchyItem.Children = new ObservableCollection<LibraryHierarchyItem>(
                        this.BuildHierarchy(libraryHierarchy, libraryHierarchyItem, libraryHierarchyItem.Items, level + 1)
                    );
                }
            }
            return libraryHierarchyItems;
        }

        private void SaveChanges()
        {
            this.SetName("Saving changes");
            this.SetPosition(this.Count);
            this.Database.Interlocked(() => this.Database.SaveChanges());
        }

        private string ExecuteScript(LibraryItem libraryItem, string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            this.EnsureScriptingContext();
            var runner = new LibraryItemScriptRunner(this.ScriptingContext, libraryItem, script);
            runner.Prepare();
            return Convert.ToString(runner.Run());
        }

        private void EnsureScriptingContext()
        {
            if (this.ScriptingContext != null)
            {
                return;
            }
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
        }
    }
}