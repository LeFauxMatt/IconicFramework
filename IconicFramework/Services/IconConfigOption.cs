using System.Globalization;
using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class IconConfigOption : ComplexOption
{
    private readonly List<ClickableTextureComponent> components = [];
    private ClickableTextureComponent? heldIcon;
    private int heldIndex;
    private int heldItem;
    private Point offset;

    public IconConfigOption(IModHelper helper)
        : base(helper)
    {
        this.Height = Game1.tileSize * ModState.Icons.Count;

        var icons = ModState.ConfigHelper.Temp.Icons;
        var i = -1;
        for (var j = 0; j < ModState.Icons.Count; j++)
        {
            IconComponent? iconComponent = null;
            while (++i < icons.Count && !ModState.Icons.TryGetValue(icons[i].Id, out iconComponent))
            {
            }

            if (iconComponent is null)
            {
                break;
            }

            this.components.Add(new ClickableTextureComponent(
                "down",
                new Rectangle(0, (Game1.tileSize * j) + 8, 44, 48),
                null,
                I18n.Config_MoveDown_Tooltip(),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                Game1.pixelZoom));

            this.components.Add(new ClickableTextureComponent(
                "showRadial",
                new Rectangle(
                    68,
                    (Game1.tileSize * j) + 12,
                    OptionsCheckbox.sourceRectChecked.Width * Game1.pixelZoom,
                    OptionsCheckbox.sourceRectChecked.Height * Game1.pixelZoom),
                null,
                I18n.Config_ShowRadial_Tooltip(),
                Game1.mouseCursors,
                icons[i].ShowRadial ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                Game1.pixelZoom));

            this.components.Add(new ClickableTextureComponent(
                "showToolbar",
                new Rectangle(
                    128,
                    (Game1.tileSize * j) + 12,
                    OptionsCheckbox.sourceRectChecked.Width * Game1.pixelZoom,
                    OptionsCheckbox.sourceRectChecked.Height * Game1.pixelZoom),
                null,
                I18n.Config_ShowToolbar_Tooltip(),
                Game1.mouseCursors,
                icons[i].ShowToolbar ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                Game1.pixelZoom));

            this.components.Add(new ClickableTextureComponent(
                "up",
                new Rectangle(184, (Game1.tileSize * j) + 8, 44, 48),
                null,
                I18n.Config_MoveUp_Tooltip(),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                Game1.pixelZoom));

            this.components.Add(new ClickableTextureComponent(
                i.ToString(CultureInfo.InvariantCulture),
                new Rectangle(256, Game1.tileSize * j, Game1.tileSize, Game1.tileSize),
                iconComponent.label,
                iconComponent.hoverText,
                iconComponent.texture,
                iconComponent.sourceRect,
                Game1.pixelZoom) { drawLabel = false });
        }
    }

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => string.Empty;

    /// <inheritdoc />
    public override void DrawOption(SpriteBatch spriteBatch, Vector2 pos)
    {
        var (originX, originY) = this.Origin;
        var (mouseX, mouseY) = this.MousePos;

        ClickableTextureComponent? hovered = null;
        ClickableTextureComponent? icon = null;
        var icons = ModState.ConfigHelper.Temp.Icons;
        var item = -1;
        var index = -1;

        for (var i = 0; i < this.components.Count; i++)
        {
            var component = this.components[i];
            var draw = i != 3 && i != this.components.Count - 5 && component != this.heldIcon;
            if (draw && component.bounds.Contains(mouseX, mouseY))
            {
                hovered = component;
                item = (5 * (i / 5)) + 4;
                icon = this.components[item];
                index = int.Parse(icon.name, CultureInfo.InvariantCulture);

                if (this.heldIcon is not null && i % 5 == 4)
                {
                    draw = false;

                    // Swap component
                    (this.components[item], this.components[this.heldItem]) =
                        (this.components[this.heldItem], this.components[item]);

                    (this.components[item].name, this.components[this.heldItem].name) =
                        (this.components[this.heldItem].name, this.components[item].name);

                    (this.components[item].bounds, this.components[this.heldItem].bounds) =
                        (this.components[this.heldItem].bounds, this.components[item].bounds);

                    // Swap config
                    (icons[index], icons[this.heldIndex]) = (icons[this.heldIndex], icons[index]);

                    this.heldItem = item;
                    this.heldIndex = index;

                    component.draw(
                        spriteBatch,
                        Color.White,
                        1f,
                        0,
                        originX,
                        originY - component.bounds.Y + this.heldIcon.bounds.Y);
                }
            }

            if (draw)
            {
                component.tryHover(mouseX, mouseY);
                component.draw(
                    spriteBatch,
                    Color.White,
                    1f,
                    0,
                    originX,
                    originY);
            }

            if (i % 5 == 4 && !string.IsNullOrWhiteSpace(component.label))
            {
                Utility.drawTextWithShadow(
                    spriteBatch,
                    component.label,
                    Game1.dialogueFont,
                    new Vector2(originX + component.bounds.Right + 16, originY + component.bounds.Y + 12),
                    SpriteText.color_Gray);
            }
        }

        if (this.Pressed && hovered is not null)
        {
            switch (hovered.name)
            {
                case "down" when int.TryParse(this.components[item + 5].name, out var otherIndex):
                    Game1.playSound("shwip");

                    // Swap component
                    (this.components[item], this.components[item + 5]) =
                        (this.components[item + 5], this.components[item]);

                    (this.components[item].name, this.components[item + 5].name) =
                        (this.components[item + 5].name, this.components[item].name);

                    (this.components[item].bounds, this.components[item + 5].bounds) =
                        (this.components[item + 5].bounds, this.components[item].bounds);

                    // Swap config
                    (icons[index], icons[otherIndex]) = (icons[otherIndex], icons[index]);

                    break;
                case "showRadial":
                    Game1.playSound("drumkit6");
                    icons[index].ShowRadial = !icons[index].ShowRadial;
                    hovered.sourceRect = icons[index].ShowRadial
                        ? OptionsCheckbox.sourceRectChecked
                        : OptionsCheckbox.sourceRectUnchecked;
                    break;
                case "showToolbar":
                    Game1.playSound("drumkit6");
                    icons[index].ShowToolbar = !icons[index].ShowToolbar;
                    hovered.sourceRect = icons[index].ShowToolbar
                        ? OptionsCheckbox.sourceRectChecked
                        : OptionsCheckbox.sourceRectUnchecked;
                    break;
                case "up" when int.TryParse(this.components[item - 5].name, out var otherIndex):
                    Game1.playSound("shwip");

                    // Swap component
                    (this.components[item], this.components[item - 5]) =
                        (this.components[item - 5], this.components[item]);

                    (this.components[item].name, this.components[item - 5].name) =
                        (this.components[item - 5].name, this.components[item].name);

                    (this.components[item].bounds, this.components[item - 5].bounds) =
                        (this.components[item - 5].bounds, this.components[item].bounds);

                    // Swap config
                    (icons[index], icons[otherIndex]) = (icons[otherIndex], icons[index]);

                    break;
                case not null when icon is not null:
                    Game1.playSound("smallSelect");
                    this.heldIcon = icon;
                    this.heldIndex = int.Parse(icon.name, CultureInfo.InvariantCulture);
                    this.heldItem = item;
                    this.offset = new Point(icon.bounds.X - mouseX, icon.bounds.Y - mouseY);
                    break;
            }
        }
        else if (this.Held && this.heldIcon is not null)
        {
            this.heldIcon.draw(
                spriteBatch,
                Color.White,
                1f,
                0,
                originX + mouseX - this.heldIcon.bounds.X + this.offset.X,
                originY + mouseY - this.heldIcon.bounds.Y + this.offset.Y);
        }
        else if (this.heldIcon is not null && !this.Pressed && !this.Held)
        {
            this.heldIcon = null;
        }

        if (!string.IsNullOrWhiteSpace(hovered?.hoverText))
        {
            IClickableMenu.drawHoverText(spriteBatch, hovered.hoverText, Game1.smallFont);
        }
    }
}