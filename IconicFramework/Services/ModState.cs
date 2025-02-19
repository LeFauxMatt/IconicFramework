using LeFauxMods.Common.Integrations.StarControl;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class ModState
{
    private static ModState? Instance;
    private readonly ConfigHelper<ModConfig> configHelper;
    private readonly IModHelper helper;
    private readonly Dictionary<string, IconComponent> icons = new(StringComparer.OrdinalIgnoreCase);
    private StarControlIntegration? starControl;
    private Dictionary<string, TextureOverride>? textureOverrides;

    private ModState(IModHelper helper)
    {
        this.helper = helper;
        this.configHelper = new ConfigHelper<ModConfig>(helper);
        helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
    }

    public static ModConfig Config => Instance!.configHelper.Config;

    public static ConfigHelper<ModConfig> ConfigHelper => Instance!.configHelper;

    public static Dictionary<string, IconComponent> Icons => Instance!.icons;

    public static StarControlIntegration StarControl =>
        Instance!.starControl ??= new StarControlIntegration(Instance.helper.ModRegistry);

    public static Dictionary<string, TextureOverride> TextureOverrides =>
        Instance!.textureOverrides ??=
            Instance.helper.GameContent.Load<Dictionary<string, TextureOverride>>(ModConstants.TextureOverridesPath);

    public static void Init(IModHelper helper) => Instance ??= new ModState(helper);

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(static name => name.IsEquivalentTo(ModConstants.TextureOverridesPath)))
        {
            this.textureOverrides = null;
            this.helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }
    }

    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        this.helper.Events.GameLoop.UpdateTicking -= this.OnUpdateTicking;
        foreach (var (id, textureOverride) in TextureOverrides)
        {
            if (!Icons.TryGetValue(id, out var iconComponent))
            {
                continue;
            }

            try
            {
                iconComponent.texture = this.helper.GameContent.Load<Texture2D>(textureOverride.Texture);
                iconComponent.sourceRect = textureOverride.SourceRect;
            }
            catch
            {
                Log.Warn("Failed to apply texture override {0} for icon {1}.", textureOverride.Texture, id);
            }
        }
    }
}