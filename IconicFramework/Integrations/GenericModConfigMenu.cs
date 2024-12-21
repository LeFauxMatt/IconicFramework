using LeFauxMods.Common.Integrations.GenericModConfigMenu;
using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with Generic Mod Config Menu.</summary>
internal sealed class GenericModConfigMenu
{
    private const string Id = "spacechase0.GenericModConfigMenu";
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IManifest manifest;
    private readonly IReflectedMethod? method;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericModConfigMenu" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    /// <param name="gmcm">Dependency for integration with generic mod config menu.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="reflection">Dependency used for reflecting into non-public code.</param>
    public GenericModConfigMenu(IIconicFrameworkApi api, GenericModConfigMenuIntegration gmcm, IManifest manifest,
        IReflectionHelper reflection)
    {
        this.gmcm = gmcm;
        this.manifest = manifest;
        if (!gmcm.IsLoaded)
        {
            return;
        }

        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        this.method = reflection.GetMethod(mod, "OpenListMenu", false);
        if (this.method is null)
        {
            return;
        }

        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_GenericModConfigMenu_Title,
            I18n.Button_GenericModConfigMenu_Description);

        api.Subscribe(this.OnIconPressedEventArgs);
    }

    private void OnIconPressedEventArgs(IIconPressedEventArgs e)
    {
        if (e.Id != Id || !this.gmcm.IsLoaded)
        {
            return;
        }

        switch (e.Button)
        {
            case SButton.MouseRight or SButton.ControllerB:
                this.gmcm.Api.OpenModMenu(this.manifest);
                return;

            default:
                this.method?.Invoke(0);
                return;
        }
    }
}
