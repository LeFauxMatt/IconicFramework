namespace LeFauxMods.IconicFramework.Services;

using System.Collections.Generic;
using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.Core.Integrations.RadialMenu;
using LeFauxMods.IconicFramework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

internal sealed class PlayerOverlay : IRadialMenuPageFactory, IRadialMenuPage
{
    private readonly IModHelper helper;
    private readonly Dictionary<string, Icon> icons;
    private readonly List<IRadialMenuItem> items = [];
    private readonly IManifest manifest;
    private readonly RadialMenuIntegration radialMenu;

    public PlayerOverlay(IModHelper helper, ModConfig config, IManifest manifest, Dictionary<string, Icon> icons)
    {
        // Init
        this.radialMenu = new(helper.ModRegistry);
        this.helper = helper;
        this.manifest = manifest;
        this.icons = icons;

        if (!this.radialMenu.IsLoaded)
        {
            return;
        }

        this.radialMenu.Api.RegisterCustomMenuPage(manifest, "icons", this);

        // Events
        EventBus.Subscribe<ModSignal>(this.OnSignal);
        this.RefreshIcons();
    }

    public IReadOnlyList<IRadialMenuItem> Items => this.items;

    public int SelectedItemIndex { get; private set; } = -1;

    public IRadialMenuPage CreatePage(Farmer who) => this;

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
        if (!this.radialMenu.IsLoaded)
        {
            return;
        }

        this.items.Clear();
        foreach (var (id, icon) in this.icons)
        {
            var count = this.items.Count;
            var texture = this.helper.GameContent.Load<Texture2D>(icon.TexturePath);
            var menuItem = new MenuItem(
                id,
                icon.HoverText,
                texture,
                icon.SourceRect,
                () =>
                {
                    this.SelectedItemIndex = count;
                    EventBus.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, SButton.MouseLeft));
                });

            this.items.Add(menuItem);
        }

        this.radialMenu.Api.InvalidatePage(this.manifest, "icons");
    }

    private class MenuItem(string uniqueId, string? hoverText, Texture2D texture, Rectangle? sourceRect, Action onSelect) : IRadialMenuItem
    {
        public string Description { get; } = string.Empty;
        public Rectangle? SourceRectangle { get; } = sourceRect;
        public Texture2D Texture { get; } = texture;
        public string Title { get; } = hoverText ?? string.Empty;
        public string UniqueId { get; } = uniqueId;

        public MenuItemActivationResult Activate(Farmer who, DelayedActions delayedActions, MenuItemAction requestedAction)
        {
            if (delayedActions != DelayedActions.None)
            {
                return MenuItemActivationResult.Delayed;
            }

            onSelect();
            return MenuItemActivationResult.Selected;
        }
    }
}
