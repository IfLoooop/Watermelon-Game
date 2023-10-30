namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Contains helper methods to calculate things
    /// </summary>
    internal static class Mathfx
    {
        #region Methods
        /// <summary>
        /// Returns the signed value of an unsigned angle <br/>
        /// (0)-(+360) -> (-180)-(+180)
        /// </summary>
        /// <param name="_Angle">Value to convert</param>
        /// <returns></returns>
        public static float SignedAngle(float _Angle)
        {
            _Angle %= 360;

            if (_Angle > 180)
            {
                return _Angle - 360;
            }

            return _Angle;
        }
        #endregion
    }
}