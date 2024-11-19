namespace LeFauxMods.IconicFramework.States;

using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using StardewModdingAPI.Events;

internal sealed class TitleMenu : IDisposable
{
    private readonly ModConfig config;
    private readonly EventManager eventManager;
    private readonly IModHelper helper;
    private readonly Dictionary<string, Icon> icons;
    private GameWorld? gameWorld;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleMenu"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="config"></param>
    /// <param name="eventManager"></param>
    /// <param name="manifest"></param>
    /// <param name="icons"></param>
    public TitleMenu(IModHelper helper, ModConfig config, EventManager eventManager, IManifest manifest, Dictionary<string, Icon> icons)
    {
        // Init
        this.helper = helper;
        this.config = config;
        this.eventManager = eventManager;
        this.icons = icons;

        // Events
        this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        this.helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.gameWorld?.Dispose();
        this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
        this.helper.Events.GameLoop.ReturnedToTitle -= this.OnReturnedToTitle;
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.gameWorld?.Dispose();
        this.gameWorld = null;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) =>
        this.gameWorld = new GameWorld(this.helper, this.config, this.eventManager, this.icons);
}
