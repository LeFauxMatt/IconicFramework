namespace LeFauxMods.IconicFramework.Models;

using System;
using LeFauxMods.IconicFramework.Api;

/// <inheritdoc cref="IIconPressedEventArgs" />
/// <summary>Initializes a new instance of the <see cref="IconPressedEventArgs" /> class.</summary>
/// <param name="id">The icon id.</param>
/// <param name="button">The button.</param>
internal sealed class IconPressedEventArgs(string id, SButton button) : EventArgs, IIconPressedEventArgs
{
    /// <inheritdoc />
    public SButton Button { get; } = button;

    /// <inheritdoc />
    public string Id { get; } = id;
}
