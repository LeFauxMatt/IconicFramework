namespace LeFauxMods.IconicFramework.Services;

using LeFauxMods.Core.Integrations.GenericModConfigMenu;
using LeFauxMods.IconicFramework.Models;

internal sealed class ConfigurationMenu
{
    private readonly ModConfig config;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;
    private readonly IManifest manifest;

    public ConfigurationMenu(IModHelper helper, ModConfig config, IManifest manifest)
    {
        this.gmcm = new(helper.ModRegistry);
        this.helper = helper;
        this.config = config;
        this.manifest = manifest;
        this.InitializeConfig();
    }

    private void InitializeConfig()
    {
        if (!this.gmcm.IsLoaded)
        {
            return;
        }

        this.gmcm.Api.Register(this.manifest, this.Reset, this.Save);
    }

    private void Reset()
    {
    }

    private void Save() => this.helper.WriteConfig(this.config);
}
