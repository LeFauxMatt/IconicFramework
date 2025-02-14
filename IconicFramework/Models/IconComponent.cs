using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Integrations.StarControl;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Models;

internal sealed class IconComponent : ClickableTextureComponent, IRadialMenuItem
{
    private readonly Lazy<IconConfig?> config;

    /// <summary>Creates a new instance of the <see cref="IconComponent" /> class.</summary>
    /// <param name="uniqueId">The icon's unique id.</param>
    /// <param name="texture">The icon's texture.</param>
    /// <param name="sourceRectangle">The icon's source rectangle.</param>
    /// <param name="getTitle">Text to appear as the title in the Radial Menu.</param>
    /// <param name="getDescription">Text to appear when hovering over the icon.</param>
    /// <param name="scale">The icon's scale.</param>
    public IconComponent(
        string uniqueId,
        Texture2D texture,
        Rectangle sourceRectangle,
        Func<string>? getTitle,
        Func<string>? getDescription,
        float scale) : base(
        uniqueId,
        Rectangle.Empty,
        getTitle?.Invoke(),
        getDescription?.Invoke(),
        texture,
        sourceRectangle,
        scale) =>
        this.config = new Lazy<IconConfig?>(() =>
            ModState.Config.Icons.FirstOrDefault(iconConfig => iconConfig.Id == uniqueId));

    /// <inheritdoc />
    public string Description => string.IsNullOrWhiteSpace(this.hoverText) ? this.label : this.hoverText;

    /// <inheritdoc />
    public bool Enabled => this.config.Value?.ShowRadial ?? false;

    /// <inheritdoc />
    public string Id => this.name;

    /// <inheritdoc />
    public Rectangle? SourceRectangle => this.sourceRect;

    /// <inheritdoc />
    public Texture2D? Texture => this.texture;

    /// <inheritdoc />
    public string Title => this.label;

    public ItemActivationResult Activate(Farmer who, DelayedActions delayedActions,
        ItemActivationType activationType = ItemActivationType.Primary)
    {
        if (delayedActions != DelayedActions.None)
        {
            return ItemActivationResult.Delayed;
        }

        switch (activationType)
        {
            case ItemActivationType.Secondary:
                ModEvents.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
                    new IconPressedEventArgs(this.name, SButton.ControllerA));
                break;

            default:
                ModEvents.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
                    new IconPressedEventArgs(this.name, SButton.ControllerB));
                break;
        }

        return ItemActivationResult.Selected;
    }
}