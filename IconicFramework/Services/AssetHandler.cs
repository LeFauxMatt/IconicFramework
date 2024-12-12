namespace LeFauxMods.IconicFramework.Services;

using Common.Integrations.FauxCore;
using Microsoft.Xna.Framework.Graphics;
using Models;
using StardewModdingAPI.Events;

internal sealed class AssetHandler
{
    private static AssetHandler? instance;

    private readonly FauxCoreIntegration fauxCoreIntegration;
    private readonly IModHelper helper;

    private AssetHandler(IModHelper helper)
    {
        this.helper = helper;
        this.fauxCoreIntegration = new FauxCoreIntegration(this.helper.ModRegistry);

        // Events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;

        if (this.fauxCoreIntegration.IsLoaded)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }
    }

    public static AssetHandler Init(IModHelper helper) => instance ??= new AssetHandler(helper);

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(Constants.DataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ContentPackData>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }

        if (this.fauxCoreIntegration.IsLoaded)
        {
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo(Constants.IconPath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }

        if (e.NameWithoutLocale.IsEquivalentTo(Constants.UIPath))
        {
            e.LoadFromModFile<Texture2D>("assets/ui.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (!this.fauxCoreIntegration.IsLoaded)
        {
            return;
        }

        this.fauxCoreIntegration.Api.AddAsset(Constants.IconPath,
            this.helper.ModContent.Load<IRawTextureData>("assets/icons.png"));

        this.fauxCoreIntegration.Api.AddAsset(Constants.UIPath,
            this.helper.ModContent.Load<IRawTextureData>("assets/ui.png"));
    }
}
