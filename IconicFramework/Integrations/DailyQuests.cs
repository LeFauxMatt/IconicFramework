namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Vanilla integration with daily quests.</summary>
internal sealed class DailyQuests
{
    private const string Id = "furyx639.IconicFramework/DailyQuests";

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyQuests"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public DailyQuests(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(Id, "furyx639.ToolbarIcons/Icons", new Rectangle(0, 16, 16, 16), I18n.Button_DailyQuests());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.activeClickableMenu = new Billboard(true);
            }
        });
    }
}
