namespace LeFauxMods.IconicFramework.Integrations;

using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Vanilla integration with special orders.</summary>
internal sealed class SpecialOrders
{
    private const string Id = "SpecialOrders";

    /// <summary>
    ///     Initializes a new instance of the <see cref="SpecialOrders" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public SpecialOrders(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(Id, Constants.IconPath, new Rectangle(64, 0, 16, 16), I18n.Button_SpecialOrders());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.activeClickableMenu = new SpecialOrdersBoard();
            }
        });
    }
}
