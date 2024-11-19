namespace LeFauxMods.IconicFramework;

using LeFauxMods.IconicFramework.Api;
using System;
using Microsoft.Xna.Framework;
using LeFauxMods.IconicFramework.Utilities;
using LeFauxMods.IconicFramework.Models;

/// <inheritdoc/>
public sealed class ModApi : IIconicFrameworkApi
{
    private readonly EventManager eventManager;
    private readonly Dictionary<string, Icon> icons;
    private readonly Dictionary<string, string> ids = [];
    private readonly IModInfo mod;
    private EventHandler<string>? toolbarIconPressed;

    /// <summary>
    /// Creates a new instance of the <see cref="ModApi"/> class.
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="eventManager"></param>
    /// <param name="icons"></param>
    internal ModApi(IModInfo mod, EventManager eventManager, Dictionary<string, Icon> icons)
    {
        this.mod = mod;
        this.eventManager = eventManager;
        this.icons = icons;

        // Events
        eventManager.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
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
        this.ids.Add(uniqueId, id);
        this.icons.Add(uniqueId, new Icon(texturePath, sourceRect, hoverText));
    }

    /// <inheritdoc/>
    public void RemoveToolbarIcon(string id)
    {
        var uniqueId = $"{this.mod.Manifest.UniqueID}-{id}";
        _ = this.ids.Remove(uniqueId);
        this.icons.Remove(uniqueId);
    }

    /// <inheritdoc/>
    public void Subscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc/>
    public void Unsubscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IIconPressedEventArgs args)
    {
        if (!this.ids.TryGetValue(args.Id, out var id))
        {
            return;
        }

        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, args.Button));

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
