using System.Reflection;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Utilities;

internal sealed class IntegrationHelper
{
    private static IntegrationHelper instance = null!;

    private readonly IModRegistry modRegistry;
    private readonly MethodInfo overrideButtonReflected;
    private readonly IReflectionHelper reflection;

    public IntegrationHelper(IModRegistry modRegistry, IReflectionHelper reflection)
    {
        instance = this;
        this.modRegistry = modRegistry;
        this.reflection = reflection;
        this.overrideButtonReflected = Game1.input.GetType().GetMethod("OverrideButton") ??
                                       throw new MethodAccessException("Unable to access OverrideButton");
    }

    /// <summary>Attempt to retrieve a keybind action.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="keybinds">The method to run.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    public static bool TryGetKeybindAction(string modId, string keybinds, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!instance.modRegistry.IsLoaded(modId))
        {
            return false;
        }

        var keys = keybinds.Trim().Split(' ');
        IList<SButton> buttons = [];
        foreach (var key in keys)
        {
            if (Enum.TryParse(key, out SButton button))
            {
                buttons.Add(button);
            }
        }

        action = () =>
        {
            foreach (var button in buttons)
            {
                instance.OverrideButton(button, true);
            }
        };

        return true;
    }

    /// <summary>Attempt to retrieve a menu action.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="fullName">The full name to the menu class.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    public static bool TryGetMenuAction(string modId, string fullName, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!TryGetMod(modId, out var mod))
        {
            return false;
        }

        var type = mod.GetType().Assembly.GetType(fullName);
        var constructor = type?.GetConstructor([]);
        if (constructor is null)
        {
            return false;
        }

        action = () =>
        {
            var menu = constructor.Invoke([]);
            Game1.activeClickableMenu = (IClickableMenu)menu;
        };

        return true;
    }

    /// <summary>Attempt to retrieve a method.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    public static bool TryGetMethod(string modId, string method, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!TryGetMod(modId, out var mod))
        {
            return false;
        }

        var reflectedMethod = instance.reflection.GetMethod(mod, method, false);
        if (reflectedMethod is null)
        {
            return false;
        }

        action = () => reflectedMethod.Invoke();
        return true;
    }

    /// <summary>Attempt to retrieve a method with parameters.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    public static bool TryGetMethodWithParams(
        string modId,
        string method,
        object?[] arguments,
        [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!TryGetMod(modId, out var mod))
        {
            return false;
        }

        var reflectedMethod = instance.reflection.GetMethod(mod, method, false);
        if (reflectedMethod is null)
        {
            return false;
        }

        action = () => reflectedMethod.Invoke(arguments);
        return true;
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

    private void OverrideButton(SButton button, bool inputState) =>
        this.overrideButtonReflected.Invoke(Game1.input, [button, inputState]);
}