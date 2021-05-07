using UnityEngine;

namespace FX
{
	public struct SpriteRendererValues
	{
		public static readonly SpriteRendererValues @default = new SpriteRendererValues(Effects.sprite.spriteRenderer);

		public Sprite sprite;

		public Color color;

		public bool flipX;

		public bool flipY;

		public Material sharedMaterial;

		public SpriteDrawMode drawMode;

		public int sortingLayerID;

		public int sortingOrder;

		public SpriteMaskInteraction maskInteraction;

		public SpriteSortPoint spriteSortPoint;

		public uint renderingLayerMask;

		public SpriteRendererValues(Sprite sprite, Color color, bool flipX, bool flipY, Material sharedMaterial, SpriteDrawMode drawMode, int sortingLayerID, int sortingOrder, SpriteMaskInteraction maskInteraction, SpriteSortPoint spriteSortPoint, uint renderingLayerMask)
		{
			this.sprite = sprite;
			this.color = color;
			this.flipX = flipX;
			this.flipY = flipY;
			this.sharedMaterial = sharedMaterial;
			this.drawMode = drawMode;
			this.sortingLayerID = sortingLayerID;
			this.sortingOrder = sortingOrder;
			this.maskInteraction = maskInteraction;
			this.spriteSortPoint = spriteSortPoint;
			this.renderingLayerMask = renderingLayerMask;
		}

		public SpriteRendererValues(SpriteRenderer from)
		{
			sprite = from.sprite;
			color = from.color;
			flipX = from.flipX;
			flipY = from.flipY;
			sharedMaterial = from.sharedMaterial;
			drawMode = from.drawMode;
			sortingLayerID = from.sortingLayerID;
			sortingOrder = from.sortingOrder;
			maskInteraction = from.maskInteraction;
			spriteSortPoint = from.spriteSortPoint;
			renderingLayerMask = from.renderingLayerMask;
		}
	}
}
