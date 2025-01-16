using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeFauxMods.IconicFramework;

/// <inheritdoc />
public sealed class ModApi : IIconicFrameworkApi
{
    private readonly EventManager eventManager = new();
    private readonly IModHelper helper;
    private readonly Dictionary<string, string> ids = [];
    private readonly IModInfo mod;
    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Creates a new instance of the <see cref="ModApi" /> class.</summary>
    /// <param name="mod">The mod information.</param>
    /// <param name="helper">Dependency for events, input, and content.</param>
    internal ModApi(IModInfo mod, IModHelper helper)
    {
        this.helper = helper;
        this.mod = mod;

        // Events
        ModEvents.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this.toolbarIconPressed += value;
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText) =>
        this.AddToolbarIcon(
            id,
            texturePath,
            sourceRect,
            () => hoverText ?? string.Empty,
            null);

    /// <inheritdoc />
    public void AddToolbarIcon(
        string id,
        string texturePath,
        Rectangle? sourceRect,
        Func<string>? getTitle,
        Func<string>? getDescription)
    {
        var uniqueId = string.IsNullOrWhiteSpace(id)
            ? this.mod.Manifest.UniqueID
            : $"{this.mod.Manifest.UniqueID}-{id}";

        IconComponent? icon;

        if (ModState.TextureOverrides.TryGetValue(uniqueId, out var textureOverride))
        {
            texturePath = textureOverride.Texture;
            sourceRect = textureOverride.SourceRect;
        }

        var texture = this.helper.GameContent.Load<Texture2D>(texturePath);
        var sourceRectangle = sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height);
        var scale = ModState.Config.IconSize * 0.75f / Math.Max(sourceRectangle.Width, sourceRectangle.Height);

        // Update previously registered icon
        if (!this.ids.TryAdd(uniqueId, id))
        {
            if (!ModState.Icons.TryGetValue(uniqueId, out icon))
            {
                return;
            }

            icon.texture = texture;
            icon.sourceRect = sourceRectangle;
            icon.label = getTitle?.Invoke() ?? icon.label;
            icon.hoverText = getDescription?.Invoke() ?? icon.hoverText;

            return;
        }

        icon = new IconComponent(
            uniqueId,
            texture,
            sourceRectangle,
            getTitle,
            getDescription,
            scale) { drawLabel = false };

        if (!ModState.Icons.TryAdd(uniqueId, icon))
        {
            return;
        }

        if (ModState.Config.Icons.All(iconConfig => iconConfig.Id != uniqueId))
        {
            ModState.Config.Icons.Add(new IconConfig { Id = uniqueId });
            ModEvents.Publish(new ConfigChangedEventArgs<ModConfig>(ModState.Config));
        }

        ModEvents.Publish(new IconChangedEventArgs(uniqueId));
    }

    /// <inheritdoc />
    public void AddToolbarIcon(
        string id,
        string texturePath,
        Rectangle? sourceRect,
        Func<string>? getTitle,
        Func<string>? getDescription,
        Action onClick,
        Action? onRightClick = null)
    {
        this.AddToolbarIcon(id, texturePath, sourceRect, getTitle, getDescription);
        this.Subscribe(e =>
        {
            if (e.Id != id)
            {
                return;
            }

            if (e.Button.IsActionButton() && onRightClick is not null)
            {
                onRightClick();
                return;
            }

            onClick();
        });
    }

    /// <inheritdoc />
    public void AddToolbarIcon(
        string texturePath,
        Rectangle? sourceRect,
        Func<string>? getTitle,
        Func<string>? getDescription,
        Action onClick,
        Action? onRightClick = null) =>
        this.AddToolbarIcon(string.Empty, texturePath, sourceRect, getTitle, getDescription, onClick, onRightClick);

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id)
    {
        var uniqueId = $"{this.mod.Manifest.UniqueID}-{id}";
        if (!this.ids.ContainsKey(uniqueId))
        {
            return;
        }

        _ = this.ids.Remove(uniqueId);
        if (ModState.Icons.Remove(uniqueId))
        {
            ModEvents.Publish(new IconChangedEventArgs(uniqueId));
        }
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon() => this.RemoveToolbarIcon(string.Empty);

    /// <inheritdoc />
    public void Subscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public void Unsubscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (!this.ids.TryGetValue(e.Id, out var id))
        {
            return;
        }

        var button = e.Button;
        if (button.IsActionButton() && !ModState.Config.EnableSecondary)
        {
            button = e.Button.TryGetController(out _)
                ? SButton.ControllerX
                : SButton.MouseLeft;
        }

        var iconPressedEventArgs = new IconPressedEventArgs(id, button);
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(iconPressedEventArgs);
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