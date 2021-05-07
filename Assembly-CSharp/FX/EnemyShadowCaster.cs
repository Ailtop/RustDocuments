using UnityEngine;

namespace FX
{
	public class EnemyShadowCaster : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		[Information("0이면 콜라이더 크기 따라감", InformationAttribute.InformationType.Info, false)]
		private float _customWidth;

		private FootShadowRenderer _shadowRenderer;

		private void Awake()
		{
			if (_collider == null)
			{
				_collider = GetComponent<Collider2D>();
			}
			_shadowRenderer = new FootShadowRenderer(0, base.transform);
			_shadowRenderer.spriteRenderer.sortingLayerID = _renderer.sortingLayerID;
			_shadowRenderer.spriteRenderer.sortingOrder = _renderer.sortingOrder - 10000;
		}

		private void LateUpdate()
		{
			Bounds bounds = _collider.bounds;
			if (_customWidth > 0f)
			{
				Vector3 size = bounds.size;
				size.x = _customWidth;
				bounds.size = size;
			}
			_shadowRenderer.SetBounds(bounds);
			_shadowRenderer.Update();
			Vector3 position = _shadowRenderer.spriteRenderer.transform.position;
			position.x = bounds.center.x;
			_shadowRenderer.spriteRenderer.transform.position = position;
		}
	}
}
