using UnityEngine;

namespace FX
{
	public class GearShadowCaster : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private Collider2D _collider;

		private FootShadowRenderer _shadowRenderer;

		private void Awake()
		{
			_shadowRenderer = new FootShadowRenderer(0, base.transform);
			_shadowRenderer.spriteRenderer.sortingLayerID = _renderer.sortingLayerID;
			_shadowRenderer.spriteRenderer.sortingOrder = _renderer.sortingOrder - 10000;
		}

		private void LateUpdate()
		{
			Bounds bounds = _collider.bounds;
			bounds.size = new Vector2(0.75f, 0.5f);
			_shadowRenderer.SetBounds(bounds);
			_shadowRenderer.Update();
		}
	}
}
