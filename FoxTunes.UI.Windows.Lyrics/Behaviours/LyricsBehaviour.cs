﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LyricsBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "BB46B834-5372-440F-B75B-57FF0E473BB4";

        public const string EDIT = "AAAA";

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement AutoScroll { get; private set; }

        public BooleanConfigurationElement AutoLookup { get; private set; }

        public TextConfigurationElement Editor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataManager = core.Managers.MetaData;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            );
            this.AutoScroll = this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_SCROLL
            );
            this.AutoLookup = this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_LOOKUP
            );
            this.Editor = this.Configuration.GetElement<TextConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.EDITOR
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    if (this.PlaybackManager.CurrentStream != null)
                    {
                        yield return new InvocationComponent(
                            CATEGORY,
                            EDIT,
                            "Edit"
                        );
                    }
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.AutoScroll.Id,
                        this.AutoScroll.Name,
                        attributes: (byte)(InvocationComponent.ATTRIBUTE_SEPARATOR | (this.AutoScroll.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE))
                    );
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.AutoLookup.Id,
                        this.AutoLookup.Name,
                        attributes: this.AutoLookup.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case EDIT:
                    return this.Edit();
            }
            if (string.Equals(component.Id, this.AutoScroll.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.AutoScroll.Toggle();
            }
            else if (string.Equals(component.Id, this.AutoLookup.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.AutoLookup.Toggle();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task Edit()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                return;
            }
            var playlistItem = outputStream.PlaylistItem;
            var metaDataItem = default(MetaDataItem);
            lock (playlistItem.MetaDatas)
            {
                metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                    element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                );
            }
            var fileName = Path.GetTempFileName();
            Logger.Write(this, LogLevel.Debug, "Editing lyrics for file \"{0}\": \"{1}\"", playlistItem.FileName, fileName);
            if (metaDataItem != null)
            {
                File.WriteAllText(fileName, metaDataItem.Value);
            }
            else
            {
                File.WriteAllText(fileName, string.Empty);
            }
            var args = string.Format("\"{0}\"", fileName);
            Logger.Write(this, LogLevel.Debug, "Using editor: \"{0}\"", this.Editor.Value);
            var process = Process.Start(this.Editor.Value, args);
            await process.WaitForExitAsync().ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                Logger.Write(this, LogLevel.Warn, "Process does not indicate success: Code = {0}", process.ExitCode);
                return;
            }
            if (metaDataItem == null)
            {
                metaDataItem = new MetaDataItem(CommonMetaData.Lyrics, MetaDataItemType.Tag);
                lock (playlistItem.MetaDatas)
                {
                    playlistItem.MetaDatas.Add(metaDataItem);
                }
            }
            metaDataItem.Value = File.ReadAllText(fileName);
            await this.MetaDataManager.Save(
                new[] { playlistItem },
                true,
                false,
                CommonMetaData.Lyrics
            ).ConfigureAwait(false);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LyricsBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
