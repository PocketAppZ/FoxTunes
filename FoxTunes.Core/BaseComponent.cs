﻿using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BaseComponent : IBaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public bool IsInitialized { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.IsInitialized = true;
        }

        protected virtual void Dispatch(Action action)
        {
#if NET40
            var task = TaskEx.Run(() =>
#else
            var task = Task.Run(() =>
#endif
            {
                try
                {
                    action();
                }
                catch
                {
                    //Nothing can be done, never throw on background thread.
                }
            });
        }

        protected virtual void Dispatch(Func<Task> function)
        {
#if NET40
            var task = TaskEx.Run(() =>
#else
            var task = Task.Run(() =>
#endif
            {
                try
                {
                    return function();
                }
                catch
                {
                    //Nothing can be done, never throw on background thread.
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            });
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual Task OnError(string message)
        {
            return this.OnError(new Exception(message));
        }

        protected virtual Task OnError(Exception exception)
        {
            return this.OnError(exception.Message, exception);
        }

        protected virtual Task OnError(string message, Exception exception)
        {
            return this.OnError(this, new ComponentErrorEventArgs(message, exception));
        }

        protected virtual Task OnError(object sender, ComponentErrorEventArgs e)
        {
            Logger.Write(this, LogLevel.Error, e.Message);
            if (this.Error == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Error(sender, e);
        }

        public event ComponentErrorEventHandler Error;
    }
}
