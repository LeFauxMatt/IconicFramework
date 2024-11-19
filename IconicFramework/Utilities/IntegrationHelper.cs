namespace LeFauxMods.IconicFramework.Utilities;

internal sealed class IntegrationHelper
{
    private static IntegrationHelper instance = null!;

    private readonly IModRegistry modRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationHelper"/> class.
    /// </summary>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    public IntegrationHelper(IModRegistry modRegistry)
    {
        instance = this;
        this.modRegistry = modRegistry;
    }

    /// <summary>Tries to get the instance of a mod based on the mod id.</summary>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="mod">The mod instance.</param>
    /// <returns><c>true</c> if the mod instance could be obtained; otherwise, <c>false</c>.</returns>
    public static bool TryGetMod(string modId, [NotNullWhen(true)] out IMod? mod)
    {
        if (!instance.modRegistry.IsLoaded(modId))
        {
            mod = null;
            return false;
        }

        var modInfo = instance.modRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }
}
