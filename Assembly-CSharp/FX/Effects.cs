using System.Collections;
using UnityEngine;

namespace FX
{
	public static class Effects
	{
		public struct SpritePoolObject
		{
			public PoolObject poolObject;

			public SpriteRenderer spriteRenderer;

			public SpritePoolObject(PoolObject poolObject)
			{
				this.poolObject = poolObject;
				spriteRenderer = poolObject.GetComponent<SpriteRenderer>();
			}

			public SpritePoolObject(PoolObject poolObject, SpriteRenderer spriteRenderer)
			{
				this.poolObject = poolObject;
				this.spriteRenderer = spriteRenderer;
			}

			public SpritePoolObject Spawn()
			{
				return new SpritePoolObject(poolObject.Spawn());
			}

			public SpritePoolObject Spawn(SpriteRendererValues spriteRendererValues)
			{
				SpritePoolObject result = Spawn();
				result.spriteRenderer.CopyFrom(spriteRenderer);
				return result;
			}

			public SpritePoolObject Spawn(SpriteRenderer spriteRenderer)
			{
				SpritePoolObject result = Spawn();
				result.spriteRenderer.CopyFrom(spriteRenderer);
				return result;
			}

			public void FadeOut(Chronometer chronometer, AnimationCurve curve, float duration)
			{
				poolObject.FadeOut(spriteRenderer, (ChronometerBase)chronometer, curve, duration);
			}
		}

		private static short _sortingOrder = short.MinValue;

		public static readonly SpritePoolObject sprite = new SpritePoolObject(Resource.instance.emptyEffect);

		public static short GetSortingOrderAndIncrease()
		{
			return _sortingOrder++;
		}

		public static IEnumerator CFadeOut(this PoolObject poolObject, SpriteRenderer spriteRenderer, ChronometerBase chronometer, AnimationCurve curve, float duration)
		{
			float t = 0f;
			Color color = spriteRenderer.color;
			float alpha = color.a;
			float multiplier = 1f / duration;
			while (t < 1f)
			{
				yield return null;
				t += chronometer.DeltaTime() * multiplier;
				color.a = alpha * (1f - curve.Evaluate(t));
				spriteRenderer.color = color;
			}
			poolObject.Despawn();
		}

		public static void FadeOut(this PoolObject poolObject, SpriteRenderer spriteRenderer, ChronometerBase chronometer, AnimationCurve curve, float duration)
		{
			poolObject.StartCoroutine(poolObject.CFadeOut(spriteRenderer, chronometer, curve, duration));
		}

		public static void CopyFrom(this SpriteRenderer spriteRenderer, SpriteRenderer from)
		{
			spriteRenderer.sprite = from.sprite;
			spriteRenderer.color = from.color;
			spriteRenderer.flipX = from.flipX;
			spriteRenderer.flipY = from.flipY;
			spriteRenderer.sharedMaterial = from.sharedMaterial;
			spriteRenderer.drawMode = from.drawMode;
			spriteRenderer.sortingLayerID = from.sortingLayerID;
			spriteRenderer.sortingOrder = from.sortingOrder;
			spriteRenderer.maskInteraction = from.maskInteraction;
			spriteRenderer.spriteSortPoint = from.spriteSortPoint;
			spriteRenderer.renderingLayerMask = from.renderingLayerMask;
		}

		public static void CopyFrom(this SpriteRenderer spriteRenderer, SpriteRendererValues from)
		{
			spriteRenderer.sprite = from.sprite;
			spriteRenderer.color = from.color;
			spriteRenderer.flipX = from.flipX;
			spriteRenderer.flipY = from.flipY;
			spriteRenderer.sharedMaterial = from.sharedMaterial;
			spriteRenderer.drawMode = from.drawMode;
			spriteRenderer.sortingLayerID = from.sortingLayerID;
			spriteRenderer.sortingOrder = from.sortingOrder;
			spriteRenderer.maskInteraction = from.maskInteraction;
			spriteRenderer.spriteSortPoint = from.spriteSortPoint;
			spriteRenderer.renderingLayerMask = from.renderingLayerMask;
		}
	}
}
