namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

/// <summary>Mod integration with CJB Cheats Menu.</summary>
internal sealed class CjbCheatsMenu
{
    private const string Id = "CJBok.CheatsMenu";

    /// <summary>
    /// Initializes a new instance of the <see cref="CjbCheatsMenu"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public CjbCheatsMenu(IIconicFrameworkApi api, IReflectionHelper reflection)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        var method = reflection.GetMethod(mod, "OpenCheatsMenu", false);
        if (method is null)
        {
            return;
        }

        api.AddToolbarIcon(Id, "furyx639.ToolbarIcons/Icons", new Rectangle(16, 16, 16, 16), I18n.Button_CheatsMenu());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                method.Invoke([0, true]);
            }
        });
    }
}