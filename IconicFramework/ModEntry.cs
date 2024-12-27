using LeFauxMods.Common.Integrations.ContentPatcher;
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

    private ToolbarMenu? toolbarMenu;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(helper.Translation);
        this.configHelper = new ConfigHelper<ModConfig>(this.Helper);
        this.config = this.configHelper.Load();
        Log.Init(this.Monitor, this.config);

        // Events
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

        var cp = new ContentPatcherIntegration(helper);
        if (cp.IsLoaded)
        {
            ModEvents.Subscribe<ConditionsApiReadyEventArgs>(_ => this.Initialize());
            return;
        }

        helper.Events.GameLoop.GameLaunched += (_, _) => this.Initialize();
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new ModApi(mod, this.Helper, this.config, this.icons);

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(Constants.DataPath))
        {
            e.LoadFrom(
                static () => new Dictionary<string, ContentData>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }

    private void Initialize()
    {
        var themeHelper = ThemeHelper.Init(this.Helper);
        themeHelper.AddAsset(Constants.IconPath, this.Helper.ModContent.Load<IRawTextureData>("assets/icons.png"));
        themeHelper.AddAsset(Constants.UiPath, this.Helper.ModContent.Load<IRawTextureData>("assets/ui.png"));

        var gmcm = new GenericModConfigMenuIntegration(this.ModManifest, this.Helper.ModRegistry);
        if (gmcm.IsLoaded)
        {
            _ = new ConfigMenu(
                this.Helper,
                this.ModManifest,
                this.config,
                this.configHelper,
                gmcm,
                this.icons);
        }

        var modInfo = this.Helper.ModRegistry.Get(this.ModManifest.UniqueID)!;
        var api = new ModApi(modInfo, this.Helper, this.config, this.icons);
        _ = new IntegrationHelper(this.Helper.ModRegistry, this.Helper.Reflection);
        _ = new AlwaysScrollMap(api, this.Helper.Reflection);
        _ = new Calendar(api);
        _ = new CjbCheatsMenu(api, this.Helper.Reflection);
        _ = new CjbItemSpawner(api, this.Helper.Reflection);
        _ = new ContentPatcher(this.Helper, api);
        _ = new DailyQuests(api);
        _ = new GenericModConfigMenu(api, gmcm, this.ModManifest, this.Helper.Reflection);
        _ = new SpecialOrders(api, this.Helper);
        _ = new StardewAquarium(api, this.Helper.Reflection);
        _ = new ToDew(api);
        _ = new ToggleCollisions(api);

        var radialMenuIntegration = new RadialMenuIntegration(this.Helper.ModRegistry);
        if (radialMenuIntegration.IsLoaded)
        {
            _ = new RadialMenu(radialMenuIntegration.Api, this.ModManifest, this.config, this.icons);
        }
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e) => this.toolbarMenu?.Dispose();

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var index = Game1.onScreenMenus.IndexOf(Game1.onScreenMenus.FirstOrDefault(menu => menu is Toolbar));
        if (index == -1)
        {
            return;
        }

        this.toolbarMenu = new ToolbarMenu(this.Helper, this.config, this.icons);
        Game1.onScreenMenus.Insert(index, this.toolbarMenu);
    }
}
