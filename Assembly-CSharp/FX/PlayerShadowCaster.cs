using Characters.Player;
using UnityEngine;

namespace FX
{
	public class PlayerShadowCaster : MonoBehaviour
	{
		private WeaponInventory _weaponInventory;

		private FootShadowRenderer _shadowRenderer;

		private Collider2D _collider;

		private float _customWidth;

		private void Awake()
		{
			_weaponInventory = GetComponent<WeaponInventory>();
			_weaponInventory.onSwap += UpdateCustomWidth;
			_collider = GetComponent<Collider2D>();
			_shadowRenderer = new FootShadowRenderer(1, base.transform);
			_shadowRenderer.spriteRenderer.sortingLayerName = "Player";
			_shadowRenderer.spriteRenderer.sortingOrder = -10000;
		}

		private void UpdateCustomWidth()
		{
			_customWidth = _weaponInventory.polymorphOrCurrent.customWidth;
		}

		private void OnDestroy()
		{
			_weaponInventory.onSwap -= UpdateCustomWidth;
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
		}
	}
}
