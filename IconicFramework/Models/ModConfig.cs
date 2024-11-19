namespace LeFauxMods.IconicFramework.Models;

using System.Collections.Generic;
using StardewModdingAPI.Utilities;

/// <summary>Represents the mod's configuration.</summary>
internal sealed class ModConfig
{
    /// <summary>Gets or sets a value containing the icons.</summary>
    public List<IconConfig> Icons { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether to play a sound when an icon is pressed.</summary>
    public bool PlaySound { get; set; } = true;

    /// <summary>Gets or sets the size that icons will be scaled to.</summary>
    public float Scale { get; set; } = 2;

    /// <summary>Gets or sets a value indicating whether to show a tooltip when an icon is hovered.</summary>
    public bool ShowTooltip { get; set; } = true;

    /// <summary>Gets or sets the key to toggle icons on or off.</summary>
    public KeybindList ToggleKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.Tab));

    /// <summary>Gets or sets a value indicating whether icons should be visible.</summary>
    public bool Visible { get; set; } = true;
}
