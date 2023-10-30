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
#pragma warning disable CS0109
        /// <summary>
        /// Main <see cref="Camera"/> on the scene
        /// </summary>
        private new Camera camera;
#pragma warning restore CS0109
        #endregion

        #region Properties
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
            this.camera = base.GetComponent<Camera>();
            
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
            if (this.camera == null)
            {
                return;
            }
            
            var _normalizedViewportCoordinates = new Rect(0, 0, 1, 1);
            var _zPositionToCalculateAt = base.transform.position.z;
            var _frustumCorners = new Vector3[4];
            
            this.camera.CalculateFrustumCorners(_normalizedViewportCoordinates, _zPositionToCalculateAt, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);

            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _frustumCorners.Length; i++)
            {
                _frustumCorners[i] = this.camera.ScreenToWorldPoint(_frustumCorners[i]);
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
        #endregion
    }
}