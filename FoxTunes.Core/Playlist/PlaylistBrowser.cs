﻿using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistBrowser : StandardComponent, IPlaylistBrowser
    {
        private PlaylistBrowserState _State { get; set; }

        public PlaylistBrowserState State
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

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistCache PlaylistCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public PlaylistNavigationStrategy NavigationStrategy { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistCache = core.Components.PlaylistCache;
            this.DatabaseFactory = core.Factories.Database;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistBehaviourConfiguration.SHUFFLE_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.NavigationStrategy = new ShufflePlaylistNavigationStrategy();
                }
                else
                {
                    this.NavigationStrategy = new StandardPlaylistNavigationStrategy();
                }
                this.NavigationStrategy.InitializeComponent(this.Core);
            });
            base.InitializeComponent(core);
        }

        public Playlist[] GetPlaylists()
        {
            return this.PlaylistCache.GetPlaylists(this.GetPlaylistsCore);
        }

        private IEnumerable<Playlist> GetPlaylistsCore()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<Playlist>(transaction);
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

        public Playlist GetPlaylist(PlaylistItem playlistItem)
        {
            return this.GetPlaylists().FirstOrDefault(playlist => playlist.Id == playlistItem.Playlist_Id);
        }

        public PlaylistItem[] GetItems(Playlist playlist)
        {
            return this.PlaylistCache.GetItems(playlist, () => this.GetItemsCore(playlist));
        }

        private IEnumerable<PlaylistItem> GetItemsCore(Playlist playlist)
        {
            this.State |= PlaylistBrowserState.Loading;
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var sequence = database.AsQueryable<PlaylistItem>(transaction)
                            .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id)
                            .OrderBy(playlistItem => playlistItem.Sequence);
                        foreach (var element in sequence)
                        {
                            yield return element;
                        }
                    }
                }
            }
            finally
            {
                this.State &= ~PlaylistBrowserState.Loading;
            }
        }

        public virtual async Task<PlaylistItem> GetItem(Playlist playlist, int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Sequence == sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public Task<PlaylistItem> GetNextItem(Playlist playlist, bool navigate)
        {
            return this.NavigationStrategy.GetNext(this.PlaylistManager.CurrentItem, navigate);
        }

        public Task<PlaylistItem> GetNextItem(PlaylistItem playlistItem)
        {
            return this.NavigationStrategy.GetNext(playlistItem, false);
        }

        public Task<PlaylistItem> GetPreviousItem(Playlist playlist, bool navigate)
        {
            return this.NavigationStrategy.GetPrevious(this.PlaylistManager.CurrentItem, navigate);
        }

        public Task<PlaylistItem> GetPreviousItem(PlaylistItem playlistItem)
        {
            return this.NavigationStrategy.GetPrevious(playlistItem, false);
        }

        public virtual async Task<PlaylistItem> GetItem(Playlist playlist, string fileName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.FileName == fileName)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<int> GetInsertIndex(Playlist playlist)
        {
            var playlistItem = await this.NavigationStrategy.GetLastPlaylistItem(playlist).ConfigureAwait(false);
            if (playlistItem == null)
            {
                return 0;
            }
            else
            {
                return playlistItem.Sequence + 1;
            }
        }
    }
}
