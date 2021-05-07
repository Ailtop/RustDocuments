using UnityEngine;

public class RandomSpriteOnEnable : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Sprite[] _sprites;

	private void OnEnable()
	{
		_spriteRenderer.sprite = _sprites.Random();
	}
}
