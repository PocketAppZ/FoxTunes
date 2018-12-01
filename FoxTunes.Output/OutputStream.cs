﻿using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class OutputStream : BaseComponent, IOutputStream
    {
        protected OutputStream(PlaylistItem playlistItem)
        {
            this.PlaylistItem = playlistItem;
        }

        public int Id
        {
            get
            {
                return this.PlaylistItem.Id;
            }
        }

        public string FileName
        {
            get
            {
                return this.PlaylistItem.FileName;
            }
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public abstract long Position { get; set; }

        protected virtual async Task OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                var e = new AsyncEventArgs();
                this.PositionChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("Position");
        }

        public event AsyncEventHandler PositionChanged = delegate { };

        public abstract long Length { get; }

        public abstract int Rate { get; }

        public abstract int Channels { get; }

        public abstract bool IsPlaying { get; }

        protected virtual async Task OnIsPlayingChanged()
        {
            if (this.IsPlayingChanged != null)
            {
                var e = new AsyncEventArgs();
                this.IsPlayingChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("IsPlaying");
        }

        public event AsyncEventHandler IsPlayingChanged = delegate { };

        public abstract bool IsPaused { get; }

        protected virtual async Task OnIsPausedChanged()
        {
            if (this.IsPausedChanged != null)
            {
                var e = new AsyncEventArgs();
                this.IsPausedChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("IsPaused");
        }

        public event AsyncEventHandler IsPausedChanged = delegate { };

        public abstract bool IsStopped { get; }

        protected virtual async Task OnIsStoppedChanged()
        {
            if (this.IsStoppedChanged != null)
            {
                var e = new AsyncEventArgs();
                this.IsStoppedChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("IsStopped");
        }

        public event AsyncEventHandler IsStoppedChanged = delegate { };

        public abstract Task Play();

        protected virtual Task OnPlayed(bool manual)
        {
            if (this.Played == null)
            {
                return Task.CompletedTask;
            }
            var e = new PlayedEventArgs(manual);
            this.Played(this, e);
            return e.Complete();
        }

        public event PlayedEventHandler Played = delegate { };

        public abstract Task Pause();

        protected virtual Task OnPaused()
        {
            if (this.Paused == null)
            {
                return Task.CompletedTask;
            }
            var e = new AsyncEventArgs();
            this.Paused(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Paused = delegate { };

        public abstract Task Resume();

        protected virtual Task OnResumed()
        {
            if (this.Resumed == null)
            {
                return Task.CompletedTask;
            }
            var e = new AsyncEventArgs();
            this.Resumed(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Resumed = delegate { };

        public abstract Task Stop();

        protected virtual Task OnStopping()
        {
            if (this.Stopping == null)
            {
                return Task.CompletedTask;
            }
            var e = new AsyncEventArgs();
            this.Stopping(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Stopping = delegate { };

        protected virtual Task OnStopped(bool manual)
        {
            if (this.Stopped == null)
            {
                return Task.CompletedTask;
            }
            var e = new StoppedEventArgs(manual);
            this.Stopped(this, e);
            return e.Complete();
        }

        public event StoppedEventHandler Stopped = delegate { };

        public virtual Task BeginSeek()
        {
            if (!this.IsPlaying)
            {
                return Task.CompletedTask;
            }
            return this.Stop();
        }

        public virtual Task EndSeek()
        {
            if (!this.IsStopped)
            {
                return Task.CompletedTask;
            }
            return this.Play();
        }

        protected virtual async Task EmitState()
        {
            await this.OnIsPlayingChanged();
            await this.OnIsPausedChanged();
            await this.OnIsStoppedChanged();
        }

        public string Description
        {
            get
            {
                return string.Format("Length = {0},  Rate {1}, Channels = {2}", this.Length, this.Rate, this.Channels);
            }
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

        protected abstract void OnDisposing();

        ~OutputStream()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
