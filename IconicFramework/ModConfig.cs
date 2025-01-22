using System.Globalization;
using System.Text;
using LeFauxMods.Common.Interface;
using LeFauxMods.Common.Models;
using LeFauxMods.IconicFramework.Models;
using StardewModdingAPI.Utilities;

namespace LeFauxMods.IconicFramework;

/// <inheritdoc cref="IModConfig{TConfig}" />
internal sealed class ModConfig : IModConfig<ModConfig>, IConfigWithLogAmount
{
    /// <summary>Gets or sets a value indicating whether secondary actions are enabled.</summary>
    public bool EnableSecondary { get; set; } = true;

    /// <summary>Gets or sets the icon data.</summary>
    public List<IconConfig> Icons { get; set; } = [];

    /// <summary>Gets or sets the icon size.</summary>
    public float IconSize { get; set; } = 32f;

    /// <summary>Gets or sets the space between icons.</summary>
    public float IconSpacing { get; set; } = 4f;

    /// <inheritdoc />
    public LogAmount LogAmount { get; set; }

    /// <summary>Gets or sets a value indicating whether to play a sound when an icon is pressed.</summary>
    public bool PlaySound { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether to show a tooltip when an icon is hovered.</summary>
    public bool ShowTooltip { get; set; } = true;

    /// <summary>Gets or sets the key to toggle icons on or off.</summary>
    public KeybindList ToggleKey { get; set; } = new(new Keybind(SButton.LeftControl, SButton.Tab));

    /// <summary>Gets or sets a value indicating whether icons should be visible.</summary>
    public bool Visible { get; set; } = true;

    /// <inheritdoc />
    public void CopyTo(ModConfig other)
    {
        other.Icons.Clear();
        foreach (var iconConfig in this.Icons)
        {
            other.Icons.Add(
                new IconConfig
                {
                    Id = iconConfig.Id, ShowRadial = iconConfig.ShowRadial, ShowToolbar = iconConfig.ShowToolbar
                });
        }

        other.EnableSecondary = this.EnableSecondary;
        other.IconSize = this.IconSize;
        other.IconSpacing = this.IconSpacing;
        other.LogAmount = this.LogAmount;
        other.PlaySound = this.PlaySound;
        other.ShowTooltip = this.ShowTooltip;
        other.ToggleKey = this.ToggleKey;
        other.Visible = this.Visible;
    }

    /// <inheritdoc />
    public string GetSummary() =>
        new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.EnableSecondary),25}: {this.EnableSecondary}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.IconSize),25}: {this.IconSize}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.IconSpacing),25}: {this.IconSpacing}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LogAmount),25}: {this.LogAmount}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.PlaySound),25}: {this.PlaySound}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ShowTooltip),25}: {this.ShowTooltip}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ToggleKey),25}: {this.ToggleKey}")
            .AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.Visible),25}: {this.Visible}")
            .ToString();
}