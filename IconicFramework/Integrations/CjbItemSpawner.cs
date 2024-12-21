using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with CJB Item Spawner.</summary>
internal sealed class CjbItemSpawner
{
    private const string Id = "CJBok.ItemSpawner";

    /// <summary>
    ///     Initializes a new instance of the <see cref="CjbItemSpawner" /> class.
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
        api.AddToolbarIcon(
            Id,
            "LooseSprites/Cursors",
            new Rectangle(147, 412, 10, 11),
            I18n.Button_ItemSpawner_Title,
            I18n.Button_ItemSpawner_Description);
        api.Subscribe(
            e =>
            {
                if (e.Id == Id)
                {
                    Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>();
                }
            });
    }
}
