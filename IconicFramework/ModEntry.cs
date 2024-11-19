namespace LeFauxMods.IconicFramework;

using System;
using LeFauxMods.IconicFramework.Integrations;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.States;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

/// <inheritdoc/>
internal sealed class ModEntry : Mod
{
    private readonly EventManager eventManager = new();
    private readonly Dictionary<string, Icon> icons = [];
    private ModConfig config = null!;

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        _ = new Log(this.Monitor);
        this.config = helper.ReadConfig<ModConfig>();
        _ = new IntegrationHelper(this.Helper.ModRegistry);

        // Integrations
        var modInfo = this.Helper.ModRegistry.Get(this.ModManifest.UniqueID)!;
        var api = new ModApi(modInfo, this.eventManager, this.icons);
        _ = new AlwaysScrollMap(api, this.Helper.Reflection);
        _ = new Calendar(api);
        _ = new CjbCheatsMenu(api, this.Helper.Reflection);
        _ = new CjbItemSpawner(api, this.Helper.Reflection);
        _ = new ContentPack(api);
        _ = new DailyQuests(api);
        _ = new GenericModConfigMenu(api, this.Helper.Reflection);
        _ = new SpecialOrders(api);
        _ = new StardewAquarium(api, this.Helper.Reflection);
        _ = new ToDew(api);
        _ = new ToggleCollisions(api);

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
    }

    /// <inheritdoc/>
    public override object? GetApi(IModInfo mod) => new ModApi(mod, this.eventManager, this.icons);

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("furyx639.ToolbarIcons/Data"))
        {
            e.LoadFrom(static () => new Dictionary<string, ContentPackData>(StringComparer.OrdinalIgnoreCase), AssetLoadPriority.Exclusive);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("furyx639.ToolbarIcons/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) =>
        _ = new TitleMenu(this.Helper, this.config, this.eventManager, this.ModManifest, this.icons);
}
