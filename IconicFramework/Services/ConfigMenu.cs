using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Services;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Services;

/// <summary>Responsible for handling the mod configuration menu.</summary>
internal sealed class ConfigMenu
{
    private readonly IGenericModConfigMenuApi api = null!;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IManifest manifest;
    private readonly IModHelper helper;
    private readonly List<IconConfigOption> options = [];
    private bool reloadConfig;

    public ConfigMenu(IModHelper helper, IManifest manifest, GenericModConfigMenuIntegration gmcm)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.gmcm = gmcm;
        if (!gmcm.IsLoaded)
        {
            return;
        }

        this.api = gmcm.Api;
        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Subscribe<IconChangedEventArgs>(this.OnIconChanged);
        this.SetupMenu();
    }

    private static ModConfig Config => ModState.ConfigHelper.Temp;

    private static ConfigHelper<ModConfig> ConfigHelper => ModState.ConfigHelper;

    private static void Reset()
    {
        ConfigHelper.Reset();
        foreach (var icon in ModState.Config.Icons)
        {
            Config.Icons.Add(new IconConfig { Id = icon.Id });
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!ModState.Config.ToggleKey.JustPressed())
        {
            return;
        }

        this.helper.Input.SuppressActiveKeybinds(ModState.Config.ToggleKey);
        Config.Visible = !ModState.Config.Visible;
        ConfigHelper.Save();
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

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.reloadConfig = false;

        if (!this.gmcm.IsLoaded || ModState.Icons.Count == 0)
        {
            return;
        }

        this.SetupMenu();
    }

    private void OnIconChanged(IconChangedEventArgs e)
    {
        if (Config.Icons.All(icon => icon.Id != e.Id))
        {
            Config.Icons.Add(new IconConfig { Id = e.Id });
        }

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
        if (option is null || !ModState.Icons.TryGetValue(option.Id, out var icon))
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

    private void Save()
    {
        Config.Icons =
        [
            ..this.options
                .Join(
                    Config.Icons,
                    static option => option.Id,
                    static icon => icon.Id,
                    static (_, icon) => icon)
                .Concat(
                    Config.Icons.Where(icon => this.options.All(option => option.Id != icon.Id)))
        ];

        Config.Visible = ModState.Config.Visible;
        ConfigHelper.Save();
    }

    private void SetupMenu()
    {
        this.gmcm.Register(Reset, this.Save);

        this.api.AddKeybindList(
            this.manifest,
            static () => Config.ToggleKey,
            static value => Config.ToggleKey = value,
            I18n.Config_ToggleKey_Name,
            I18n.Config_ToggleKey_Tooltip);

        this.api.AddBoolOption(
            this.manifest,
            static () => Config.EnableSecondary,
            static value => Config.EnableSecondary = value,
            I18n.Config_EnableSecondary_Name,
            I18n.Config_EnableSecondary_Tooltip);

        this.api.AddBoolOption(
            this.manifest,
            static () => Config.PlaySound,
            static value => Config.PlaySound = value,
            I18n.Config_PlaySound_Name,
            I18n.Config_PlaySound_Tooltip);

        this.api.AddBoolOption(
            this.manifest,
            static () => Config.ShowTooltip,
            static value => Config.ShowTooltip = value,
            I18n.Config_ShowTooltip_Name,
            I18n.Config_ShowTooltip_Tooltip);

        this.api.AddNumberOption(
            this.manifest,
            static () => Config.IconSize,
            static value => Config.IconSize = value,
            I18n.Config_IconSize_Name,
            I18n.Config_IconSize_Tooltip,
            16f,
            64f,
            8f);

        this.api.AddNumberOption(
            this.manifest,
            static () => Config.IconSpacing,
            static value => Config.IconSpacing = value,
            I18n.Config_IconSpacing_Name,
            I18n.Config_IconSpacing_Tooltip,
            1f,
            8f,
            1f);

        this.api.AddSectionTitle(this.manifest, I18n.Config_CustomizeToolbar_Name,
            I18n.Config_CustomizeToolbar_Tooltip);

        this.options.Clear();
        IconConfigOption? previousOption = null;
        foreach (var iconConfig in Config.Icons)
        {
            if (!ModState.Icons.TryGetValue(iconConfig.Id, out _))
            {
                continue;
            }

            var option = new IconConfigOption(iconConfig.Id, this.helper);
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