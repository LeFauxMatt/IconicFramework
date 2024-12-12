namespace LeFauxMods.IconicFramework.Integrations;

using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Utilities;

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

        var modType = mod.GetType();
        var perScreenList = modType.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod);
        var toDoMenu = modType.Assembly.GetType("ToDew.ToDoMenu");
        if (perScreenList is null || toDoMenu is null)
        {
            return;
        }

        api.AddToolbarIcon(Id, Constants.IconPath, new Rectangle(48, 16, 16, 16), I18n.Button_ToDew());
        api.Subscribe(e =>
        {
            if (e.Id == Id)
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
            }
        });
    }
}
