using System;
using UnityEngine;
using UnityEngine.UIElements;
using Watermelon_Game.Menus;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Controls
{
    /// <summary>
    /// Handles mouse input
    /// </summary>
    internal sealed class InputController : MonoBehaviour, IMouseCaptureEvent
    {
        #region Fields
        /// <summary>
        /// The last saved mouse position
        /// </summary>
        private Vector2 lastMousePosition;
        #endregion

        #region Properties
        /// <summary>
        /// Current position of the mouse in world coordinates
        /// </summary>
        public static Vector2 MouseWorldPosition { get; private set; }
        #endregion
        
        #region Events
        /// <summary>
        /// Is called whenever the mouse moves <br/>
        /// <b>Parameter:</b> The current mouse position in world coordinates
        /// </summary>
        public static event Action<Vector2> OnMouseMove;
        #endregion
        
        #region Methods
        private void Update()
        {
            this.MouseMovement();
        }

        /// <summary>
        /// Invokes <see cref="OnMouseMove"/> whenever the mouse moves
        /// </summary>
        private void MouseMovement()
        {
            if (MenuController.IsAnyMenuOpen)
            {
                return;
            }
            
            var _mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            
            if (_mousePosition != this.lastMousePosition)
            {
                this.lastMousePosition = _mousePosition;
                MouseWorldPosition = CameraUtils.ScreenToWorldPoint(this.lastMousePosition);
                OnMouseMove?.Invoke(MouseWorldPosition);
            }
        }
        #endregion
    }
}