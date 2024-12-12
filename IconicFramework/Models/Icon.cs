namespace LeFauxMods.IconicFramework.Models;

using Microsoft.Xna.Framework;

/// <summary>
///     Creates a new instance of the <see cref="Icon" /> class.
/// </summary>
/// <param name="uniqueId">The icon's unique id.</param>
/// <param name="texturePath">The path to the texture icon.</param>
/// <param name="sourceRect">The source rectangle of the icon.</param>
/// <param name="hoverText">Text to appear when hovering over the icon.</param>
internal sealed class Icon(string uniqueId, string texturePath, Rectangle? sourceRect, string? hoverText)
{
    /// <summary>Gets the icon's hover text.</summary>
    public string HoverText { get; } = hoverText ?? string.Empty;

    /// <summary>Gets the icon's source rectangle.</summary>
    public Rectangle SourceRect { get; } = sourceRect ?? Rectangle.Empty;

    /// <summary>Gets the path to the icon's texture.</summary>
    public string TexturePath { get; } = texturePath;

    /// <summary>Gets the icon's unique id.</summary>
    public string UniqueId { get; } = uniqueId;
}
