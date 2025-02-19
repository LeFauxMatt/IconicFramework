using LeFauxMods.Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.SpecialOrders;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Vanilla integration with special orders.</summary>
internal sealed class SpecialOrders
{
    private const string Id = "SpecialOrders";
    private readonly IIconicFrameworkApi api;
    private readonly IModHelper helper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SpecialOrders" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="helper">Dependency for events, input, and content.</param>
    public SpecialOrders(IIconicFrameworkApi api, IModHelper helper)
    {
        this.api = api;
        this.helper = helper;

        api.Subscribe(static e =>
        {
            if (e.Id != Id)
            {
                return;
            }

            switch (e.Button)
            {
                case SButton.MouseRight or SButton.ControllerB:
                    Game1.player.team.qiChallengeBoardMutex.RequestLock(static delegate
                    {
                        Game1.activeClickableMenu = new SpecialOrdersBoard("Qi")
                        {
                            behaviorBeforeCleanup = static delegate
                            {
                                Game1.player.team.qiChallengeBoardMutex.ReleaseLock();
                            }
                        };
                    });

                    return;
                default:
                    Game1.player.team.ordersBoardMutex.RequestLock(static delegate
                    {
                        Game1.activeClickableMenu = new SpecialOrdersBoard
                        {
                            behaviorBeforeCleanup = static delegate
                            {
                                Game1.player.team.ordersBoardMutex.ReleaseLock();
                            }
                        };
                    });

                    return;
            }
        });

        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!SpecialOrder.IsSpecialOrdersBoardUnlocked())
        {
            return;
        }

        this.helper.Events.GameLoop.DayStarted -= this.OnDayStarted;

        this.api.AddToolbarIcon(
            Id,
            ModConstants.IconPath,
            new Rectangle(64, 0, 16, 16),
            I18n.Button_SpecialOrders_Title,
            I18n.Button_SpecialOrders_Description);
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e) =>
        this.helper.Events.GameLoop.DayStarted -= this.OnDayStarted;

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) =>
        this.helper.Events.GameLoop.DayStarted += this.OnDayStarted;
}