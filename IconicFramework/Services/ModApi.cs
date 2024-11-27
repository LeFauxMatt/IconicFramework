namespace LeFauxMods.IconicFramework.Services;

using System;
using System.Collections.Generic;
using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.Core.Services;
using LeFauxMods.Core.Utilities;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

/// <inheritdoc/>
public sealed class ModApi : IIconicFrameworkApi
{
    private readonly EventManager eventManager = new();
    private readonly Dictionary<string, Icon> icons;
    private readonly Dictionary<string, string> ids = [];
    private readonly IModInfo mod;
    private EventHandler<string>? toolbarIconPressed;

    /// <summary>
    /// Creates a new instance of the <see cref="ModApi"/> class.
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="icons"></param>
    internal ModApi(IModInfo mod, Dictionary<string, Icon> icons)
    {
        this.mod = mod;
        this.icons = icons;

        // Events
        EventBus.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add
        {
            Log.WarnOnce(
                "{0} uses deprecated code. {1} event is deprecated. Please subscribe to the {2} event instead.",
                this.mod.Manifest.Name,
                nameof(this.ToolbarIconPressed),
                nameof(IIconPressedEventArgs));

            this.toolbarIconPressed += value;
        }
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc/>
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var uniqueId = $"{this.mod.Manifest.UniqueID}-{id}";
        if (!this.ids.TryAdd(uniqueId, id))
        {
            return;
        }

        this.icons.Add(uniqueId, new Icon(uniqueId, texturePath, sourceRect, hoverText));
        EventBus.Publish(ModSignal.IconsChanged);
    }

    /// <inheritdoc/>
    public void AddToolbarIcon(IIcon icon, string? hoverText)
    {
        if (!this.ids.TryAdd(icon.UniqueId, icon.Id))
        {
            return;
        }

        this.icons.Add(icon.UniqueId, new Icon(icon.UniqueId, icon.Path, icon.Area, hoverText));
    }

    /// <inheritdoc/>
    public void RemoveToolbarIcon(string id)
    {
        var uniqueId = $"{this.mod.Manifest.UniqueID}-{id}";
        if (!this.ids.ContainsKey(uniqueId))
        {
            return;
        }

        _ = this.ids.Remove(uniqueId);
        _ = this.icons.Remove(uniqueId);
        EventBus.Publish(ModSignal.IconsChanged);
    }

    /// <inheritdoc/>
    public void RemoveToolbarIcon(IIcon icon)
    {
        if (!this.ids.ContainsKey(icon.UniqueId))
        {
            return;
        }

        _ = this.ids.Remove(icon.UniqueId);
        _ = this.icons.Remove(icon.UniqueId);
        EventBus.Publish(ModSignal.IconsChanged);
    }

    /// <inheritdoc/>
    public void Subscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc/>
    public void Unsubscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (!this.ids.TryGetValue(e.Id, out var id))
        {
            return;
        }

        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, e.Button));
        if (this.toolbarIconPressed is null)
        {
            return;
        }

        foreach (var handler in this.toolbarIconPressed.GetInvocationList())
        {
            try
            {
                _ = handler.DynamicInvoke(this, id);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "{0} failed in {1}: {2}",
                    this.mod.Manifest.Name,
                    nameof(this.ToolbarIconPressed),
                    ex.Message);
            }
        }
    }
}
