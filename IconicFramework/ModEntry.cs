using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.Common.Integrations.RadialMenu;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Integrations;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Services;
using LeFauxMods.IconicFramework.Utilities;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    private readonly Dictionary<string, IconComponent> icons = [];
    private ModConfig config = null!;
    private ConfigHelper<ModConfig> configHelper = null!;
    private GenericModConfigMenuIntegration gmcm = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        this.configHelper = new ConfigHelper<ModConfig>(helper);
        this.config = this.configHelper.Load();
        this.gmcm = new GenericModConfigMenuIntegration(this.ModManifest, helper.ModRegistry);
        if (this.gmcm.IsLoaded)
        {
            _ = new ConfigMenu(
                helper,
                this.ModManifest,
                this.config,
                this.configHelper,
                this.gmcm,
                this.icons);
        }

        I18n.Init(helper.Translation);
        Log.Init(this.Monitor);

        var themeHelper = ThemeHelper.Init(helper);
        themeHelper.AddAsset(Constants.IconPath, helper.ModContent.Load<IRawTextureData>("assets/icons.png"));
        themeHelper.AddAsset(Constants.UiPath, helper.ModContent.Load<IRawTextureData>("assets/ui.png"));

        // Integrations
        var modInfo = this.Helper.ModRegistry.Get(this.ModManifest.UniqueID)!;
        var api = new ModApi(modInfo, helper, this.config, this.icons);
        _ = new IntegrationHelper(helper.ModRegistry, this.Helper.Reflection);
        _ = new AlwaysScrollMap(api, helper.Reflection);
        _ = new Calendar(api);
        _ = new CjbCheatsMenu(api, helper.Reflection);
        _ = new CjbItemSpawner(api, helper.Reflection);
        _ = new ContentPatcher(helper, api);
        _ = new DailyQuests(api);
        _ = new GenericModConfigMenu(api, this.gmcm, this.ModManifest, helper.Reflection);
        _ = new SpecialOrders(api, helper);
        _ = new StardewAquarium(api, helper.Reflection);
        _ = new ToDew(api);
        _ = new ToggleCollisions(api);

        // Events
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi(mod, this.Helper, this.config, this.icons);

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(Constants.DataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ContentData>(StringComparer.OrdinalIgnoreCase),
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
