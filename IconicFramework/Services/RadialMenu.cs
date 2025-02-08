using LeFauxMods.Common.Integrations.StarControl;
using LeFauxMods.Common.Models;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using StardewModdingAPI.Utilities;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class RadialMenu : IRadialMenuPageFactory
{
    private readonly IManifest manifest;
    private readonly IStarControlApi radialMenu;
    private readonly PerScreen<RadialMenuPage?> radialMenuPage = new();

    /// <summary>Initializes a new instance of the <see cref="RadialMenu" /> class.</summary>
    /// <param name="api">The radial menu api.</param>
    /// <param name="manifest">The mod's manifest.</param>
    public RadialMenu(IStarControlApi api, IManifest manifest)
    {
        // Init
        this.radialMenu = api;
        this.manifest = manifest;
        this.radialMenu.RegisterCustomMenuPage(manifest, "icons", this);

        // Events
        ModEvents.Subscribe<ConfigChangedEventArgs<ModConfig>>(this.OnConfigChanged);
        ModEvents.Subscribe<IconChangedEventArgs>(this.OnIconChanged);
    }

    public IRadialMenuPage CreatePage(Farmer who)
    {
        this.radialMenuPage.Value ??= new RadialMenuPage(ModState.Config, ModState.Icons);
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