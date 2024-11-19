namespace LeFauxMods.IconicFramework.Features;

using LeFauxMods.IconicFramework.Api;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

internal sealed class ToolbarOverlay : IClickableMenu, IDisposable
{
    private readonly List<ClickableTextureComponent> buttons = [];
    private readonly ModConfig config;
    private readonly EventManager eventManager;
    private readonly IModHelper helper;
    private readonly Dictionary<string, Icon> icons;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolbarOverlay"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="config"></param>
    /// <param name="eventManager"></param>
    /// <param name="icons"></param>
    public ToolbarOverlay(IModHelper helper, ModConfig config, EventManager eventManager, Dictionary<string, Icon> icons)
    {
        // Init
        this.helper = helper;
        this.config = config;
        this.eventManager = eventManager;
        this.icons = icons;
        this.ReinitializeIcons();
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        if (Game1.activeClickableMenu is not null)
        {
            return;
        }

        this.AdjustPositionsIfNeeded();
        foreach (var button in this.buttons)
        {
            button.scale = Math.Max(2f, button.scale - 0.025f);
            button.draw(b);
        }
    }

    /// <inheritdoc />
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => this.AdjustPositionsIfNeeded(true);

    public override bool isWithinBounds(int x, int y)
    {
        if (!this.buttons.Any())
        {
            return false;
        }

        return new Rectangle(
            this.buttons[0].bounds.Left,
            this.buttons[0].bounds.Top,
            this.buttons[^1].bounds.Right - this.buttons[0].bounds.Left,
            this.buttons[0].bounds.Bottom).Contains(x, y);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        foreach (var button in this.buttons.Where(button => button.containsPoint(x, y)))
        {
            button.tryHover(x, y, 0.01f);
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var button = this.buttons.FirstOrDefault(button => button.containsPoint(x, y));
        if (button is not null)
        {
            this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(button.name, SButton.MouseLeft));
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        var button = this.buttons.FirstOrDefault(button => button.containsPoint(x, y));
        if (button is not null)
        {
            this.eventManager.Publish(new IconPressedEventArgs(button.name, SButton.MouseRight));
        }
    }

    private void AdjustPositionsIfNeeded(bool force = false)
    {
        var playerGlobalPos = Game1.player.StandingPixel.ToVector2();
        var playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalPos);
        var alignTop = !Game1.options.pinToolbarToggle && (playerLocalVec.Y > (Game1.viewport.Height / 2) + 64);
        var margin = Utility.makeSafeMarginY(8);
        var previousY = this.yPositionOnScreen;
        this.yPositionOnScreen = alignTop ? 112 + margin - 8 : Game1.uiViewport.Height - margin + 8;

        if (!force && previousY == this.yPositionOnScreen)
        {
            return;
        }

        for (var i = 0; i < this.buttons.Count; i++)
        {
            this.buttons[i].bounds = new Rectangle((Game1.uiViewport.Width / 2) - 384 + (i * 32), this.yPositionOnScreen, 32, 32);
        }
    }

    private void ReinitializeIcons()
    {
        this.buttons.Clear();
        foreach (var (id, icon) in this.icons)
        {
            var texture = this.helper.GameContent.Load<Texture2D>(icon.TexturePath);
            var button = new ClickableTextureComponent(id, Rectangle.Empty, null, icon.HoverText, texture, icon.SourceRect, 2f);
            this.buttons.Add(button);
        }

        this.AdjustPositionsIfNeeded(true);
    }
}
