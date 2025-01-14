using System.Reflection;
using LeFauxMods.Common.Integrations.IconicFramework;
using LeFauxMods.Common.Utilities;
using LeFauxMods.IconicFramework.Utilities;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace LeFauxMods.IconicFramework.Integrations;

/// <summary>Mod integration with ToDew.</summary>
internal sealed class ToDew
{
    private const string Id = "jltaylor-us.ToDew";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToDew" /> class.
    /// </summary>
    /// <param name="api">The Iconic Framework API.</param>
    public ToDew(IIconicFrameworkApi api)
    {
        if (!IntegrationHelper.TryGetMod(Id, out var mod))
        {
            return;
        }

        Type? modType = null;
        object? perScreenList = null;
        Type? toDoMenu = null;
        try
        {
            modType = mod.GetType();
            perScreenList = modType.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod);
            toDoMenu = modType.Assembly.GetType("ToDew.ToDoMenu");
        }
        catch
        {
            // ignored
        }


        if (modType is null || perScreenList is null || toDoMenu is null)
        {
            Log.WarnOnce("Integration with {0} failed to load method.", Id);
            return;
        }

        api.AddToolbarIcon(
            Id,
            Constants.IconPath,
            new Rectangle(48, 16, 16, 16),
            I18n.Button_ToDew_Title,
            I18n.Button_ToDew_Description,
            () =>
            {
                var value = perScreenList.GetType().GetProperty("Value")?.GetValue(perScreenList);
                if (value is null)
                {
                    return;
                }

                var action = toDoMenu.GetConstructor([modType, value.GetType()]);
                if (action is null)
                {
                    return;
                }

                var menu = action.Invoke([mod, value]);
                Game1.activeClickableMenu = (IClickableMenu)menu;
            });
    }
}