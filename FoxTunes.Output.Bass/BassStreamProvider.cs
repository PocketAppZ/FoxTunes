﻿using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStreamProvider : StandardComponent, IBassStreamProvider
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public BassStreamProvider()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Streams = new ConcurrentDictionary<BassStreamProviderKey, byte[]>();
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public ConcurrentDictionary<BassStreamProviderKey, byte[]> Streams { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamFactory StreamFactory { get; private set; }

        public IBassStreamPipelineManager PipelineManager { get; private set; }

        public virtual byte Priority
        {
            get
            {
                return PRIORITY_NORMAL;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.StreamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            this.PipelineManager = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineManager>();
            if (this.StreamFactory != null)
            {
                this.StreamFactory.Register(this);
            }
            base.InitializeComponent(core);
        }

        public virtual bool CanCreateStream(PlaylistItem playlistItem)
        {
            return true;
        }

        public virtual async Task<int> CreateStream(PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode;
            if (this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync();
#endif
            try
            {
                if (this.Output.PlayFromMemory)
                {
                    var buffer = await this.GetBuffer(playlistItem);
                    var channelHandle = Bass.CreateStream(buffer, 0, buffer.Length, flags);
                    if (channelHandle != 0)
                    {
                        if (!this.Streams.TryAdd(new BassStreamProviderKey(playlistItem.FileName, channelHandle), buffer))
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to pin handle of buffer for file \"{0}\". Playback may fail.", playlistItem.FileName);
                        }
                    }
                    return channelHandle;
                }
                return Bass.CreateStream(playlistItem.FileName, 0, 0, flags);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task<byte[]> GetBuffer(PlaylistItem playlistItem)
        {
            var buffer = default(byte[]);
            foreach (var key in this.Streams.Keys)
            {
                if (string.Equals(key.FileName, playlistItem.FileName, StringComparison.OrdinalIgnoreCase) && this.Streams.TryGetValue(key, out buffer))
                {
                    Logger.Write(this, LogLevel.Debug, "Recycling existing buffer of {0} bytes from file \"{0}\".", buffer.Length, playlistItem.FileName);
                    return buffer;
                }
            }
            Logger.Write(this, LogLevel.Debug, "Loading file \"{0}\" into memory.", playlistItem.FileName);
            using (var stream = File.OpenRead(playlistItem.FileName))
            {
                buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                Logger.Write(this, LogLevel.Debug, "Buffered {0} bytes from file \"{0}\".", buffer.Length, playlistItem.FileName);
            }
            return buffer;
        }

        public virtual void FreeStream(PlaylistItem playlistItem, int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
            if (this.Streams.TryRemove(new BassStreamProviderKey(playlistItem.FileName, channelHandle)))
            {
                Logger.Write(this, LogLevel.Debug, "Released handle of buffer for channel {0}.", channelHandle);
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

        protected virtual void OnDisposing()
        {
            this.Streams.Clear();
        }

        ~BassStreamProvider()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        public class BassStreamProviderKey : IEquatable<BassStreamProviderKey>
        {
            public BassStreamProviderKey(string fileName, int channelHandle)
            {
                this.FileName = fileName;
                this.ChannelHandle = channelHandle;
            }

            public string FileName { get; private set; }

            public int ChannelHandle { get; private set; }

            public virtual bool Equals(BassStreamProviderKey other)
            {
                if (other == null)
                {
                    return false;
                }
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }
                if (!string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (this.ChannelHandle != other.ChannelHandle)
                {
                    return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as BassStreamProviderKey);
            }

            public override int GetHashCode()
            {
                var hashCode = default(int);
                unchecked
                {
                    if (!string.IsNullOrEmpty(this.FileName))
                    {
                        hashCode += this.FileName.GetHashCode();
                    }
                    hashCode += this.ChannelHandle.GetHashCode();
                }
                return hashCode;
            }

            public static bool operator ==(BassStreamProviderKey a, BassStreamProviderKey b)
            {
                if ((object)a == null && (object)b == null)
                {
                    return true;
                }
                if ((object)a == null || (object)b == null)
                {
                    return false;
                }
                if (object.ReferenceEquals((object)a, (object)b))
                {
                    return true;
                }
                return a.Equals(b);
            }

            public static bool operator !=(BassStreamProviderKey a, BassStreamProviderKey b)
            {
                return !(a == b);
            }
        }
    }
}