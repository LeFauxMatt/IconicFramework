namespace LeFauxMods.IconicFramework.Models;

/// <summary>Event arguments for when an icon is changed.</summary>
/// <param name="id">The id of the changed icon.</param>
internal sealed class IconChangedEventArgs(string id) : EventArgs
{
    /// <summary>Gets the changed icon's id.</summary>
    public string Id { get; } = id;
}
