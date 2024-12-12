namespace LeFauxMods.IconicFramework.Models;

using StardewModdingAPI.Utilities;

/// <summary>Represents the mod's configuration.</summary>
internal sealed class ModConfig
{
    /// <summary>Gets or sets icon data.</summary>
    public List<IconConfig> Icons { get; set; } = [];

    /// <summary>Gets or sets the icon size.</summary>
    public float IconSize { get; set; } = 32f;

    /// <summary>Gets or sets the space between icons.</summary>
    public float IconSpacing { get; set; } = 4f;

    /// <summary>Gets or sets a value indicating whether to play a sound when an icon is pressed.</summary>
    public bool PlaySound { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether to show a tooltip when an icon is hovered.</summary>
    public bool ShowTooltip { get; set; } = true;

    /// <summary>Gets or sets the key to toggle icons on or off.</summary>
    public KeybindList ToggleKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.Tab));

    /// <summary>Gets or sets a value indicating whether icons should be visible.</summary>
    public bool Visible { get; set; } = true;
}
