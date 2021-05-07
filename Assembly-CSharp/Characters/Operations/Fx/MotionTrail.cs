using System.Collections;
using FX;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class MotionTrail : CharacterOperation
	{
		public enum Layer
		{
			Behind,
			Front
		}

		protected static readonly int _overlayColor = Shader.PropertyToID("_OverlayColor");

		protected static readonly int _outlineEnabled = Shader.PropertyToID("_IsOutlineEnabled");

		protected static readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");

		protected static readonly int _outlineSize = Shader.PropertyToID("_OutlineSize");

		protected static readonly int _alphaThreshold = Shader.PropertyToID("_AlphaThreshold");

		protected const string _outsideMaterialKeyword = "SPRITE_OUTLINE_OUTSIDE";

		[SerializeField]
		private Layer _layer;

		[Header("Time")]
		[SerializeField]
		[FrameTime]
		private float _duration;

		[SerializeField]
		[FrameTime]
		private float _interval;

		[Header("Color")]
		[SerializeField]
		private bool _changeColor = true;

		[SerializeField]
		private Color _color;

		[Header("Fadeout")]
		[SerializeField]
		private AnimationCurve _fadeOutCurve;

		[SerializeField]
		[FrameTime]
		private float _fadeOutDuration;

		private CoroutineReference _cTrail;

		private MaterialPropertyBlock _propertyBlock;

		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
		}

		public override void Run(Character owner)
		{
			foreach (CharacterAnimation animation in owner.animationController.animations)
			{
				if (animation.gameObject.activeInHierarchy)
				{
					_cTrail = this.StartCoroutineWithReference(CTrail(owner, animation.spriteRenderer, owner.chronometer.animation));
				}
			}
		}

		private IEnumerator CTrail(Character owner, SpriteRenderer spriteRenderer, Chronometer chronometer)
		{
			float remainTime = ((_duration == 0f) ? float.PositiveInfinity : _duration);
			float remainInterval = 0f;
			int sortingOrderCount = 0;
			while (remainTime > 0f)
			{
				if (remainInterval <= 0f)
				{
					Effects.SpritePoolObject spritePoolObject = Effects.sprite.Spawn();
					spritePoolObject.spriteRenderer.CopyFrom(spriteRenderer);
					spritePoolObject.spriteRenderer.sortingLayerID = owner.sortingGroup.sortingLayerID;
					sortingOrderCount++;
					int sortingOrder = owner.sortingGroup.sortingOrder;
					sortingOrder = ((_layer != Layer.Front) ? (sortingOrder - sortingOrderCount) : (sortingOrder + sortingOrderCount));
					spritePoolObject.spriteRenderer.sortingOrder = sortingOrder;
					spritePoolObject.spriteRenderer.color = Color.white;
					spritePoolObject.spriteRenderer.sharedMaterial = Materials.character;
					spritePoolObject.spriteRenderer.GetPropertyBlock(_propertyBlock);
					if (_changeColor)
					{
						_propertyBlock.SetColor(_overlayColor, _color);
					}
					else
					{
						_propertyBlock.SetColor(_overlayColor, Color.clear);
					}
					spritePoolObject.spriteRenderer.SetPropertyBlock(_propertyBlock);
					Transform obj = spritePoolObject.poolObject.transform;
					Transform transform = spriteRenderer.transform;
					obj.SetPositionAndRotation(transform.position, transform.rotation);
					obj.localScale = transform.lossyScale;
					obj.rotation = transform.rotation;
					spritePoolObject.FadeOut(chronometer, _fadeOutCurve, _fadeOutDuration);
					remainInterval = _interval;
				}
				yield return null;
				float num = chronometer.DeltaTime();
				remainInterval -= num;
				remainTime -= num;
			}
		}

		public override void Stop()
		{
			_cTrail.Stop();
		}
	}
}
