namespace LeFauxMods.IconicFramework.Services;

using Common.Integrations.GenericModConfigMenu;
using Common.Models;
using Common.Services;
using Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Models;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

/// <summary>Generic mod config menu integration.</summary>
internal sealed class ConfigMenu
{
    private readonly ModConfig config;
    private readonly ConfigHelper<ModConfig> configHelper;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;
    private readonly Dictionary<string, IconComponent> icons;
    private readonly IManifest manifest;
    private readonly List<ToolbarIconOption> options = [];
    private readonly ModConfig tempConfig;
    private bool reloadConfig;

    public ConfigMenu(
        IModHelper helper,
        IManifest manifest,
        ModConfig config,
        ConfigHelper<ModConfig> configHelper,
        Dictionary<string, IconComponent> icons)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.gmcm = new GenericModConfigMenuIntegration(manifest, helper.ModRegistry);
        this.config = config;
        this.configHelper = configHelper;
        this.icons = icons;
        this.tempConfig = this.configHelper.Load();

        helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Subscribe<IconChangedEventArgs>(this.OnIconChanged);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.config.ToggleKey.JustPressed())
        {
            return;
        }

        this.config.Visible = !this.config.Visible;
        this.helper.Input.SuppressActiveKeybinds(this.config.ToggleKey);
        this.configHelper.Save(this.config);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<ModConfig> e)
    {
        if (this.reloadConfig)
        {
            return;
        }

        this.reloadConfig = true;
        this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
    }

    private void OnIconChanged(IconChangedEventArgs e)
    {
        if (this.reloadConfig)
        {
            return;
        }

        this.reloadConfig = true;
        this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.gmcm.IsLoaded || !this.gmcm.Api.TryGetCurrentMenu(out _, out _))
        {
            return;
        }

        var option = this.options.FirstOrDefault(option => !string.IsNullOrWhiteSpace(option.HoverText));
        if (option is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, option.HoverText, null, null);
        }
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (!this.gmcm.IsLoaded || !this.gmcm.Api.TryGetCurrentMenu(out _, out _))
        {
            return;
        }

        foreach (var option in this.options)
        {
            option.HoverText = null;
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.reloadConfig = false;

        if (!Context.IsGameLaunched || !this.gmcm.IsLoaded || this.icons.Count == 0)
        {
            return;
        }

        var api = this.gmcm.Api;
        this.config.CopyTo(this.tempConfig);

        this.gmcm.Register(
            () =>
            {
                new ModConfig().CopyTo(this.tempConfig);
                foreach (var iconConfig in this.config.Icons)
                {
                    this.tempConfig.Icons.Add(new IconConfig { Id = iconConfig.Id });
                }
            },
            () =>
            {
                this.tempConfig.Icons =
                [
                    ..this.options
                        .Join(
                            this.tempConfig.Icons,
                            option => option.Id,
                            iconConfig => iconConfig.Id,
                            (_, iconConfig) => iconConfig)
                        .Concat(
                            this.tempConfig.Icons.Where(
                                iconConfig => this.options.All(option => option.Id != iconConfig.Id)))
                ];

                this.tempConfig.Visible = this.config.Visible;
                this.tempConfig.CopyTo(this.config);
                this.configHelper.Save(this.tempConfig);
            });

        api.AddKeybindList(
            this.manifest,
            () => this.tempConfig.ToggleKey,
            value => this.tempConfig.ToggleKey = value,
            I18n.Config_ToggleKey_Name,
            I18n.Config_ToggleKey_Tooltip);

        api.AddBoolOption(
            this.manifest,
            () => this.tempConfig.PlaySound,
            value => this.tempConfig.PlaySound = value,
            I18n.Config_PlaySound_Name,
            I18n.Config_PlaySound_Tooltip);

        api.AddBoolOption(
            this.manifest,
            () => this.tempConfig.ShowTooltip,
            value => this.tempConfig.ShowTooltip = value,
            I18n.Config_ShowTooltip_Name,
            I18n.Config_ShowTooltip_Tooltip);

        api.AddNumberOption(
            this.manifest,
            () => this.tempConfig.IconSize,
            value => this.tempConfig.IconSize = value,
            () => "Size",
            () => "Size",
            16f,
            64f,
            8f);

        api.AddNumberOption(
            this.manifest,
            () => this.tempConfig.IconSpacing,
            value => this.tempConfig.IconSpacing = value,
            () => "Spacing",
            () => "Spacing",
            1f,
            8f,
            1f);

        api.AddSectionTitle(this.manifest, I18n.Config_CustomizeToolbar_Name, I18n.Config_CustomizeToolbar_Tooltip);

        this.options.Clear();
        ToolbarIconOption? previousOption = null;
        foreach (var iconConfig in this.tempConfig.Icons)
        {
            if (!this.icons.TryGetValue(iconConfig.Id, out _))
            {
                continue;
            }

            var option = new ToolbarIconOption(iconConfig.Id, this.helper, this.tempConfig, this.icons);
            if (previousOption is not null)
            {
                previousOption.Next = option;
                option.Previous = previousOption;
            }

            this.options.Add(option);
            this.gmcm.AddComplexOption(option);
            previousOption = option;
        }
    }

    private sealed class ToolbarIconOption(
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

        public string? HoverText { get; set; }

        public string Id { get; private set; } = id;

        public ToolbarIconOption? Next { get; set; }

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

            var (mouseX, mouseY) = Utility
                .ModifyCoordinatesForUIScale(helper.Input.GetCursorPosition().GetScaledScreenPixels())
                .ToPoint();

            var mouseLeft = helper.Input.GetState(SButton.MouseLeft) == SButtonState.Pressed;
            var mouseRight = helper.Input.GetState(SButton.MouseRight) == SButtonState.Pressed;
            var hoverY = mouseY >= pos.Y && mouseY < pos.Y + this.Height;
            var clicked = (mouseLeft || mouseRight) && hoverY;

            // Down Arrow
            if (this.Next is not null)
            {
                this.downArrow.tryHover(mouseX - (int)pos.X + 540, mouseY - (int)pos.Y + 4);
                this.downArrow.draw(
                    spriteBatch,
                    Color.White,
                    1f,
                    0,
                    (int)pos.X - 540,
                    (int)pos.Y - 4);

                if ((this.downArrow.bounds with
                    {
                        X = (int)pos.X - 540,
                        Y = (int)pos.Y - 4
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
                (int)pos.X - 472,
                (int)pos.Y);

            if ((this.showToolbar.bounds with
                {
                    X = (int)pos.X - 472,
                    Y = (int)pos.Y
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
                (int)pos.X - 412,
                (int)pos.Y);

            if ((this.showRadial.bounds with
                {
                    X = (int)pos.X - 412,
                    Y = (int)pos.Y
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
                this.upArrow.tryHover(mouseX - (int)pos.X + 356, mouseY - (int)pos.Y + 8);
                this.upArrow.draw(
                    spriteBatch,
                    Color.White,
                    1f,
                    0,
                    (int)pos.X - 356,
                    (int)pos.Y - 8);

                if ((this.upArrow.bounds with
                    {
                        X = (int)pos.X - 356,
                        Y = (int)pos.Y - 8
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
            var x = (int)pos.X - 284;
            var y = (int)pos.Y + 8;

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
}
