using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawnerAim : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject rotationButtons;
        #endregion
        
        #region Fields
        private LineRenderer lineRenderer;
        private ContactFilter2D contactFilter2D;
        private readonly List<RaycastHit2D> raycastHits2D = new();
        #endregion

        #region Properties
        public static FruitSpawnerAim Instance { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
            this.lineRenderer = this.GetComponent<LineRenderer>();
            this.contactFilter2D.SetLayerMask(LayerMask.GetMask("Container", "Fruit"));
        }

        private void Update()
        {
            this.SetLineRendererSize();
        }

        private void FixedUpdate()
        {
            RotateAim();
        }

        private void SetLineRendererSize()
        {
            this.raycastHits2D.Clear();
            
            var _transform = this.transform;
            Physics2D.Raycast(_transform.position, -_transform.up, contactFilter2D, raycastHits2D);

            var _rayCastHit2D = this.raycastHits2D.First();
            this.lineRenderer.SetPosition(1, new Vector3(0, -_rayCastHit2D.distance, 1));
        }

        /// <summary>
        /// Activates/Deactivates the aim rotation controls
        /// </summary>
        /// <param name="_Enable">True to activate, false to deactivate</param>
        public void SetAimRotation(bool _Enable)
        {
            this.rotationButtons.SetActive(_Enable);
        }
        
        private void RotateAim()
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
            var _zRotation = _Direction * FruitSpawner.Instance.RotationStep * Time.fixedDeltaTime;
            var _currentRotation = Mathfx.SignedAngle(base.transform.eulerAngles.z);
            var _canRotateLeft = _Direction < 0 && _currentRotation > FruitSpawner.Instance.MaxRotationAngle * -1;
            var _canRotateRight = _Direction > 0 && _currentRotation < FruitSpawner.Instance.MaxRotationAngle;
            
            if (_canRotateLeft || _canRotateRight)
            {
                base.transform.Rotate(new Vector3(0, 0,  _zRotation));   
            }
        }

        public void ResetAim()
        {
            this.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        
        public static void Enable(bool _Value)
        {
            Instance.lineRenderer.enabled = _Value;
        }
        #endregion
    }
}