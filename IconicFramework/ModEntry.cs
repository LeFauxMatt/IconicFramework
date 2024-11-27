namespace LeFauxMods.IconicFramework;

using System;
using LeFauxMods.IconicFramework.Integrations;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Services;
using LeFauxMods.IconicFramework.Utilities;
using StardewModdingAPI.Events;

/// <inheritdoc/>
internal sealed class ModEntry : Mod
{
    private readonly Dictionary<string, Icon> icons = [];
    private ModConfig config = null!;

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        _ = new Log(this.Monitor);
        this.config = helper.ReadConfig<ModConfig>();
        _ = new IntegrationHelper(this.Helper.ModRegistry, this.Helper.Reflection);
        ThemeHelper.AddAsset(Constants.IconPath, this.Helper.ModContent.Load<IRawTextureData>("assets/icons.png"));
        ThemeHelper.AddAsset(Constants.UIPath, this.Helper.ModContent.Load<IRawTextureData>("assets/ui.png"));

        // Integrations
        var modInfo = this.Helper.ModRegistry.Get(this.ModManifest.UniqueID)!;
        var api = new ModApi(modInfo, this.icons);
        _ = new AlwaysScrollMap(api, this.Helper.Reflection);
        _ = new Calendar(api);
        _ = new CjbCheatsMenu(api, this.Helper.Reflection);
        _ = new CjbItemSpawner(api, this.Helper.Reflection);
        _ = new ContentPack(this.Helper, api);
        _ = new DailyQuests(api);
        _ = new GenericModConfigMenu(api, this.Helper.Reflection);
        _ = new SpecialOrders(api);
        _ = new StardewAquarium(api, this.Helper.Reflection);
        _ = new ToDew(api);
        _ = new ToggleCollisions(api);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc/>
    public override object? GetApi(IModInfo mod) => new ModApi(mod, this.icons);

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(Constants.DataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ContentPackData>(StringComparer.OrdinalIgnoreCase), AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) =>
        _ = new PlayerOverlay(this.Helper, this.config, this.ModManifest, this.icons);

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) =>
        Game1.onScreenMenus.Add(new ToolbarOverlay(this.Helper, this.config, this.icons));
}
