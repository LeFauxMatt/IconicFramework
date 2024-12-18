namespace LeFauxMods.IconicFramework.Integrations;

using Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using Utilities;

/// <summary>Mod integration with CJB Cheats Menu.</summary>
internal sealed class CjbCheatsMenu
{
    private const string Id = "CJBok.CheatsMenu";

    /// <summary>
    ///     Initializes a new instance of the <see cref="CjbCheatsMenu" /> class.
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

        api.AddToolbarIcon(
            Id,
            "LooseSprites/Cursors",
            new Rectangle(346, 392, 8, 8),
            I18n.Button_CheatsMenu_Title,
            I18n.Button_CheatsMenu_Description);
        api.Subscribe(
            e =>
            {
                if (e.Id == Id)
                {
                    method.Invoke();
                }
            });
    }
}
