using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class ToolbarMenu : IClickableMenu, IDisposable
{
    private readonly ModConfig config;
    private readonly IModHelper helper;
    private readonly PerScreen<IconComponent?> hoverIcon = new();
    private readonly Dictionary<string, IconComponent> icons;

    /// <summary>Initializes a new instance of the <see cref="ToolbarMenu" /> class.</summary>
    /// <param name="helper">Dependency for events, input, and content.</param>
    /// <param name="config">The mod's configuration.</param>
    /// <param name="icons">The icons.</param>
    public ToolbarMenu(IModHelper helper, ModConfig config, Dictionary<string, IconComponent> icons)
    {
        // Init
        this.helper = helper;
        this.config = config;
        this.icons = icons;
        this.allClickableComponents ??= [];
        this.AdjustPositionsIfNeeded(true);

        // Events
        helper.Events.Display.RenderedHud += this.OnRenderedHud;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Subscribe<IconChangedEventArgs>(this.OnIconChanged);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.helper.Events.Display.RenderedHud -= this.OnRenderedHud;
        this.helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        ModEvents.Unsubscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Unsubscribe<IconChangedEventArgs>(this.OnIconChanged);
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        if (Game1.activeClickableMenu is not null || !this.config.Visible || this.icons.Count == 0)
        {
            return;
        }

        this.AdjustPositionsIfNeeded();
        var uiTexture = this.helper.GameContent.Load<Texture2D>(Constants.UiPath);

        // Draw texture behind icons
        foreach (var component in this.allClickableComponents)
        {
            if (!this.icons.TryGetValue(component.name, out var icon) || !icon.visible)
            {
                continue;
            }

            var rect = component.bounds;
            rect.Inflate(
                this.config.IconSize / Game1.pixelZoom * (icon.scale - icon.baseScale),
                this.config.IconSize / Game1.pixelZoom * (icon.scale - icon.baseScale));

            b.Draw(uiTexture, rect, Color.White);
        }

        // Draw icon texture
        foreach (var component in this.allClickableComponents)
        {
            if (!this.icons.TryGetValue(component.name, out var icon) || !icon.visible)
            {
                continue;
            }

            icon.scale = Math.Max(icon.baseScale, icon.scale - 0.025f);
            icon.draw(b);
        }
    }

    /// <inheritdoc />
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) =>
        this.AdjustPositionsIfNeeded(true);

    public override bool isWithinBounds(int x, int y) =>
        this.allClickableComponents.Count > 0 && base.isWithinBounds(x, y);

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        if (!this.config.Visible || this.allClickableComponents.Count == 0)
        {
            return;
        }

        foreach (var component in this.allClickableComponents)
        {
            if (!this.icons.TryGetValue(component.name, out var icon))
            {
                continue;
            }

            icon.tryHover(x, y, 0.25f);
            if (component.containsPoint(x, y))
            {
                this.hoverIcon.Value = icon;
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (!this.config.Visible || this.allClickableComponents.Count == 0)
        {
            return;
        }

        foreach (var component in this.allClickableComponents)
        {
            if (!component.containsPoint(x, y) || !this.icons.TryGetValue(component.name, out var icon))
            {
                continue;
            }

            if (this.config.PlaySound)
            {
                _ = Game1.playSound("drumkit6");
            }

            ModEvents.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
                new IconPressedEventArgs(icon.name, SButton.MouseLeft));

            return;
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (!this.config.Visible || this.allClickableComponents.Count == 0)
        {
            return;
        }

        foreach (var component in this.allClickableComponents)
        {
            if (!component.containsPoint(x, y) || !this.icons.TryGetValue(component.name, out var icon))
            {
                continue;
            }

            if (this.config.PlaySound)
            {
                _ = Game1.playSound("drumkit6");
            }

            ModEvents.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
                new IconPressedEventArgs(icon.name, SButton.MouseRight));

            return;
        }
    }

    private void AdjustPositionsIfNeeded(bool force = false)
    {
        var margin = Utility.makeSafeMarginY(8);
        var playerGlobalPos = Game1.player.StandingPixel.ToVector2();
        var playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalPos);
        var alignTop = !Game1.options.pinToolbarToggle &&
                       playerLocalVec.Y > (int)(Game1.viewport.Height / 2f) + Game1.tileSize;

        var previousX = this.xPositionOnScreen;
        var previousY = this.yPositionOnScreen;
        this.xPositionOnScreen = (Game1.uiViewport.Width / 2) - 384;
        this.yPositionOnScreen = alignTop
            ? +(int)this.config.IconSpacing + margin + 96
            : Game1.uiViewport.Height - (int)this.config.IconSize - (int)this.config.IconSpacing - margin - 96;

        if (!force && previousX == this.xPositionOnScreen && previousY == this.yPositionOnScreen)
        {
            return;
        }

        this.allClickableComponents.Clear();
        foreach (var iconConfig in this.config.Icons)
        {
            if (!this.icons.TryGetValue(iconConfig.Id, out var icon) || !iconConfig.ShowToolbar)
            {
                continue;
            }

            this.allClickableComponents.Add(new ClickableComponent(Rectangle.Empty, iconConfig.Id));
        }

        var index = 0;
        foreach (var component in this.allClickableComponents)
        {
            if (!this.icons.TryGetValue(component.name, out var icon))
            {
                continue;
            }

            component.bounds = new Rectangle(
                this.xPositionOnScreen + (int)(index * (this.config.IconSize + this.config.IconSpacing)),
                this.yPositionOnScreen,
                (int)this.config.IconSize,
                (int)this.config.IconSize);

            icon.bounds.Width = (int)(icon.sourceRect.Width * icon.baseScale);
            icon.bounds.Height = (int)(icon.sourceRect.Height * icon.baseScale);
            icon.bounds.X = component.bounds.X + (int)((this.config.IconSize - icon.bounds.Width) / 2);
            icon.bounds.Y = component.bounds.Y + (int)((this.config.IconSize - icon.bounds.Height) / 2);
            index++;
        }

        this.width = (int)((index * this.config.IconSize) + ((index - 1) * this.config.IconSpacing));
        this.height = (int)this.config.IconSize;
    }

    private void OnConfigChanged(ConfigChangedEventArgs<ModConfig> e) => this.AdjustPositionsIfNeeded(true);

    private void OnIconChanged(IconChangedEventArgs e) => this.AdjustPositionsIfNeeded(true);

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not null || !this.config.Visible || this.icons.Count == 0)
        {
            return;
        }


        foreach (var component in this.allClickableComponents.OfType<ClickableTextureComponent>())
        {
            component.scale = component.baseScale;
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!this.config.ShowTooltip || this.hoverIcon.Value is null || !Game1.IsHudDrawn)
        {
            return;
        }

        if (this.hoverIcon.Value.Title == this.hoverIcon.Value.Description)
        {
            drawHoverText(e.SpriteBatch, this.hoverIcon.Value.Title, Game1.smallFont);
            this.hoverIcon.Value = null;
            return;
        }

        drawHoverText(
            e.SpriteBatch,
            this.hoverIcon.Value.Description,
            Game1.smallFont,
            boldTitleText: this.hoverIcon.Value.Title);

        this.hoverIcon.Value = null;
    }
}
