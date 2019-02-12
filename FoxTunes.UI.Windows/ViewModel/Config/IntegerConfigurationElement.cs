﻿using System;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel.Config
{
    public class IntegerConfigurationElement : ViewModelBase
    {
        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(
           "Element",
           typeof(global::FoxTunes.IntegerConfigurationElement),
           typeof(IntegerConfigurationElement),
           new PropertyMetadata(new PropertyChangedCallback(OnElementChanged))
       );

        public static global::FoxTunes.IntegerConfigurationElement GetElement(ViewModelBase source)
        {
            return (global::FoxTunes.IntegerConfigurationElement)source.GetValue(ElementProperty);
        }

        public static void SetElement(ViewModelBase source, global::FoxTunes.IntegerConfigurationElement value)
        {
            source.SetValue(ElementProperty, value);
        }

        public static void OnElementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = sender as IntegerConfigurationElement;
            if (viewModel == null)
            {
                return;
            }
            viewModel.OnElementChanged();
        }

        public global::FoxTunes.IntegerConfigurationElement Element
        {
            get
            {
                return this.GetValue(ElementProperty) as global::FoxTunes.IntegerConfigurationElement;
            }
            set
            {
                this.SetValue(ElementProperty, value);
            }
        }

        protected virtual void OnElementChanged()
        {
            this.OnMinValueChanged();
            this.OnMaxValueChanged();
            this.OnSmallChangeChanged();
            this.OnLargeChangeChanged();
            if (this.ElementChanged != null)
            {
                this.ElementChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Element");
        }

        public event EventHandler ElementChanged;

        public int MinValue
        {
            get
            {
                if (this.Element != null && this.Element.ValidationRules != null)
                {
                    foreach (var validationRule in this.Element.ValidationRules.OfType<IntegerValidationRule>())
                    {
                        return validationRule.MinValue;
                    }
                }
                return int.MinValue;
            }
        }

        protected virtual void OnMinValueChanged()
        {
            if (this.MinValueChanged != null)
            {
                this.MinValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MinValue");
        }

        public event EventHandler MinValueChanged;

        public int MaxValue
        {
            get
            {
                if (this.Element != null && this.Element.ValidationRules != null)
                {
                    foreach (var validationRule in this.Element.ValidationRules.OfType<IntegerValidationRule>())
                    {
                        return validationRule.MaxValue;
                    }
                }
                return int.MaxValue;
            }
        }

        protected virtual void OnMaxValueChanged()
        {
            if (this.MaxValueChanged != null)
            {
                this.MaxValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MaxValue");
        }

        public event EventHandler MaxValueChanged;

        public int SmallChange
        {
            get
            {
                if (this.Element != null && this.Element.ValidationRules != null)
                {
                    foreach (var validationRule in this.Element.ValidationRules.OfType<IntegerValidationRule>())
                    {
                        return ((validationRule.MaxValue - validationRule.MinValue) / 10).ToNearestPower();
                    }
                }
                return 1;
            }
        }

        protected virtual void OnSmallChangeChanged()
        {
            if (this.SmallChangeChanged != null)
            {
                this.SmallChangeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SmallChange");
        }

        public event EventHandler SmallChangeChanged;

        public int LargeChange
        {
            get
            {
                return this.SmallChange * 2;
            }
        }

        protected virtual void OnLargeChangeChanged()
        {
            if (this.LargeChangeChanged != null)
            {
                this.LargeChangeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LargeChange");
        }

        public event EventHandler LargeChangeChanged;

        protected override Freezable CreateInstanceCore()
        {
            return new IntegerConfigurationElement();
        }
    }
}
