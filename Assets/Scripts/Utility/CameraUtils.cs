using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Utility class for the <see cref="Camera"/>
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(Camera))]
    internal sealed class CameraUtils : MonoBehaviour
    {
        #region Inspector Fields
        [Tooltip("Absolute x position of the cameras horizontal frustum (In world coordinates)")] 
        [ReadOnly] [SerializeField] private float xFrustumPosition;
        [Tooltip("Absolute y position of the cameras horizontal frustum (In world coordinates)")]
        [ReadOnly] [SerializeField] private float yFrustumPosition;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="CameraUtils"/>
        /// </summary>
        private static CameraUtils instance;
        #endregion

        #region Properties
        /// <summary>
        /// Main <see cref="Camera"/> in the scene
        /// </summary>
        public static Camera Camera { get; private set; }
        /// <summary>
        /// <see cref="xFrustumPosition"/>
        /// </summary>
        public static float XFrustumPosition => instance.xFrustumPosition;
        /// <summary>
        /// <see cref="yFrustumPosition"/>
        /// </summary>
        public static float YFrustumPosition => instance.yFrustumPosition;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the size of the window
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetWindowSize()
        {
            var _workArea = Screen.mainWindowDisplayInfo.workArea;
            Screen.SetResolution(_workArea.width, _workArea.height, FullScreenMode.FullScreenWindow);
        }
        
        private void Awake()
        {
            instance = this;
            Camera = base.GetComponent<Camera>();
            
        }
        
        private void OnRectTransformDimensionsChange()
        {
            this.CalculateFrustumPoints();
        }
        
        /// <summary>
        /// Calculates the x and y world position of the camera frustum
        /// </summary>
        private void CalculateFrustumPoints()
        {
            if (Camera == null)
            {
                return;
            }
            
            var _normalizedViewportCoordinates = new Rect(0, 0, 1, 1);
            var _zPositionToCalculateAt = base.transform.position.z;
            var _frustumCorners = new Vector3[4];
            
            Camera.CalculateFrustumCorners(_normalizedViewportCoordinates, _zPositionToCalculateAt, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);

            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _frustumCorners.Length; i++)
            {
                _frustumCorners[i] = Camera.ScreenToWorldPoint(_frustumCorners[i]);
            }

            var _xPosition = _frustumCorners.Max(_Vector3 => _Vector3.x);
            var _yPosition = _frustumCorners.Max(_Vector3 => _Vector3.y);

            this.xFrustumPosition = CeilAbsolute(_xPosition);
            this.yFrustumPosition = CeilAbsolute(_yPosition);
        }
        
        /// <summary>
        /// Returns the absolute ceiled value of the given value
        /// </summary>
        /// <param name="_Value">Value to get the absolute ceil of</param>
        /// <returns>The absolute ceiled value of the given value</returns>
        private static float CeilAbsolute(float _Value)
        {
            var _absoluteValue = Mathf.Abs(_Value);
            var _ceiledValue = Mathf.Ceil(_absoluteValue);

            return _ceiledValue;
        }
        
        /// <summary>
        /// Transforms a point from screen space into world space, where world space is defined as the coordinate system at the very top of your game's hierarchy
        /// </summary>
        /// <param name="_Position">A screen space position (often mouse x, y), plus a z position for depth (for example, a camera clipping plane)</param>
        /// <returns>The worldspace point created by converting the screen space point at the provided distance z from the camera plane</returns>
        public static Vector3 ScreenToWorldPoint(Vector2 _Position)
        {
            return Camera.ScreenToWorldPoint(_Position);
        }

        /// <summary>
        /// Transforms position from world space into screen space
        /// </summary>
        /// <param name="_Position">A world space position</param>
        /// <returns>The screen space point created by converting the world space point</returns>
        public static Vector2 WorldToScreenSpace(Vector3 _Position)
        {
            return Camera.WorldToScreenPoint(_Position);
        }

        /// <summary>
        /// Transforms the given <see cref="_WorldPosition"/> into a screen point in the given <see cref="_Canvas"/>
        /// </summary>
        /// <param name="_Canvas">The <see cref="Canvas"/> that contains the <see cref="RectTransform"/></param>
        /// <param name="_WorldPosition">The world position to transform</param>
        /// <returns>The given <see cref="_WorldPosition"/> into a screen point in the given <see cref="_Canvas"/></returns>
        public static Vector2 WorldPointToLocalPointInRectangle(Canvas _Canvas, Vector3 _WorldPosition)
        {
            var _screenPosition = Camera.WorldToScreenPoint(_WorldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, _screenPosition, _Canvas.worldCamera, out var _localPoint);
            return _localPoint;
        }
        #endregion
    }
}