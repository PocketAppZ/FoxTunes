﻿using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID, ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class SystemTheme : ThemeBase
    {
        public const string ID = "3E9EFE8C-5245-4F8B-97D1-EB47CC70E373";

        public SystemTheme()
            : base(ID, "System")
        {

        }

        public ResourceDictionary ResourceDictionary { get; private set; }

        public override Stream ArtworkPlaceholder
        {
            get
            {
                return typeof(ExpressionDarkTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
            }
        }

        public override void Enable()
        {
            Application.Current.Resources.MergedDictionaries.Add(this.ResourceDictionary);
        }

        public override void Disable()
        {
            Application.Current.Resources.MergedDictionaries.Remove(this.ResourceDictionary);
        }

        public override void InitializeComponent(ICore core)
        {
            this.ResourceDictionary = new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
            };
            base.InitializeComponent(core);
        }
    }
}
