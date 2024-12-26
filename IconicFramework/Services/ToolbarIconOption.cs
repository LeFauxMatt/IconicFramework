using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class ToolbarIconOption(
    string id,
    IModHelper helper,
    ModConfig config,
    Dictionary<string, IconComponent> icons) : ComplexOption
{
    private readonly ClickableTextureComponent downArrow = new(
        "down",
        new Rectangle(0, 0, 44, 48),
        null,
        I18n.Config_MoveDown_Tooltip(),
        Game1.mouseCursors,
        new Rectangle(421, 472, 11, 12),
        Game1.pixelZoom);

    private readonly ClickableTextureComponent showRadial = new(
        "showRadial",
        new Rectangle(
            0,
            0,
            OptionsCheckbox.sourceRectChecked.Width * Game1.pixelZoom,
            OptionsCheckbox.sourceRectChecked.Height * Game1.pixelZoom),
        null,
        I18n.Config_ShowRadial_Tooltip(),
        Game1.mouseCursors,
        OptionsCheckbox.sourceRectChecked,
        Game1.pixelZoom);

    private readonly ClickableTextureComponent showToolbar = new(
        "showToolbar",
        new Rectangle(
            0,
            0,
            OptionsCheckbox.sourceRectChecked.Width * Game1.pixelZoom,
            OptionsCheckbox.sourceRectChecked.Height * Game1.pixelZoom),
        null,
        I18n.Config_ShowToolbar_Tooltip(),
        Game1.mouseCursors,
        OptionsCheckbox.sourceRectChecked,
        Game1.pixelZoom);

    private readonly ClickableTextureComponent upArrow = new(
        "up",
        new Rectangle(0, 0, 44, 48),
        null,
        I18n.Config_MoveUp_Tooltip(),
        Game1.mouseCursors,
        new Rectangle(421, 459, 11, 12),
        Game1.pixelZoom);

    /// <inheritdoc />
    public override int Height => Game1.tileSize;

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => string.Empty;

    public bool Held { get; set; }

    public string? HoverText { get; set; }

    public string Id { get; set; } = id;

    public ToolbarIconOption? Next { get; set; }

    public Point Position { get; private set; }

    public ToolbarIconOption? Previous { get; set; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        if (!icons.TryGetValue(this.Id, out var icon))
        {
            return;
        }

        var iconConfig = config.Icons.FirstOrDefault(iconConfig => iconConfig.Id == this.Id);
        if (iconConfig is null)
        {
            return;
        }

        this.Position = pos.ToPoint();
        var (mouseX, mouseY) = Utility
            .ModifyCoordinatesForUIScale(helper.Input.GetCursorPosition().GetScaledScreenPixels())
            .ToPoint();

        var mouseLeft = helper.Input.GetState(SButton.MouseLeft);
        var mouseRight = helper.Input.GetState(SButton.MouseRight);
        var hoverY = mouseY >= this.Position.Y && mouseY < this.Position.Y + this.Height;
        var clicked = hoverY && (mouseLeft is SButtonState.Pressed || mouseRight is SButtonState.Pressed);

        // Down Arrow
        if (this.Next is not null)
        {
            this.downArrow.tryHover(mouseX - this.Position.X + 540, mouseY - this.Position.Y + 4);
            this.downArrow.draw(
                spriteBatch,
                Color.White,
                1f,
                0,
                this.Position.X - 540,
                this.Position.Y - 4);

            if ((this.downArrow.bounds with
                {
                    X = this.Position.X - 540,
                    Y = this.Position.Y - 4
                }).Contains(mouseX, mouseY))
            {
                this.HoverText = this.downArrow.hoverText;

                if (clicked)
                {
                    (this.Id, this.Next.Id) = (this.Next.Id, this.Id);
                }
            }
        }

        // Show Toolbar
        this.showToolbar.sourceRect = iconConfig.ShowToolbar
            ? OptionsCheckbox.sourceRectChecked
            : OptionsCheckbox.sourceRectUnchecked;

        this.showToolbar.draw(
            spriteBatch,
            Color.White,
            1f,
            0,
            this.Position.X - 472,
            this.Position.Y);

        if ((this.showToolbar.bounds with
            {
                X = this.Position.X - 472,
                Y = this.Position.Y
            }).Contains(mouseX, mouseY))
        {
            this.HoverText = this.showToolbar.hoverText;

            if (clicked)
            {
                iconConfig.ShowToolbar = !iconConfig.ShowToolbar;
            }
        }

        // Show Radial Menu
        this.showRadial.sourceRect = iconConfig.ShowRadial
            ? OptionsCheckbox.sourceRectChecked
            : OptionsCheckbox.sourceRectUnchecked;

        this.showRadial.draw(
            spriteBatch,
            Color.White,
            1f,
            0,
            this.Position.X - 412,
            this.Position.Y);

        if ((this.showRadial.bounds with
            {
                X = this.Position.X - 412,
                Y = this.Position.Y
            }).Contains(mouseX, mouseY))
        {
            this.HoverText = this.showRadial.hoverText;

            if (clicked)
            {
                iconConfig.ShowRadial = !iconConfig.ShowRadial;
            }
        }

        // Up Arrow
        if (this.Previous is not null)
        {
            this.upArrow.tryHover(mouseX - this.Position.X + 356, mouseY - this.Position.Y + 8);
            this.upArrow.draw(
                spriteBatch,
                Color.White,
                1f,
                0,
                this.Position.X - 356,
                this.Position.Y - 8);

            if ((this.upArrow.bounds with
                {
                    X = this.Position.X - 356,
                    Y = this.Position.Y - 8
                }).Contains(mouseX, mouseY))
            {
                this.HoverText = this.upArrow.hoverText;

                if (clicked)
                {
                    (this.Id, this.Previous.Id) = (this.Previous.Id, this.Id);
                }
            }
        }

        // IconComponent
        var x = this.Position.X - 284;
        var y = this.Position.Y + 8;
        if ((icon.bounds with
            {
                X = x,
                Y = y,
                Width = (int)(icon.sourceRect.Width * icon.baseScale * 2),
                Height = (int)(icon.sourceRect.Height * icon.baseScale * 2)
            }).Contains(mouseX, mouseY))
        {
            icon.scale = Math.Min(icon.scale + 0.04f, (icon.baseScale * 2) + 0.1f);
            this.HoverText = icon.hoverText;
            if (clicked)
            {
                this.Held = true;
            }
        }
        else
        {
            icon.scale = Math.Max(icon.scale - 0.04f, icon.baseScale * 2);
        }

        icon.draw(
            spriteBatch,
            Color.White,
            1f,
            0,
            x - icon.bounds.X,
            y - icon.bounds.Y);

        // Label
        Utility.drawTextWithShadow(
            spriteBatch,
            icon.label,
            Game1.dialogueFont,
            new Vector2(pos.X - 220, pos.Y),
            SpriteText.color_Gray);
    }
}
