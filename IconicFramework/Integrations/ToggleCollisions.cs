using LeFauxMods.Common.Integrations.IconicFramework;
using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Vanilla integration with collisions.</summary>
internal sealed class ToggleCollisions
{
    private const string Id = "ToggleCollisions";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggleCollisions" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public ToggleCollisions(IIconicFrameworkApi api) =>
        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(16, 16, 16, 16),
            I18n.Button_NoClip_Title,
            I18n.Button_NoClip_Disable,
            () =>
            {
                Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
                if (Game1.player.ignoreCollisions)
                {
                    api.AddToolbarIcon(
                        Id,
                        Constants.IconPath,
                        new Rectangle(16, 16, 16, 16),
                        I18n.Button_NoClip_Title,
                        I18n.Button_NoClip_Enable);

                    return;
                }

                api.AddToolbarIcon(
                    Id,
                    Constants.IconPath,
                    new Rectangle(16, 16, 16, 16),
                    I18n.Button_NoClip_Title,
                    I18n.Button_NoClip_Disable);
            });
}