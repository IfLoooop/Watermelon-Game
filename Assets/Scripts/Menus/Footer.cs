using Watermelon_Game.Singletons;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Needed so the OnClick event on the buttons don't loose their references on load
    /// </summary>
    internal sealed class Footer : PersistantMonoBehaviour<Footer> { }
}