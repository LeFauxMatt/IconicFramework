namespace LeFauxMods.IconicFramework.States;

using System;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Features;
using LeFauxMods.IconicFramework.Utilities;

internal sealed class GameWorld : IDisposable
{
    private readonly PlayerOverlay playerOverlay;
    private readonly ToolbarOverlay toolbarOverlay;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameWorld"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="config"></param>
    /// <param name="eventManager"></param>
    /// <param name="icons"></param>
    public GameWorld(IModHelper helper, ModConfig config, EventManager eventManager, Dictionary<string, Icon> icons)
    {
        // Init
        this.playerOverlay = new(helper, config, eventManager, icons);
        this.toolbarOverlay = new(helper, config, eventManager, icons);
        Game1.onScreenMenus.Add(this.playerOverlay);
        Game1.onScreenMenus.Add(this.toolbarOverlay);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Game1.onScreenMenus.Remove(this.playerOverlay);
        Game1.onScreenMenus.Remove(this.toolbarOverlay);
    }
}
