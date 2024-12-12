namespace LeFauxMods.IconicFramework.Models;

/// <summary>
///     Signals for mod communication.
/// </summary>
internal enum ModSignal
{
    /// <summary>Mod configuration changed.</summary>
    ConfigChanged,

    /// <summary>Icons were added or removed.</summary>
    IconsChanged
}
