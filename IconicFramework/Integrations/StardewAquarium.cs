using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with Stardew Aquarium.</summary>
internal sealed class StardewAquarium
{
    private const string Id = "Cherry.StardewAquarium";

    /// <summary>
    ///     Initializes a new instance of the <see cref="StardewAquarium" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public StardewAquarium(IIconicFrameworkApi api, IReflectionHelper reflection)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        IReflectedMethod? method = null;
        try
        {
            method = reflection.GetMethod(mod, "OpenAquariumCollectionMenu", false);
        }
        catch
        {
            // ignored
        }

        if (method is null)
        {
            Log.WarnOnce("Integration with {0} failed to load method.", Id);
            return;
        }

        api.AddToolbarIcon(
            Id,
            ModConstants.IconPath,
            new Rectangle(0, 0, 16, 16),
            I18n.Button_StardewAquarium_Title,
            I18n.Button_StardewAquarium_Description,
            () => method.Invoke("aquariumprogress", Array.Empty<string>()));
    }
}