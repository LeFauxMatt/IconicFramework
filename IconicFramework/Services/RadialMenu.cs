namespace LeFauxMods.IconicFramework.Services;

using Common.Integrations.RadialMenu;
using Common.Models;
using Common.Utilities;
using Models;
using StardewModdingAPI.Utilities;

internal sealed class RadialMenu : IRadialMenuPageFactory
{
    private readonly ModConfig config;
    private readonly Dictionary<string, IconComponent> icons;
    private readonly IManifest manifest;
    private readonly IRadialMenuApi radialMenu;
    private readonly PerScreen<RadialMenuPage?> radialMenuPage = new();

    /// <summary>Initializes a new instance of the <see cref="RadialMenu" /> class.</summary>
    /// <param name="api">The radial menu api.</param>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="config">The mod's configuration.</param>
    /// <param name="icons">The icons.</param>
    public RadialMenu(IRadialMenuApi api, IManifest manifest, ModConfig config, Dictionary<string, IconComponent> icons)
    {
        // Init
        this.radialMenu = api;
        this.manifest = manifest;
        this.config = config;
        this.icons = icons;
        this.radialMenu.RegisterCustomMenuPage(manifest, "icons", this);

        // Events
        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Subscribe<IconChangedEventArgs>(this.OnIconChanged);
    }

    public IRadialMenuPage CreatePage(Farmer who)
    {
        this.radialMenuPage.Value ??= new RadialMenuPage(this.config, this.icons);
        this.radialMenuPage.Value.ReloadIcons();
        return this.radialMenuPage.Value;
    }

    private void OnConfigChanged(ConfigChangedEventArgs<ModConfig> e)
    {
        this.radialMenuPage.Value?.ReloadIcons();
        this.radialMenu.InvalidatePage(this.manifest, "icons");
    }

    private void OnIconChanged(IconChangedEventArgs e)
    {
        this.radialMenuPage.Value?.ReloadIcons();
        this.radialMenu.InvalidatePage(this.manifest, "icons");
    }

    private sealed class RadialMenuPage(ModConfig config, Dictionary<string, IconComponent> icons) : IRadialMenuPage
    {
        private List<IRadialMenuItem>? items;

        /// <inheritdoc />
        public IReadOnlyList<IRadialMenuItem> Items =>
            this.items ??= config.Icons.Select(iconConfig => icons.GetValueOrDefault(iconConfig.Id))
                .Where(item => item?.Texture is not null)
                .OfType<IRadialMenuItem>()
                .ToList();

        /// <inheritdoc />
        public int SelectedItemIndex => -1;

        public void ReloadIcons() => this.items = null;
    }
}
