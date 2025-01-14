namespace LeFauxMods.IconicFramework.Models;

/// <summary>IconComponent config option.</summary>
internal sealed class IconConfig
{
    /// <summary>Gets or sets the icon id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the icon will be shown on the radial menu.</summary>
    public bool ShowRadial { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the icon will be shown on the toolbar.</summary>
    public bool ShowToolbar { get; set; } = true;
}