namespace LeFauxMods.IconicFramework.Services;

using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

internal sealed class ToolbarOverlay : IClickableMenu, IDisposable
{
    private readonly List<ClickableTextureComponent> buttons = [];
    private readonly ModConfig config;
    private readonly IModHelper helper;
    private readonly Dictionary<string, Icon> icons;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolbarOverlay"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="config"></param>
    /// <param name="icons"></param>
    public ToolbarOverlay(IModHelper helper, ModConfig config, Dictionary<string, Icon> icons)
    {
        // Init
        this.helper = helper;
        this.config = config;
        this.icons = icons;

        // Events
        EventBus.Subscribe<Signals>(this.OnSignal);

        this.RefreshIcons();
    }

    /// <inheritdoc />
    public void Dispose() => EventBus.Unsubscribe<Signals>(this.OnSignal);

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
        foreach (var button in this.buttons)
        {
            button.tryHover(x, y, 0.25f);
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var button = this.buttons.FirstOrDefault(button => button.containsPoint(x, y));
        if (button is not null)
        {
            EventBus.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(button.name, SButton.MouseLeft));
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        var button = this.buttons.FirstOrDefault(button => button.containsPoint(x, y));
        if (button is not null)
        {
            EventBus.Publish(new IconPressedEventArgs(button.name, SButton.MouseRight));
        }
    }

    private void AdjustPositionsIfNeeded(bool force = false)
    {
        var playerGlobalPos = Game1.player.StandingPixel.ToVector2();
        var playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalPos);
        var alignTop = !Game1.options.pinToolbarToggle && (playerLocalVec.Y > (Game1.viewport.Height / 2) + 64);
        var margin = Utility.makeSafeMarginY(8);
        var previousY = this.yPositionOnScreen;
        this.yPositionOnScreen = alignTop ? 112 + margin - 8 : Game1.uiViewport.Height - 144 - margin + 8;

        if (!force && previousY == this.yPositionOnScreen)
        {
            return;
        }

        for (var i = 0; i < this.buttons.Count; i++)
        {
            this.buttons[i].bounds = new Rectangle((Game1.uiViewport.Width / 2) - 384 + (i * 36), this.yPositionOnScreen, 32, 32);
        }
    }

    private void OnSignal(Signals signal)
    {
        if (signal != Signals.IconsChanged)
        {
            return;
        }

        this.RefreshIcons();
    }

    private void RefreshIcons()
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
