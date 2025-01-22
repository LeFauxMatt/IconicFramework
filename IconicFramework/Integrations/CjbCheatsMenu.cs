using System.Reflection;
using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with CJB Cheats Menu.</summary>
internal sealed class CjbCheatsMenu
{
    private const string Id = "CJBok.CheatsMenu";

    /// <summary>
    ///     Initializes a new instance of the <see cref="CjbCheatsMenu" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public CjbCheatsMenu(IIconicFrameworkApi api)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        MethodInfo? method = null;
        try
        {
            method = mod.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(static methodInfo =>
                    methodInfo.Name == "OpenCheatsMenu" && methodInfo.GetParameters().Length == 0);
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
            "LooseSprites/Cursors",
            new Rectangle(346, 392, 8, 8),
            I18n.Button_CheatsMenu_Title,
            I18n.Button_CheatsMenu_Description,
            () => method.Invoke(mod, null));
    }
}