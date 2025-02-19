using LeFauxMods.Common.Integrations.ContentPatcher;
using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Models;
using LeFauxMods.IconicFramework.Utilities;
using StardewModdingAPI.Events;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration for Content Patcher integration.</summary>
internal sealed class ContentPatcher
{
    private readonly Dictionary<string, Action> actions = new(StringComparer.OrdinalIgnoreCase);
    private readonly IIconicFrameworkApi api;
    private readonly IModHelper helper;

    public ContentPatcher(IModHelper helper, IIconicFrameworkApi api)
    {
        // Init
        this.api = api;
        this.helper = helper;

        // Events
        var contentPatcher = new ContentPatcherIntegration(helper);
        if (!contentPatcher.IsLoaded)
        {
            return;
        }

        helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        this.api.Subscribe(this.OnIconPressed);
        this.ReloadIcons();
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(static assetName => assetName.IsEquivalentTo(ModConstants.DataPath)))
        {
            this.ReloadIcons();
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (this.actions.TryGetValue(e.Id, out var action))
        {
            action.Invoke();
        }
    }

    private void ReloadIcons()
    {
        var content = this.helper.GameContent.Load<Dictionary<string, ContentData>>(ModConstants.DataPath);
        foreach (var (id, data) in content)
        {
            switch (data.Type)
            {
                case IntegrationType.Menu
                    when IntegrationHelper.TryGetMenuAction(data.ModId, data.ExtraData, out var action) &&
                         this.actions.TryAdd(id, action):
                    break;

                case IntegrationType.Method
                    when IntegrationHelper.TryGetMethod(data.ModId, data.ExtraData, out var action) &&
                         this.actions.TryAdd(id, action):
                    break;

                case IntegrationType.Keybind
                    when IntegrationHelper.TryGetKeybindAction(data.ModId, data.ExtraData, out var action) &&
                         this.actions.TryAdd(id, action):
                    break;
                case IntegrationType.Menu:
                case IntegrationType.Method:
                case IntegrationType.Keybind:
                    break;
                default:
                    Log.WarnOnce(
                        "Failed to add icon: {{ id: {0}, mod: {1}, type: {2}, description: {3} }}.",
                        id,
                        data.ModId,
                        data.Type.ToStringFast(),
                        data.HoverText);

                    continue;
            }

            Log.TraceOnce(
                "Adding icon: {{ id: {0}, mod: {1}, type: {2}, description: {3} }}.",
                id,
                data.ModId,
                data.Type.ToStringFast(),
                data.HoverText);

            this.api.AddToolbarIcon(
                id,
                data.TexturePath,
                data.SourceRect,
                () => data.Title,
                () => data.HoverText);
        }
    }
}