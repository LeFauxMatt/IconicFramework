namespace LeFauxMods.IconicFramework.Models;

using Microsoft.Xna.Framework;

internal sealed class ContentPackData
{
    /// <summary>Gets or sets additional data depending on the integration type.</summary>
    public string ExtraData { get; set; } = string.Empty;

    /// <summary>Gets or sets the hover text.</summary>
    public string HoverText { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique id for the mod integration.</summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>Gets or sets the source rectangle.</summary>
    public Rectangle SourceRect { get; set; } = Rectangle.Empty;

    /// <summary>Gets or sets the texture path.</summary>
    public string TexturePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the integration type.</summary>
    public IntegrationType Type { get; set; }
}
