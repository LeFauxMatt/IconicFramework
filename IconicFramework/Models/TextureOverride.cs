using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Models;

/// <summary>Represents texture overrides for icons.</summary>
internal sealed class TextureOverride
{
    /// <summary>Gets or sets the texture path.</summary>
    public string Texture { get; set; } = string.Empty;

    /// <summary>Gets or sets the source rectangle.</summary>
    public Rectangle SourceRect { get; set; } = Rectangle.Empty;
}