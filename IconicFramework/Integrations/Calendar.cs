namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Vanilla integration with calendar.</summary>
internal sealed class Calendar
{
    private const string Id = "furyx639.IconicFramework/Calendar";

    /// <summary>
    /// Initializes a new instance of the <see cref="Calendar"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public Calendar(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(Id, "furyx639.ToolbarIcons/Icons", new Rectangle(32, 16, 16, 16), I18n.Button_Calendar());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.activeClickableMenu = new Billboard();
            }
        });
    }
}
