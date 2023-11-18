using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Handles the inside bounds of the container
    /// </summary>
    internal sealed class ContainerBounds : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Inside bounds of the container")]
        [SerializeField] private RectTransform bounds;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="ContainerBounds"/>
        /// </summary>
        private static ContainerBounds instance;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            this.bounds.GetComponent<Canvas>().worldCamera = CameraUtils.Camera;
        }

        /// <summary>
        /// Returns true if the x and y components of point is a point inside <see cref="bounds"/>
        /// </summary>
        /// <param name="_Point">Point to test</param>
        /// <returns>True if the point lies within the specified rectangle</returns>
        public static bool Contains(Vector2 _Point) // TODO: Needs to be the correct container in multiplayer
        {
            var _offset = _Point - instance.bounds.anchoredPosition / 2; // TODO: Check if this still works when the container is moved in multiplayer
            return instance.bounds.rect.Contains(_offset);
        }
        #endregion
    }
}