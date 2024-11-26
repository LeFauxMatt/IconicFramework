namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

/// <summary>Mod integration with Generic Mod Config Menu.</summary>
internal sealed class GenericModConfigMenu
{
    private const string Id = "spacechase0.GenericModConfigMenu";

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericModConfigMenu"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public GenericModConfigMenu(IIconicFrameworkApi api, IReflectionHelper reflection)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        var method = reflection.GetMethod(mod, "OpenListMenu", false);
        if (method is null)
        {
            return;
        }

        api.AddToolbarIcon(Id, Constants.IconPath, new Rectangle(16, 0, 16, 16), I18n.Button_GenericModConfigMenu());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                method.Invoke([0]);
            }
        });
    }
}
