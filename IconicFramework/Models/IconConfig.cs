namespace LeFauxMods.IconicFramework.Models;

/// <summary>A single Icon.</summary>
/// <remarks>Initializes a new instance of the <see cref="IconConfig" /> class.</remarks>
/// <param name="id">The id of the toolbar icon.</param>
/// <param name="enabled">Whether the toolbar icon is enabled.</param>
internal class IconConfig(string id, bool enabled = true)
{
    /// <summary>Gets or sets a value indicating whether the Toolbar Icon is enabled.</summary>
    public bool Enabled { get; set; } = enabled;

    /// <summary>Gets or sets the Id of the Toolbar Icon.</summary>
    public string Id { get; set; } = id;
}
