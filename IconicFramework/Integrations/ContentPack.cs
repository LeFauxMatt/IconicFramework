namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.IconicFramework.Api;

internal sealed class ContentPack
{
    private readonly IIconicFrameworkApi api;

    public ContentPack(IIconicFrameworkApi api)
    {
        this.api = api;
    }
}
