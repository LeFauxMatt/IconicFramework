namespace LeFauxMods.IconicFramework.Integrations;

using LeFauxMods.Core.Integrations.ContentPatcher;
using LeFauxMods.Core.Integrations.IconicFramework;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using StardewModdingAPI.Events;

internal sealed class ContentPack
{
    private readonly Dictionary<string, Action> actions = [];
    private readonly IIconicFrameworkApi api;
    private readonly IModHelper helper;

    public ContentPack(IModHelper helper, IIconicFrameworkApi api)
    {
        // Init
        this.api = api;
        this.helper = helper;

        // Events
        helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        this.api.Subscribe(this.OnIconPressed);

        var contentPatcher = new ContentPatcherIntegration(helper);
        contentPatcher.ConditionsApiReady += this.OnConditionsApiReady;
    }

    private void AddIcon(string id, ContentPackData data)
    {
        switch (data.Type)
        {
            case IntegrationType.Menu when IntegrationHelper.TryGetMenuAction(data.ModId, data.ExtraData, out var action) && this.actions.TryAdd(id, action):
                break;

            case IntegrationType.Method when IntegrationHelper.TryGetMethod(data.ModId, data.ExtraData, out var action) && this.actions.TryAdd(id, action):
                break;

            case IntegrationType.Keybind when IntegrationHelper.TryGetKeybindAction(data.ModId, data.ExtraData, out var action) && this.actions.TryAdd(id, action):
                break;

            default:
                Log.WarnOnce(
                    "Failed to add icon: {{ id: {0}, mod: {1}, type: {2}, description: {3} }}.",
                    id,
                    data.ModId,
                    data.Type.ToStringFast(),
                    data.HoverText);

                return;
        }

        Log.TraceOnce(
            "Adding icon: {{ id: {0}, mod: {1}, type: {2}, description: {3} }}.",
            id,
            data.ModId,
            data.Type.ToStringFast(),
            data.HoverText);

        this.api.AddToolbarIcon(id, data.TexturePath, data.SourceRect, data.HoverText);
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(assetName => assetName.IsEquivalentTo(Constants.DataPath)))
        {
            this.ReloadIcons();
        }
    }

    private void OnConditionsApiReady(object? sender, bool e) => this.ReloadIcons();

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (this.actions.TryGetValue(e.Id, out var action))
        {
            action.Invoke();
        }
    }

    private void ReloadIcons()
    {
        var content = this.helper.GameContent.Load<Dictionary<string, ContentPackData>>(Constants.DataPath);
        foreach (var (id, data) in content)
        {
            this.AddIcon(id, data);
        }
    }
}
