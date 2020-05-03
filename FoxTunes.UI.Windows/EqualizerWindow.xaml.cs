﻿using System.ComponentModel;
using System.Windows;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for EqualizerWindow.xaml
    /// </summary>
    public partial class EqualizerWindow : Window
    {
        public EqualizerWindow()
        {
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
            if (!global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds.Top;
                }
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.EqualizerWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}