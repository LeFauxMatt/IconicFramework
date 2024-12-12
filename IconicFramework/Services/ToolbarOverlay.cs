namespace LeFauxMods.IconicFramework.Services;

using System.Globalization;
using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.Core.Utilities;
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
    private string hoverText = string.Empty;

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
        EventBus.Subscribe<ModSignal>(this.OnSignal);

        this.RefreshIcons();
    }

    /// <inheritdoc />
    public void Dispose() => EventBus.Unsubscribe<ModSignal>(this.OnSignal);

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        if (Game1.activeClickableMenu is not null)
        {
            return;
        }

        this.AdjustPositionsIfNeeded();
        var uiTexture = this.helper.GameContent.Load<Texture2D>(Constants.UIPath);
        for (var i = 0; i < this.buttons.Count; i++)
        {
            var button = this.buttons[i];
            var rect = this.GetSlot(i);
            rect.Inflate(this.config.IconSize / 4f * (button.scale - button.baseScale), this.config.IconSize / 4f * (button.scale - button.baseScale));
            b.Draw(uiTexture, rect, Color.White);
        }

        foreach (var button in this.buttons)
        {
            button.scale = Math.Max(button.baseScale, button.scale - 0.025f);
            button.draw(b);
        }

        if (this.config.ShowTooltip && !string.IsNullOrWhiteSpace(this.hoverText))
        {
            drawHoverText(b, this.hoverText, Game1.smallFont);
            this.hoverText = string.Empty;
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

        return base.isWithinBounds(x, y);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        for (var i = 0; i < this.buttons.Count; i++)
        {
            var button = this.buttons[i];
            button.tryHover(x, y, 0.25f);
            if (this.allClickableComponents[i].containsPoint(x, y))
            {
                this.hoverText = button.hoverText;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        for (var i = 0; i < this.buttons.Count; i++)
        {
            if (!this.allClickableComponents[i].containsPoint(x, y))
            {
                continue;
            }

            var button = this.buttons[i];
            EventBus.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(button.name, SButton.MouseLeft));

            if (this.config.PlaySound)
            {
                _ = Game1.playSound("drumkit6");
            }

            return;
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        for (var i = 0; i < this.buttons.Count; i++)
        {
            if (!this.allClickableComponents[i].containsPoint(x, y))
            {
                continue;
            }

            var button = this.buttons[i];
            EventBus.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(button.name, SButton.MouseRight));

            if (this.config.PlaySound)
            {
                _ = Game1.playSound("drumkit6");
            }

            return;
        }
    }

    private void AdjustPositionsIfNeeded(bool force = false)
    {
        var playerGlobalPos = Game1.player.StandingPixel.ToVector2();
        var playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalPos);
        var alignTop = !Game1.options.pinToolbarToggle && (playerLocalVec.Y > (Game1.viewport.Height / 2) + Game1.tileSize);
        var margin = Utility.makeSafeMarginY(8);
        var previousX = this.xPositionOnScreen;
        var previousY = this.yPositionOnScreen;
        this.xPositionOnScreen = (Game1.uiViewport.Width / 2) - 384;
        this.yPositionOnScreen = alignTop ? margin + 98 : Game1.uiViewport.Height - margin - 130;

        if (!force && previousX == this.xPositionOnScreen && previousY == this.yPositionOnScreen)
        {
            return;
        }

        for (var i = 0; i < this.buttons.Count; i++)
        {
            var button = this.buttons[i];
            var slot = this.GetSlot(i);
            this.allClickableComponents[i].bounds = slot;
            var width = (int)(button.sourceRect.Width * button.baseScale);
            var height = (int)(button.sourceRect.Height * button.baseScale);
            button.bounds = slot with
            {
                X = slot.X + (int)((this.config.IconSize - width) / 2),
                Y = slot.Y + (int)((this.config.IconSize - height) / 2),
                Width = width,
                Height = height,
            };
        }

        this.width = (int)((this.buttons.Count * this.config.IconSize) + ((this.buttons.Count - 1) * this.config.IconSpacing));
        this.height = (int)this.config.IconSize;
    }

    private Rectangle GetSlot(int index) =>
        new(this.xPositionOnScreen + (int)(index * (this.config.IconSize + this.config.IconSpacing)),
            this.yPositionOnScreen,
            (int)this.config.IconSize,
            (int)this.config.IconSize);

    private void OnSignal(ModSignal signal)
    {
        if (signal != ModSignal.IconsChanged)
        {
            return;
        }

        this.RefreshIcons();
    }

    private void RefreshIcons()
    {
        this.allClickableComponents ??= [];
        this.allClickableComponents.Clear();
        this.buttons.Clear();
        var index = 0;
        foreach (var (id, icon) in this.icons)
        {
            var texture = this.helper.GameContent.Load<Texture2D>(icon.TexturePath);
            var scale = this.config.IconSize * 0.75f / Math.Max(icon.SourceRect.Width, icon.SourceRect.Height);
            var button = new ClickableTextureComponent(id, Rectangle.Empty, null, icon.HoverText, texture, icon.SourceRect, scale);

            this.buttons.Add(button);
            this.allClickableComponents.Add(new ClickableComponent(Rectangle.Empty, index.ToString(CultureInfo.InvariantCulture)));
            index++;
        }

        this.allClickableComponents.AddRange(this.buttons);
        this.AdjustPositionsIfNeeded(true);
    }
}
