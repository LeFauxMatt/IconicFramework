using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Integrations.RadialMenu;
using LeFauxMods.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Models;

/// <summary>Creates a new instance of the <see cref="IconComponent" /> class.</summary>
/// <param name="uniqueId">The icon's unique id.</param>
/// <param name="texture">The icon's texture.</param>
/// <param name="sourceRectangle">The icon's source rectangle.</param>
/// <param name="getTitle">Text to appear as the title in the Radial Menu.</param>
/// <param name="getDescription">Text to appear when hovering over the icon.</param>
/// <param name="scale">The icon's scale.</param>
internal sealed class IconComponent(
    string uniqueId,
    Texture2D texture,
    Rectangle sourceRectangle,
    Func<string>? getTitle,
    Func<string>? getDescription,
    float scale) : ClickableTextureComponent(
        uniqueId,
        Rectangle.Empty,
        getTitle?.Invoke(),
        getDescription?.Invoke(),
        texture,
        sourceRectangle,
        scale),
    IRadialMenuItem
{
    /// <inheritdoc />
    public string Description => string.IsNullOrWhiteSpace(this.hoverText) ? this.label : this.hoverText;

    /// <inheritdoc />
    public Rectangle? SourceRectangle => this.sourceRect;

    /// <inheritdoc />
    public Texture2D? Texture => this.texture;

    /// <inheritdoc />
    public string Title => this.label;

    /// <inheritdoc />
    public MenuItemActivationResult Activate(Farmer who, DelayedActions delayedActions, MenuItemAction requestedAction)
    {
        if (delayedActions != DelayedActions.None)
        {
            return MenuItemActivationResult.Delayed;
        }

        ModEvents.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
            new IconPressedEventArgs(this.name, SButton.MouseLeft));

        return MenuItemActivationResult.Selected;
    }
}
