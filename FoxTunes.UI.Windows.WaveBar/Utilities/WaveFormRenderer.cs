﻿using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class WaveFormRenderer : RendererBase
    {
        public static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register(
            "Bitmap",
            typeof(WriteableBitmap),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnBitmapChanged))
        );

        public static WriteableBitmap GetBitmap(WaveFormRenderer source)
        {
            return (WriteableBitmap)source.GetValue(BitmapProperty);
        }

        public static void SetBitmap(WaveFormRenderer source, WriteableBitmap value)
        {
            source.SetValue(BitmapProperty, value);
        }

        public static void OnBitmapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnBitmapChanged();
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnWidthChanged))
        );

        public static double GetWidth(WaveFormRenderer source)
        {
            return (double)source.GetValue(WidthProperty);
        }

        public static void SetWidth(WaveFormRenderer source, double value)
        {
            source.SetValue(WidthProperty, value);
        }

        public static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnWidthChanged();
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
           "Height",
           typeof(double),
           typeof(WaveFormRenderer),
           new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnHeightChanged))
       );

        public static double GetHeight(WaveFormRenderer source)
        {
            return (double)source.GetValue(HeightProperty);
        }

        public static void SetHeight(WaveFormRenderer source, double value)
        {
            source.SetValue(HeightProperty, value);
        }

        public static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnHeightChanged();
        }

        public static readonly DependencyProperty Color1Property = DependencyProperty.Register(
            "Color1",
            typeof(Color),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnColor1Changed))
        );

        public static Color GetColor1(WaveFormRenderer source)
        {
            return (Color)source.GetValue(Color1Property);
        }

        public static void SetColor1(WaveFormRenderer source, Color value)
        {
            source.SetValue(Color1Property, value);
        }

        public static void OnColor1Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnColor1Changed();
        }

        public static readonly DependencyProperty Color2Property = DependencyProperty.Register(
            "Color2",
            typeof(Color),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnColor2Changed))
        );

        public static Color GetColor2(WaveFormRenderer source)
        {
            return (Color)source.GetValue(Color2Property);
        }

        public static void SetColor2(WaveFormRenderer source, Color value)
        {
            source.SetValue(Color2Property, value);
        }

        public static void OnColor2Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnColor2Changed();
        }

        public WaveFormRenderer()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public WaveFormGenerator.WaveFormGeneratorData GeneratorData { get; private set; }

        public WaveFormRendererData RendererData { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Amplitude { get; private set; }

        public WriteableBitmap Bitmap
        {
            get
            {
                return (WriteableBitmap)this.GetValue(BitmapProperty);
            }
            set
            {
                this.SetValue(BitmapProperty, value);
            }
        }

        protected virtual void OnBitmapChanged()
        {
            if (this.BitmapChanged != null)
            {
                this.BitmapChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bitmap");
        }

        public event EventHandler BitmapChanged;

        public double Width
        {
            get
            {
                return (double)this.GetValue(WidthProperty);
            }
            set
            {
                this.SetValue(WidthProperty, value);
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public double Height
        {
            get
            {
                return (double)this.GetValue(HeightProperty);
            }
            set
            {
                this.SetValue(HeightProperty, value);
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        public Color Color1
        {
            get
            {
                return (Color)this.GetValue(Color1Property);
            }
            set
            {
                this.SetValue(Color1Property, value);
            }
        }

        protected virtual void OnColor1Changed()
        {
            if (this.Color1Changed != null)
            {
                this.Color1Changed(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Color1");
        }

        public event EventHandler Color1Changed;

        public Color Color2
        {
            get
            {
                return (Color)this.GetValue(Color2Property);
            }
            set
            {
                this.SetValue(Color2Property, value);
            }
        }

        protected virtual void OnColor2Changed()
        {
            if (this.Color2Changed != null)
            {
                this.Color2Changed(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Color2");
        }

        public event EventHandler Color2Changed;

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.MODE_ELEMENT
            );
            this.Amplitude = this.Configuration.GetElement<IntegerConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.AMPLITUDE_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.Mode.ValueChanged += this.OnValueChanged;
            this.Amplitude.ValueChanged += this.OnValueChanged;
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                await this.CreateBitmap().ConfigureAwait(false);
                if (this.PlaybackManager.CurrentStream != null)
                {
                    await this.Update(this.PlaybackManager.CurrentStream).ConfigureAwait(false);
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.RefreshBitmap();
        }

        protected virtual async Task Update(IOutputStream stream)
        {
            var cached = default(bool);

#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync(LockTimeout.TimeSpan).ConfigureAwait(false);
#endif

            try
            {
                if (this.GeneratorData != null)
                {
                    this.GeneratorData.Updated -= this.OnUpdated;
                    if (this.GeneratorData.CancellationToken != null)
                    {
                        this.GeneratorData.CancellationToken.Cancel();
                    }
                }

                this.GeneratorData = null;
                this.RendererData = null;

                if (stream == null)
                {
                    return;
                }

                if (this.LoadGeneratorData(stream))
                {
                    cached = true;
                }
                else
                {
                    stream = await this.Output.Duplicate(stream).ConfigureAwait(false);

                    this.GeneratorData = WaveFormGenerator.Create(stream);
                    this.GeneratorData.Updated += this.OnUpdated;
                }

                await Windows.Invoke(
                    () => this.RendererData = Create(this.GeneratorData, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight)
                ).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }

            if (cached)
            {
                var task = this.Update();
            }
            else
            {
                try
                {
                    WaveFormGenerator.Populate(stream, this.GeneratorData);
                }
                finally
                {
                    stream.Dispose();
                }
            }
        }

        protected virtual bool LoadGeneratorData(IOutputStream stream)
        {
            var generatorData = default(WaveFormGenerator.WaveFormGeneratorData);
            if (WaveFormCache.TryLoad(stream, out generatorData))
            {
                this.GeneratorData = generatorData;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void OnUpdated(object sender, EventArgs e)
        {
            var task = this.Update();
        }

        protected virtual async Task CreateBitmap()
        {
            await Windows.Invoke(() =>
            {
                var width = this.Width;
                var height = this.Height;
                if (width == 0 || double.IsNaN(width) || height == 0 || double.IsNaN(height))
                {
                    //We need proper dimentions.
                    return;
                }

                var size = Windows.ActiveWindow.GetElementPixelSize(
                    width * this.ScalingFactor.Value,
                    height * this.ScalingFactor.Value
                );

                this.Semaphore.Wait();

                try
                {
                    this.Bitmap = new WriteableBitmap(
                        Convert.ToInt32(size.Width),
                        Convert.ToInt32(size.Height),
                        96,
                        96,
                        PixelFormats.Pbgra32,
                        null
                    );

                    if (this.GeneratorData != null)
                    {
                        this.RendererData = Create(this.GeneratorData, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight);
                    }
                }
                finally
                {
                    this.Semaphore.Release();
                }
            });

            var task = this.Update();
        }

        protected virtual void RefreshBitmap()
        {
            this.Semaphore.Wait();
            try
            {
                if (this.RendererData != null)
                {
                    this.RendererData.Available = 0;
                    this.RendererData.Position = 0;
                }
            }
            finally
            {
                this.Semaphore.Release();
            }

            var task = this.Update();
        }

        public async Task Render()
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);

#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif

            try
            {
                if (this.RendererData == null)
                {
                    return;
                }

                await Windows.Invoke(() =>
                {
                    bitmap = this.Bitmap;
                    success = bitmap.TryLock(LockTimeout);
                    if (!success)
                    {
                        return;
                    }
                    info = BitmapHelper.CreateRenderInfo(bitmap, this.Color1);
                }).ConfigureAwait(false);

                if (!success)
                {
                    //No bitmap or failed to establish lock.
                    return;
                }

                Render(
                    this.RendererData,
                    info,
                    WaveBarBehaviourConfiguration.GetMode(this.Mode.Value)
                );

                await Windows.Invoke(() =>
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    bitmap.Unlock();
                }).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

#if NET40
        public Task Update()
#else
        public async Task Update()
#endif
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                if (this.GeneratorData != null && this.RendererData != null)
                {
                    Update(
                        this.GeneratorData,
                        this.RendererData,
                        this.Amplitude.Value,
                        WaveBarBehaviourConfiguration.GetMode(this.Mode.Value)
                    );
                }
            }
            finally
            {
                this.Semaphore.Release();
            }

            var task = this.Render();

#if NET40
            return TaskEx.FromResult(false);
#endif
        }

        protected override Freezable CreateInstanceCore()
        {
            return new WaveFormRenderer();
        }

        protected override void OnDisposing()
        {
            if (this.Semaphore != null)
            {
                this.Semaphore.Dispose();
            }
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            if (this.GeneratorData != null)
            {
                this.GeneratorData.Updated -= this.OnUpdated;
            }
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.Mode != null)
            {
                this.Mode.ValueChanged -= this.OnValueChanged;
            }
            if (this.Amplitude != null)
            {
                this.Amplitude.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Update(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude, WaveFormRendererMode mode)
        {
            if (generatorData.Peak == 0)
            {
                return;
            }
            else if (generatorData.Peak != rendererData.Peak)
            {
                rendererData.Available = 0;
                rendererData.Peak = generatorData.Peak;
            }

            switch (mode)
            {
                case WaveFormRendererMode.Mono:
                    UpdateMono(generatorData, rendererData, amplitude);
                    break;
                case WaveFormRendererMode.Seperate:
                    UpdateSeperate(generatorData, rendererData, amplitude);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void UpdateMono(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude)
        {
            var center = rendererData.Height / 2.0f;
            var factor = (rendererData.Peak / 2.0f) * (10.0f - amplitude);

            var data = generatorData.Data;
            var elements = rendererData.Elements;

            while (rendererData.Available < rendererData.Capacity)
            {
                var valuePosition = rendererData.Available * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    break;
                }

                var x = rendererData.Available;
                var y = default(int);
                var width = 1;
                var height = default(int);

                var topValue = default(float);
                var bottomValue = default(float);
                for (var a = 0; a < rendererData.ValuesPerElement; a++)
                {
                    for (var b = 0; b < generatorData.Channels; b++)
                    {
                        topValue += Math.Abs(data[valuePosition + a, b].Min);
                        bottomValue += Math.Abs(data[valuePosition + a, b].Max);
                    }
                }
                topValue /= (rendererData.ValuesPerElement * generatorData.Channels);
                bottomValue /= (rendererData.ValuesPerElement * generatorData.Channels);

                topValue /= factor;
                bottomValue /= factor;

                topValue = Math.Min(topValue, 1);
                bottomValue = Math.Min(bottomValue, 1);

                y = Convert.ToInt32(center - (topValue * center));
                height = Convert.ToInt32((center - y) + (bottomValue * center));

                elements[rendererData.Available, 0].X = x;
                elements[rendererData.Available, 0].Y = y;
                elements[rendererData.Available, 0].Width = width;
                elements[rendererData.Available, 0].Height = height;

                rendererData.Available++;
            }
        }

        private static void UpdateSeperate(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude)
        {
            var factor = rendererData.Peak / (generatorData.Channels * 2) * (10.0f - amplitude);

            var data = generatorData.Data;
            var elements = rendererData.Elements;

            while (rendererData.Available < rendererData.Capacity)
            {
                var valuePosition = rendererData.Available * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    break;
                }

                var x = rendererData.Available;
                var waveHeight = rendererData.Height / generatorData.Channels;

                for (var channel = 0; channel < generatorData.Channels; channel++)
                {
                    var waveCenter = (waveHeight * channel) + (waveHeight / 2);

                    var y = default(int);
                    var width = 1;
                    var height = default(int);

                    var topValue = default(float);
                    var bottomValue = default(float);
                    for (var a = 0; a < rendererData.ValuesPerElement; a++)
                    {
                        topValue += Math.Abs(data[valuePosition + a, channel].Min);
                        bottomValue += Math.Abs(data[valuePosition + a, channel].Max);
                    }
                    topValue /= (rendererData.ValuesPerElement * generatorData.Channels);
                    bottomValue /= (rendererData.ValuesPerElement * generatorData.Channels);

                    topValue /= factor;
                    bottomValue /= factor;

                    topValue = Math.Min(topValue, 1);
                    bottomValue = Math.Min(bottomValue, 1);

                    y = Convert.ToInt32(waveCenter - (topValue * (waveHeight / 2)));
                    height = Convert.ToInt32((waveCenter - y) + (bottomValue * (waveHeight / 2)));

                    elements[rendererData.Available, channel].X = x;
                    elements[rendererData.Available, channel].Y = y;
                    elements[rendererData.Available, channel].Width = width;
                    elements[rendererData.Available, channel].Height = height;
                }

                rendererData.Available++;
            }
        }

        public static void Render(WaveFormRendererData rendererData, BitmapHelper.RenderInfo renderInfo, WaveFormRendererMode mode)
        {
            var elements = rendererData.Elements;

            if (rendererData.Position == 0 || rendererData.Available < rendererData.Position)
            {
                rendererData.Position = 0;
                BitmapHelper.Clear(renderInfo);
            }

            var channels = default(int);
            switch (mode)
            {
                case WaveFormRendererMode.Mono:
                    channels = 1;
                    break;
                case WaveFormRendererMode.Seperate:
                    channels = rendererData.Channels;
                    break;
                default:
                    throw new NotImplementedException();
            }

            for (; rendererData.Position < rendererData.Available; rendererData.Position++)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    BitmapHelper.DrawRectangle(
                        renderInfo,
                        elements[rendererData.Position, channel].X,
                        elements[rendererData.Position, channel].Y,
                        elements[rendererData.Position, channel].Width,
                        elements[rendererData.Position, channel].Height
                    );
                }
            }
        }

        public static WaveFormRendererData Create(WaveFormGenerator.WaveFormGeneratorData generatorData, int width, int height)
        {
            return new WaveFormRendererData()
            {
                Width = width,
                Height = height,
                ValuesPerElement = Math.Max(generatorData.Capacity / width, 1),
                Elements = new Int32Rect[width, generatorData.Channels],
                Channels = generatorData.Channels,
                Position = 0,
                Available = 0,
                Capacity = width,
                Peak = 0
            };
        }

        public class WaveFormRendererData
        {
            public int Width;

            public int Height;

            public int ValuesPerElement;

            public Int32Rect[,] Elements;

            public int Channels;

            public int Position;

            public int Available;

            public int Capacity;

            public float Peak;
        }
    }

    public enum WaveFormRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }
}
