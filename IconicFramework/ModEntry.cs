namespace LeFauxMods.IconicFramework;

using Common.Integrations.RadialMenu;
using Common.Services;
using Common.Utilities;
using Integrations;
using Models;
using Services;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using Utilities;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    private readonly Dictionary<string, IconComponent> icons = [];
    private ModConfig config = null!;
    private ConfigHelper<ModConfig> configHelper = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        this.configHelper = new ConfigHelper<ModConfig>(this.Helper);
        this.config = this.configHelper.Load();
        _ = new ConfigMenu(
            helper,
            this.ModManifest,
            this.config,
            this.configHelper,
            this.icons);

        I18n.Init(this.Helper.Translation);
        Log.Init(this.Monitor);

        var themeHelper = ThemeHelper.Init(this.Helper);
        themeHelper.AddAsset(Constants.IconPath, this.Helper.ModContent.Load<IRawTextureData>("assets/icons.png"));
        themeHelper.AddAsset(Constants.UiPath, this.Helper.ModContent.Load<IRawTextureData>("assets/ui.png"));

        // Integrations
        var modInfo = this.Helper.ModRegistry.Get(this.ModManifest.UniqueID)!;
        var api = new ModApi(modInfo, this.Helper, this.config, this.icons);
        _ = new IntegrationHelper(this.Helper.ModRegistry, this.Helper.Reflection);
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
        this.Helper.Events.Content.AssetRequested += OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi(mod, this.Helper, this.config, this.icons);

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(Constants.DataPath))
        {
            e.LoadFrom(
                static () => new Dictionary<string, ContentPackData>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }

    private static void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        foreach (var toolbarMenu in Game1.onScreenMenus.OfType<ToolbarMenu>())
        {
            toolbarMenu.Dispose();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var radialMenuIntegration = new RadialMenuIntegration(this.Helper.ModRegistry);
        if (radialMenuIntegration.IsLoaded)
        {
            _ = new RadialMenu(radialMenuIntegration.Api, this.ModManifest, this.config, this.icons);
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var index = Game1.onScreenMenus.IndexOf(Game1.onScreenMenus.FirstOrDefault(menu => menu is Toolbar));
        if (index != -1)
        {
            Game1.onScreenMenus.Insert(index, new ToolbarMenu(this.Helper, this.config, this.icons));
        }
    }
}
