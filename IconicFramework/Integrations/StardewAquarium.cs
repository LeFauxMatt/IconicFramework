namespace LeFauxMods.IconicFramework.Integrations;

using Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using Utilities;

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

        var method = reflection.GetMethod(mod, "OpenAquariumCollectionMenu", false);
        if (method is null)
        {
            return;
        }

        api.AddToolbarIcon(Id, Constants.IconPath, new Rectangle(0, 0, 16, 16), I18n.Button_StardewAquarium());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                method.Invoke("aquariumprogress", Array.Empty<string>());
            }
        });
    }
}
