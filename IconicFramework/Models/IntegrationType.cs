namespace LeFauxMods.IconicFramework.Models;

using NetEscapades.EnumGenerators;
using StardewValley.Menus;

/// <summary>The type of mod integration.</summary>
[EnumExtensions]
internal enum IntegrationType
{
    /// <summary>Opens an <see cref="IClickableMenu" /> from the mod.</summary>
    Menu = 0,

    /// <summary>Invokes a method from the mod.</summary>
    Method = 1,

    /// <summary>Issue a keybind.</summary>
    Keybind = 2
}
