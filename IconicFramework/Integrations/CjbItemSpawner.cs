namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Mod integration with CJB Item Spawner.</summary>
internal sealed class CjbItemSpawner
{
    private const string Id = "CJBok.ItemSpawner";

    /// <summary>
    /// Initializes a new instance of the <see cref="CjbItemSpawner"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public CjbItemSpawner(IIconicFrameworkApi api, IReflectionHelper reflection)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        var buildMenu = reflection.GetMethod(mod, "BuildMenu", false);
        api.AddToolbarIcon(Id, "furyx639.ToolbarIcons/Icons", new Rectangle(16, 16, 16, 16), I18n.Button_ItemSpawner());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>();
            }
        });
    }
}
