namespace LeFauxMods.IconicFramework.Integrations;

using Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Vanilla integration with daily quests.</summary>
internal sealed class DailyQuests
{
    private const string Id = "DailyQuests";

    /// <summary>
    ///     Initializes a new instance of the <see cref="DailyQuests" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public DailyQuests(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(Id, Constants.IconPath, new Rectangle(0, 16, 16, 16), I18n.Button_DailyQuests());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.activeClickableMenu = new Billboard(true);
            }
        });
    }
}
