namespace LeFauxMods.IconicFramework.Integrations;

using Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using Utilities;

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

        var enabledIndoors = reflection.GetField<bool>(config, "EnabledIndoors", false);
        var enabledOutdoors = reflection.GetField<bool>(config, "EnabledOutdoors", false);

        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(32, 16, 16, 16),
            I18n.Button_AlwaysScrollMap_Title,
            I18n.Button_AlwaysScrollMap_Description);

        api.Subscribe(
            e =>
            {
                if (e.Id == Id)
                {
                    if (Game1.currentLocation.IsOutdoors)
                    {
                        enabledOutdoors.SetValue(!enabledOutdoors.GetValue());
                    }
                    else
                    {
                        enabledIndoors.SetValue(!enabledIndoors.GetValue());
                    }
                }
            });
    }
}
