using FX;
using UnityEngine;

public class MinimapAgentGenerator : MonoBehaviour
{
	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private Color _color;

	[SerializeField]
	[HideInInspector]
	private bool _generated;

	private float _pixelPerUnit;

	private float _width;

	private float _height;

	private void Awake()
	{
		if (_generated)
		{
			Object.Destroy(this);
			return;
		}
		_generated = true;
		SpriteRenderer mainRenderer = base.gameObject.GetComponentInParent<SpriteEffectStack>().mainRenderer;
		Generate(base.gameObject, _collider.bounds, _color, mainRenderer);
		Object.Destroy(this);
	}

	public static SpriteRenderer Generate(GameObject gameObject, Bounds bounds, Color color, SpriteRenderer spriteRenderer)
	{
		return Generate(gameObject, bounds, color, spriteRenderer.sortingLayerID, spriteRenderer.sortingOrder);
	}

	public static SpriteRenderer Generate(GameObject gameObject, Bounds bounds, Color color, int sortingLayerID, int sortingOrder)
	{
		Sprite pixelSprite = Resource.instance.pixelSprite;
		gameObject.transform.position = bounds.center;
		Vector2 vector = new Vector2(bounds.size.x * pixelSprite.pixelsPerUnit / pixelSprite.rect.width, bounds.size.y * pixelSprite.pixelsPerUnit / pixelSprite.rect.height);
		gameObject.transform.localScale = vector;
		gameObject.layer = 25;
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sharedMaterial = Materials.minimap;
		spriteRenderer.sprite = pixelSprite;
		spriteRenderer.sortingLayerID = sortingLayerID;
		spriteRenderer.sortingOrder = sortingOrder;
		spriteRenderer.color = color;
		return spriteRenderer;
	}
}
