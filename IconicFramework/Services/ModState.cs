using LeFauxMods.Common.Services;
using LeFauxMods.IconicFramework.Models;

namespace LeFauxMods.IconicFramework.Services;

internal sealed class ModState
{
    private static ModState? Instance;

    private readonly ConfigHelper<ModConfig> configHelper;
    private readonly Dictionary<string, IconComponent> icons = [];

    private ModState(IModHelper helper) => this.configHelper = new ConfigHelper<ModConfig>(helper);

    public static ModConfig Config => Instance!.configHelper.Config;

    public static ConfigHelper<ModConfig> ConfigHelper => Instance!.configHelper;

    public static Dictionary<string, IconComponent> Icons => Instance!.icons;

    public static void Init(IModHelper helper) => Instance ??= new ModState(helper);
}