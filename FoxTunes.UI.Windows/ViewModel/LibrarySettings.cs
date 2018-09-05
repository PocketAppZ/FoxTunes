﻿using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibrarySettings : ViewModelBase
    {
        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private LibraryHierarchy _SelectedLibraryHierarchy { get; set; }

        public LibraryHierarchy SelectedLibraryHierarchy
        {
            get
            {
                return this._SelectedLibraryHierarchy;
            }
            set
            {
                this._SelectedLibraryHierarchy = value;
                this.OnSelectedLibraryHierarchyChanged();
            }
        }

        protected virtual void OnSelectedLibraryHierarchyChanged()
        {
            if (this.SelectedLibraryHierarchy != null)
            {
                this.SelectedHierarchyLevel = this.SelectedLibraryHierarchy.Levels.FirstOrDefault();
            }
            else
            {
                this.SelectedHierarchyLevel = null;
            }
            if (this.SelectedLibraryHierarchyChanged != null)
            {
                this.SelectedLibraryHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedLibraryHierarchy");
        }

        public event EventHandler SelectedLibraryHierarchyChanged = delegate { };

        private ObservableCollection<LibraryHierarchy> _LibraryHierarchies { get; set; }

        public ObservableCollection<LibraryHierarchy> LibraryHierarchies
        {
            get
            {
                return this._LibraryHierarchies;
            }
            set
            {
                this._LibraryHierarchies = value;
                this.OnLibraryHierarchiesChanged();
            }
        }

        protected virtual void OnLibraryHierarchiesChanged()
        {
            if (this.LibraryHierarchiesChanged != null)
            {
                this.LibraryHierarchiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LibraryHierarchies");
        }

        public event EventHandler LibraryHierarchiesChanged = delegate { };

        private LibraryHierarchyLevel _SelectedHierarchyLevel { get; set; }

        public LibraryHierarchyLevel SelectedHierarchyLevel
        {
            get
            {
                return this._SelectedHierarchyLevel;
            }
            set
            {
                this._SelectedHierarchyLevel = value;
                this.OnSelectedHierarchyLevelChanged();
            }
        }

        protected virtual void OnSelectedHierarchyLevelChanged()
        {
            if (this.SelectedHierarchyLevelChanged != null)
            {
                this.SelectedHierarchyLevelChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchyLevel");
        }

        public event EventHandler SelectedHierarchyLevelChanged = delegate { };

        private bool _SettingsVisible { get; set; }

        public bool SettingsVisible
        {
            get
            {
                return this._SettingsVisible;
            }
            set
            {
                this._SettingsVisible = value;
                this.OnSettingsVisibleChanged();
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged = delegate { };

        public ICommand NewHierarchyCommand
        {
            get
            {
                return new Command(
                    () => this.Database.Sets.LibraryHierarchy.Create().With(libraryHierarchy =>
                    {
                        libraryHierarchy.Name = "New";
                        this.LibraryHierarchies.Add(libraryHierarchy);
                        this.SelectedLibraryHierarchy = libraryHierarchy;
                    }),
                    () => this.LibraryHierarchies != null
                );
            }
        }

        public ICommand DeleteHierarchyCommand
        {
            get
            {
                return new Command(
                    () =>
                    {
                        this.LibraryHierarchies.Remove(this.SelectedLibraryHierarchy);
                        this.SelectedLibraryHierarchy = null;
                    },
                    () => this.LibraryHierarchies != null && this.SelectedLibraryHierarchy != null
                );
            }
        }

        public ICommand NewHierarchyLevelCommand
        {
            get
            {
                return new Command(
                    () => this.Database.Sets.LibraryHierarchyLevel.Create().With(libraryHierarchyLevel =>
                    {
                        libraryHierarchyLevel.Name = "New";
                        this.SelectedLibraryHierarchy.Levels.Add(libraryHierarchyLevel);
                        this.SelectedHierarchyLevel = libraryHierarchyLevel;
                    }),
                    () => this.SelectedLibraryHierarchy != null
                );
            }
        }

        public ICommand DeleteHierarchyLevelCommand
        {
            get
            {
                return new Command(
                    () =>
                    {
                        this.SelectedLibraryHierarchy.Levels.Remove(this.SelectedHierarchyLevel);
                        this.SelectedHierarchyLevel = null;
                    },
                    () => this.SelectedHierarchyLevel != null
                );
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new Command(this.Save)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Save()
        {
            try
            {
                using (var transaction = this.Database.BeginTransaction())
                {
                    var libraryHierarchies = this.Database.Set<LibraryHierarchy>(transaction);
                    libraryHierarchies.Remove(libraryHierarchies.Except(this.LibraryHierarchies));
                    libraryHierarchies.AddOrUpdate(this.LibraryHierarchies);
                    transaction.Commit();
                }
                this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
                this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            }
            catch (Exception e)
            {
                this.OnError("Save", e);
                throw;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Cancel()
        {
            this.Refresh();
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.Refresh();
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
        {
            this.LibraryHierarchies = new ObservableCollection<LibraryHierarchy>(this.Database.Sets.LibraryHierarchy);
            this.SelectedLibraryHierarchy = this.LibraryHierarchies.FirstOrDefault();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibrarySettings();
        }
    }
}
