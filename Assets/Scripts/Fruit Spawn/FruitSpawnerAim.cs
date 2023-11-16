using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Controls the aim of the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class FruitSpawnerAim : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the RotationButtons GameObject")]
        [SerializeField] private GameObject rotationButtons;
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="LineRenderer"/>
        /// </summary>
        private LineRenderer lineRenderer;
        /// <summary>
        /// Determines what the <see cref="lineRenderer"/> can collide with
        /// </summary>
        private ContactFilter2D contactFilter2D;
        /// <summary>
        /// Contains the objects, the <see cref="lineRenderer"/> hits
        /// </summary>
        private readonly List<RaycastHit2D> raycastHits2D = new();
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.lineRenderer = this.GetComponent<LineRenderer>();
            this.contactFilter2D.SetLayerMask(LayerMaskController.Container_Fruit_Mask);
        }
        
        private void Update()
        {
            this.SetLineRendererSize();
        }

        private void FixedUpdate()
        {
            RotationInput();
        }

        /// <summary>
        /// Sets the length of the <see cref="lineRenderer"/> to the position of the first fruit that was hit by the raycast
        /// </summary>
        private void SetLineRendererSize()
        {
            this.raycastHits2D.Clear();
            
            var _transform = this.transform;
            Physics2D.Raycast(_transform.position, -_transform.up, contactFilter2D, raycastHits2D);

            var _rayCastHit2D = this.raycastHits2D.First();
            this.lineRenderer.SetPosition(1, new Vector3(0, -_rayCastHit2D.distance, 1));
        }
        
        /// <summary>
        /// Activates/deactivates the aim rotation controls
        /// </summary>
        /// <param name="_Value">The value to set <see cref="rotationButtons"/>.<see cref="GameObject.SetActive(bool)"/> to</param>
        public void ActivateRotationButtons(bool _Value)
        {
            this.rotationButtons.SetActive(_Value);
        }
        
        /// <summary>
        /// Handles the input, for when this <see cref="FruitSpawnerAim"/> should <see cref="Rotate"/>
        /// </summary>
        private void RotationInput()
        {
            if (this.rotationButtons.activeSelf)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    this.Rotate(-1);
                }
                if (Input.GetKey(KeyCode.E))
                {
                    this.Rotate(1);
                }
            }
        }
        
        /// <summary>
        /// Rotates the <see cref="FruitSpawner"/> in the given direction
        /// </summary>
        /// <param name="_Direction">Negative value = left, positive value = right</param>
        private void Rotate(int _Direction)
        {
            var _zRotation = _Direction * FruitSpawner.RotationSpeed * Time.fixedDeltaTime;
            var _currentRotation = Mathfx.SignedAngle(base.transform.eulerAngles.z);
            var _canRotateLeft = _Direction < 0 && _currentRotation > FruitSpawner.MaxRotationAngle * -1;
            var _canRotateRight = _Direction > 0 && _currentRotation < FruitSpawner.MaxRotationAngle;
            
            if (_canRotateLeft || _canRotateRight)
            {
                base.transform.Rotate(new Vector3(0, 0,  _zRotation));   
            }
        }

        /// <summary>
        /// Resets the <see cref="Transform.rotation"/> of this <see cref="FruitSpawnerAim"/> to <see cref="Vector3.zero"/>
        /// </summary>
        public void ResetAimRotation()
        {
            this.transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        /// <summary>
        /// Sets the value if <see cref="lineRenderer"/>.<see cref="LineRenderer.enabled"/>
        /// </summary>
        /// <param name="_Value"></param>
        public void EnableAim(bool _Value)
        {
            this.lineRenderer.enabled = _Value;
        }
        #endregion
    }
}