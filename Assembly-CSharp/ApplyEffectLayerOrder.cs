using FX;
using UnityEngine;

public class ApplyEffectLayerOrder : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	private void Start()
	{
		_spriteRenderer.sortingOrder = Effects.GetSortingOrderAndIncrease();
	}
}
