using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawnerAim : MonoBehaviour
    {
        #region Fields
        private LineRenderer lineRenderer;
        private ContactFilter2D contactFilter2D;
        private readonly List<RaycastHit2D> raycastHits2D = new();
        #endregion

        #region Methods
        private void Awake()
        {
            this.lineRenderer = this.GetComponent<LineRenderer>();
            this.contactFilter2D.SetLayerMask(LayerMask.GetMask("Container", "Fruit"));
        }

        private void Update()
        {
            this.SetLineRendererSize();
        }

        private void SetLineRendererSize()
        {
            this.raycastHits2D.Clear();
            
            var _transform = this.transform;
            Physics2D.Raycast(_transform.position, -_transform.up, contactFilter2D, raycastHits2D);

            var _rayCastHit2D = this.raycastHits2D.First();
            this.lineRenderer.SetPosition(1, new Vector3(0, -_rayCastHit2D.distance, 1));
        }
        #endregion
    }
}