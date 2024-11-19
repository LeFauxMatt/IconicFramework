namespace LeFauxMods.IconicFramework.Features;

using LeFauxMods.IconicFramework.Api;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

internal sealed class PlayerOverlay : IClickableMenu, IDisposable
{
    private readonly List<ClickableTextureComponent> buttons = [];
    private readonly ModConfig config;
    private readonly EventManager eventManager;
    private readonly IModHelper helper;
    private readonly Dictionary<string, Icon> icons;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerOverlay"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="config"></param>
    /// <param name="eventManager"></param>
    /// <param name="icons"></param>
    public PlayerOverlay(IModHelper helper, ModConfig config, EventManager eventManager, Dictionary<string, Icon> icons)
        : base((Game1.uiViewport.Width / 2) - 384 - 64, Game1.uiViewport.Height, 896, 208)
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
            button.scale = Math.Max(Game1.pixelZoom, button.scale - 0.025f);
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
            button.tryHover(x, y, 0.1f);
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
        var playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalPos).ToPoint();
        var previousX = this.xPositionOnScreen;
        var previousY = this.yPositionOnScreen;
        this.xPositionOnScreen = playerLocalVec.X;
        this.yPositionOnScreen = playerLocalVec.Y - Game1.tileSize;

        if (!force && previousX == this.xPositionOnScreen && previousY == this.yPositionOnScreen)
        {
            return;
        }

        const float radius = Game1.tileSize * 2.5f;
        var angleStep = 2 * Math.PI / this.buttons.Count;

        for (var i = 0; i < this.buttons.Count; i++)
        {
            var angle = i * angleStep;
            var x = this.xPositionOnScreen + (int)(radius * Math.Cos(angle));
            var y = this.yPositionOnScreen + (int)(radius * Math.Sin(angle));

            this.buttons[i].bounds = new Rectangle(
                x,
                y,
                Game1.tileSize,
                Game1.tileSize);
        }
    }

    private void ReinitializeIcons()
    {
        this.buttons.Clear();
        foreach (var (id, icon) in this.icons)
        {
            var texture = this.helper.GameContent.Load<Texture2D>(icon.TexturePath);
            var button = new ClickableTextureComponent(id, Rectangle.Empty, null, icon.HoverText, texture, icon.SourceRect, Game1.pixelZoom);
            this.buttons.Add(button);
        }

        this.AdjustPositionsIfNeeded(true);
    }
}
