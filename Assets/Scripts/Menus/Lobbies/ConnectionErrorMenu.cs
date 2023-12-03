using Watermelon_Game.Networking;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Displayed when there were errors connecting to another host 
    /// </summary>
    internal sealed class ConnectionErrorMenu : MenuBase
    {
        #region Methods
        public override void Close(bool _PlaySound)
        {
            CustomNetworkManager.CancelConnectionAttempt();
            base.Close(_PlaySound);
        }
        #endregion
    }
}