using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Services;

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
        GenericModConfigMenuIntegration gmcm,
        Dictionary<string, IconComponent> icons)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.gmcm = gmcm;
        this.config = config;
        this.configHelper = configHelper;
        this.icons = icons;
        this.tempConfig = this.configHelper.Load();

        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
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
        if (!this.gmcm.IsLoaded ||
            !this.gmcm.Api.TryGetCurrentMenu(out var mod, out _) ||
            !mod.UniqueID.Equals(Constants.ModId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var option = this.options.FirstOrDefault(option => !string.IsNullOrWhiteSpace(option.HoverText));
        if (option is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, option.HoverText, null, null);
        }

        option = this.options.FirstOrDefault(option => option.Held);
        if (option is null || !this.icons.TryGetValue(option.Id, out var icon))
        {
            return;
        }

        var mouseLeft = this.helper.Input.GetState(SButton.MouseLeft);
        option.Held = mouseLeft is SButtonState.Held or SButtonState.Pressed;

        var (mouseX, mouseY) = Utility
            .ModifyCoordinatesForUIScale(this.helper.Input.GetCursorPosition().GetScaledScreenPixels())
            .ToPoint();

        icon.draw(
            e.SpriteBatch,
            Color.White,
            1f,
            0,
            mouseX - icon.bounds.X - 16,
            mouseY - icon.bounds.Y - 16);

        Game1.activeClickableMenu.drawMouse(e.SpriteBatch);

        var swap = this.options.FirstOrDefault(
            swap =>
                mouseY >= swap.Position.Y && mouseY <= swap.Position.Y + swap.Height);

        if (swap is null || swap.Id == option.Id)
        {
            return;
        }

        (option.Id, swap.Id) = (swap.Id, option.Id);
        (option.Held, swap.Held) = (swap.Held, option.Held);
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (!this.gmcm.IsLoaded ||
            !this.gmcm.Api.TryGetCurrentMenu(out var mod, out _) ||
            !mod.UniqueID.Equals(Constants.ModId, StringComparison.OrdinalIgnoreCase))
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
            () => this.tempConfig.EnableSecondary,
            value => this.tempConfig.EnableSecondary = value,
            I18n.Config_EnableSecondary_Name,
            I18n.Config_EnableSecondary_Tooltip);

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
}
