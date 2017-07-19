﻿using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class ScriptingContext : BaseComponent, IScriptingContext
    {
        public abstract void SetValue(string name, object value);

        public abstract object GetValue(string name);

        public abstract object Run(string script);

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
        }
    }
}