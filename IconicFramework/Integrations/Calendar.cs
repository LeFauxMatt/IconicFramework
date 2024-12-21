using LeFauxMods.Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Vanilla integration with calendar.</summary>
internal sealed class Calendar
{
    private const string Id = "Calendar";

    /// <summary>
    ///     Initializes a new instance of the <see cref="Calendar" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public Calendar(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(32, 16, 16, 16),
            I18n.Button_Calendar_Title,
            I18n.Button_Calendar_Description);

        api.Subscribe(
            e =>
            {
                if (e.Id == Id)
                {
                    Game1.activeClickableMenu = new Billboard();
                }
            });
    }
}
