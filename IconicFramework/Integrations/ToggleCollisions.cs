namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;
using Microsoft.Xna.Framework;

/// <summary>Vanilla integration with collisions.</summary>
internal sealed class ToggleCollisions
{
    private const string Id = "furyx639.IconicFramework/ToggleCollisions";

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleCollisions"/> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public ToggleCollisions(IIconicFrameworkApi api)
    {
        api.AddToolbarIcon(Id, "furyx639.ToolbarIcons/Icons", new Rectangle(16, 16, 16, 16), I18n.Button_NoClip());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
            {
                Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
            }
        });
    }
}
