namespace LeFauxMods.IconicFramework;

using LeFauxMods.IconicFramework.Utilities;

/// <inheritdoc/>
internal sealed class ModEntry : Mod
{
    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        _ = new Log(this.Monitor);
    }
}
