﻿using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class StatisticsManager : StandardManager, IBackgroundTaskSource
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public async Task IncrementPlayCount(PlaylistItem playlistItem)
        {
            using (var task = new UpdatePlaylistPlayCountTask(playlistItem))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;
    }
}
