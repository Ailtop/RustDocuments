using Singletons;
using UnityEngine;

namespace FX
{
	public class FreezeSprites : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _front;

		[SerializeField]
		private SpriteRenderer _back;

		[SerializeField]
		private Vector2Int _size;

		[SerializeField]
		protected SoundInfo _freezeSound;

		public Vector2Int size => _size;

		public void Initialize(SpriteRenderer spriteRenderer, int multiplier)
		{
			SetLayer(spriteRenderer.sortingLayerID, spriteRenderer.sortingOrder);
			base.transform.localScale = Vector3.one * multiplier;
		}

		public void SetLayer(int sortingLayerID, int sortingOrder)
		{
			_front.sortingLayerID = sortingLayerID;
			_front.sortingOrder = sortingOrder + 1;
			_back.sortingLayerID = sortingLayerID;
			_back.sortingOrder = sortingOrder - 1;
		}

		public void Enable()
		{
			base.gameObject.SetActive(true);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_freezeSound, base.transform.position);
		}

		public void Disable()
		{
			base.gameObject.SetActive(false);
		}
	}
}
