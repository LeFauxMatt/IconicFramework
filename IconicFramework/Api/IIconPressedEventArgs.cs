namespace LeFauxMods.IconicFramework.Api;

/// <summary>Represents the event arguments for a toolbar icon being pressed.</summary>
public interface IIconPressedEventArgs
{
    /// <summary>Gets the button that was pressed.</summary>
    SButton Button { get; }

    /// <summary>Gets the id of the icon that was pressed.</summary>
    string Id { get; }
}
