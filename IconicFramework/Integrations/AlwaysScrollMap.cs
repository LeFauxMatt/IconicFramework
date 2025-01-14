using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with Always Scroll Map.</summary>
internal sealed class AlwaysScrollMap
{
    private const string Id = "bcmpinc.AlwaysScrollMap";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AlwaysScrollMap" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public AlwaysScrollMap(IIconicFrameworkApi api, IReflectionHelper reflection)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        var config = mod.GetType().GetField("config")?.GetValue(mod);
        if (config is null)
        {
            return;
        }

        IReflectedField<bool>? enabledIndoors = null;
        IReflectedField<bool>? enabledOutdoors = null;

        try
        {
            enabledIndoors = reflection.GetField<bool>(config, "EnabledIndoors", false);
            enabledOutdoors = reflection.GetField<bool>(config, "EnabledOutdoors", false);
        }
        catch
        {
            Log.WarnOnce("Integration with {0} failed to load method.", Id);
        }

        if (enabledIndoors is null || enabledOutdoors is null)
        {
            return;
        }

        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(32, 16, 16, 16),
            I18n.Button_AlwaysScrollMap_Title,
            I18n.Button_AlwaysScrollMap_Description,
            () =>
            {
                if (Game1.currentLocation.IsOutdoors)
                {
                    enabledOutdoors.SetValue(!enabledOutdoors.GetValue());
                }
                else
                {
                    enabledIndoors.SetValue(!enabledIndoors.GetValue());
                }
            });
    }
}