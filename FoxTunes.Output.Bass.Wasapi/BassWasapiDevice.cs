﻿using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FoxTunes
{
    public static class BassWasapiDevice
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static BassWasapiDevice()
        {
            //Perhaps we shouldn't Reset each time the output is started.
            //But what if the system config changes and the current device id isn't what we think it is?
            //var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            //configuration.GetElement<SelectionConfigurationElement>(
            //    BassOutputConfiguration.SECTION,
            //    BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_DEVICE
            //).ConnectValue(value => Reset());
            //configuration.GetElement<CommandConfigurationElement>(
            //    BassOutputConfiguration.SECTION,
            //    BassWasapiStreamOutputConfiguration.ELEMENT_REFRESH
            //).Invoked += (sender, e) => Reset();
        }

        public static bool IsInitialized { get; private set; }

        public static bool IsDefaultDevice
        {
            get
            {
                return Info != null && Info.IsDefault;
            }
        }

        public static void Init(int device, bool exclusive, bool autoFormat, bool buffer, bool eventDriven, bool dither, int frequency, int channels, BassFlags flags)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Device is already initialized.");
            }

            IsInitialized = true;

            LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Initializing BASS WASAPI.");

            try
            {
                BassUtils.OK(
                    BassWasapiHandler.Init(
                        device,
                        frequency,
                        channels,
                        GetFlags(exclusive, autoFormat, buffer, eventDriven, dither)
                    )
                );

                LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Initialized BASS WASAPI.");
            }
            catch
            {
                Free();
                throw;
            }
        }

        public static void Detect(int device, bool exclusive, bool autoFormat, bool buffer, bool eventDriven, bool dither)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Device is already initialized.");
            }

            IsInitialized = true;

            LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detecting WASAPI device.");

            try
            {
                var flags = GetFlags(exclusive, autoFormat, buffer, eventDriven, dither);
                BassUtils.OK(
                    BassWasapiHandler.Init(
                        device,
                        0,
                        0,
                        flags
                    )
                );
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(BassWasapi.CurrentDevice, out deviceInfo));
                Info = new BassWasapiDeviceInfo(
                    BassWasapi.CurrentDevice,
                    deviceInfo.MixFrequency,
                    0,
                    deviceInfo.MixChannels,
                    GetSupportedFormats(
                        BassWasapi.CurrentDevice,
                        flags
                    ),
                    BassWasapi.CheckFormat(
                        BassWasapi.CurrentDevice,
                        deviceInfo.MixFrequency,
                        deviceInfo.MixChannels,
                        flags
                    ),
                    device == BassWasapi.DefaultDevice
                );

                LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detected WASAPI device: {0} => Inputs => {1}, Outputs = {2}, Rate = {3}, Format = {4}", BassWasapi.CurrentDevice, Info.Inputs, Info.Outputs, Info.Rate, Enum.GetName(typeof(WasapiFormat), Info.Format));
                LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detected WASAPI device: {0} => Rates => {1}", BassWasapi.CurrentDevice, string.Join(", ", Info.SupportedRates));
            }
            finally
            {
                Free();
            }
        }

        public static void Reset()
        {
            Info = null;
        }

        private static WasapiInitFlags GetFlags(bool exclusive, bool autoFormat, bool buffer, bool eventDriven, bool dither)
        {
            var flags = WasapiInitFlags.Shared;
            if (exclusive)
            {
                flags |= WasapiInitFlags.Exclusive;
            }
            if (autoFormat)
            {
                flags |= WasapiInitFlags.AutoFormat;
            }
            if (buffer)
            {
                flags |= WasapiInitFlags.Buffer;
            }
            if (eventDriven)
            {
                flags |= WasapiInitFlags.EventDriven;
            }
            if (dither)
            {
                flags |= WasapiInitFlags.Dither;
            }
            return flags;
        }

        private static IDictionary<int, WasapiFormat> GetSupportedFormats(int device, WasapiInitFlags flags)
        {
            var supportedFormats = new Dictionary<int, WasapiFormat>();
            foreach (var rate in OutputRate.PCM)
            {
                var format = BassWasapi.CheckFormat(device, rate, BassWasapi.Info.Channels, flags);
                if (format == WasapiFormat.Unknown)
                {
                    continue;
                }
                supportedFormats.Add(rate, format);
            }
            return supportedFormats;
        }

        public static void Free()
        {
            if (!IsInitialized)
            {
                return;
            }
            LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Releasing BASS WASAPI.");
            BassWasapi.Free();
            BassWasapiHandler.Free();
            IsInitialized = false;
        }

        public static BassWasapiDeviceInfo Info { get; private set; }

        public class BassWasapiDeviceInfo
        {
            public BassWasapiDeviceInfo(int device, int rate, int inputs, int outputs, IDictionary<int, WasapiFormat> supportedFormats, WasapiFormat format, bool isDefault)
            {
                this.Device = device;
                this.Rate = rate;
                this.Inputs = inputs;
                this.Outputs = outputs;
#if NET40
                this.SupportedFormats = new Dictionary<int, WasapiFormat>(supportedFormats);
#else
                this.SupportedFormats = new ReadOnlyDictionary<int, WasapiFormat>(supportedFormats);
#endif
                this.Format = format;
                this.IsDefault = isDefault;
            }

            public int Device { get; private set; }

            public int Rate { get; private set; }

            public int Inputs { get; private set; }

            public int Outputs { get; private set; }

            public IEnumerable<int> SupportedRates
            {
                get
                {
                    return this.SupportedFormats.Keys;
                }
            }

#if NET40
            public Dictionary<int, WasapiFormat> SupportedFormats { get; private set; }
#else
            public IReadOnlyDictionary<int, WasapiFormat> SupportedFormats { get; private set; }
#endif

            public WasapiFormat Format { get; private set; }

            public bool IsDefault { get; private set; }

            public int GetNearestRate(int rate)
            {
                //Find the closest supported rate.
                foreach (var supportedRate in this.SupportedRates)
                {
                    if (supportedRate >= rate)
                    {
                        return supportedRate;
                    }
                }
                //Ah. The minimum supported rate is not enough.
                return this.SupportedRates.LastOrDefault();
            }
        }
    }
}
