using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace FX
{
	[Serializable]
	public class EffectInfo
	{
		[Serializable]
		public class AttachInfo
		{
			public enum Pivot
			{
				Center,
				TopLeft,
				Top,
				TopRight,
				Left,
				Right,
				BottomLeft,
				Bottom,
				BottomRight,
				Custom
			}

			private static readonly EnumArray<Pivot, Vector2> _pivotValues = new EnumArray<Pivot, Vector2>(new Vector2(0f, 0f), new Vector2(-0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0f), new Vector2(0f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0f, 0f));

			[SerializeField]
			internal bool _attach;

			[SerializeField]
			private Pivot _pivot;

			[SerializeField]
			[HideInInspector]
			private Vector2 _pivotValue;

			public bool attach => _attach;

			public Pivot pivot => _pivot;

			public Vector2 pivotValue => _pivotValue;

			public AttachInfo()
			{
				_attach = false;
				_pivot = Pivot.Center;
				_pivotValue = Vector2.zero;
			}

			public AttachInfo(bool attach, bool layerOnly, int layerOrderOffset, Pivot pivot)
			{
				_attach = attach;
				_pivot = pivot;
				_pivotValue = _pivotValues[pivot];
			}
		}

		[Serializable]
		public class SizeForEffectAndAnimatorArray : EnumArray<Character.SizeForEffect, RuntimeAnimatorController>
		{
		}

		public enum Blend
		{
			Normal,
			Darken,
			Lighten,
			LinearBurn,
			LinearDodge
		}

		public static readonly int huePropertyID = Shader.PropertyToID("_Hue");

		[SerializeField]
		private bool _fold;

		public bool subordinated;

		public PoolObject effect;

		public RuntimeAnimatorController animation;

		public SizeForEffectAndAnimatorArray animationBySize;

		public AttachInfo attachInfo;

		public CustomFloat scale;

		public CustomFloat scaleX;

		public CustomFloat scaleY;

		public CustomAngle angle;

		public PositionNoise noise;

		public Color color = Color.white;

		public Blend blend;

		[Range(-180f, 180f)]
		public int hue;

		public int sortingLayerId;

		public bool autoLayerOrder = true;

		public short sortingLayerOrder;

		public bool trackChildren;

		public bool loop;

		public float delay;

		public float duration;

		[Header("Flips")]
		[Tooltip("Owner의 방향에 따라서 각도를 뒤집음")]
		public bool flipDirectionByOwnerDirection;

		[Tooltip("Owner의 방향에 따라서 X 스케일을 뒤집음")]
		public bool flipXByOwnerDirection = true;

		[Tooltip("Owner의 방향에 따라서 Y 스케일을 뒤집음")]
		public bool flipYByOwnerDirection;

		[Tooltip("이미지를 좌우 반전시킴")]
		public bool flipX;

		[Tooltip("이미지를 상하 반전시킴")]
		public bool flipY;

		public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float fadeOutDuration;

		public Chronometer chronometer;

		private readonly List<ReusableChronoSpriteEffect> _children = new List<ReusableChronoSpriteEffect>();

		public EffectInfo()
		{
			attachInfo = new AttachInfo();
			scale = new CustomFloat(1f);
			scaleX = new CustomFloat(1f);
			scaleY = new CustomFloat(1f);
			angle = new CustomAngle(0f);
			noise = new PositionNoise();
			color = Color.white;
			sortingLayerId = int.MinValue;
			fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			flipDirectionByOwnerDirection = false;
			flipXByOwnerDirection = true;
			flipYByOwnerDirection = false;
		}

		public EffectInfo(PoolObject effect)
		{
			this.effect = effect;
			attachInfo = new AttachInfo();
			scale = new CustomFloat(1f);
			scaleX = new CustomFloat(1f);
			scaleY = new CustomFloat(1f);
			angle = new CustomAngle(0f);
			noise = new PositionNoise();
			color = Color.white;
			sortingLayerId = SortingLayer.NameToID("Effect");
			fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			flipDirectionByOwnerDirection = false;
			flipXByOwnerDirection = true;
			flipYByOwnerDirection = false;
		}

		public EffectInfo(RuntimeAnimatorController animation)
		{
			this.animation = animation;
			animationBySize = new SizeForEffectAndAnimatorArray();
			for (int i = 0; i < animationBySize.Array.Length; i++)
			{
				animationBySize.Array[i] = animation;
			}
			attachInfo = new AttachInfo();
			scale = new CustomFloat(1f);
			scaleX = new CustomFloat(1f);
			scaleY = new CustomFloat(1f);
			angle = new CustomAngle(0f);
			noise = new PositionNoise();
			color = Color.white;
			sortingLayerId = SortingLayer.NameToID("Effect");
			fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			flipDirectionByOwnerDirection = false;
			flipXByOwnerDirection = true;
			flipYByOwnerDirection = false;
		}

		public EffectInfo(RuntimeAnimatorController animation, SizeForEffectAndAnimatorArray animationBySize)
		{
			this.animation = animation;
			this.animationBySize = animationBySize;
			attachInfo = new AttachInfo();
			scale = new CustomFloat(1f);
			scaleX = new CustomFloat(1f);
			scaleY = new CustomFloat(1f);
			angle = new CustomAngle(0f);
			noise = new PositionNoise();
			color = Color.white;
			sortingLayerId = SortingLayer.NameToID("Effect");
			fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			flipDirectionByOwnerDirection = false;
			flipXByOwnerDirection = true;
			flipYByOwnerDirection = false;
		}

		private ReusableChronoSpriteEffect SpawnEffectObject(PoolObject effect, Vector3 position, float extraAngle, float extraScale, bool flip = false)
		{
			float num = angle.value + extraAngle;
			if (flip && flipDirectionByOwnerDirection)
			{
				num = (180f - num) % 360f;
			}
			ReusableChronoSpriteEffect component = effect.Spawn(position + noise.Evaluate(), Quaternion.Euler(0f, 0f, num), false).GetComponent<ReusableChronoSpriteEffect>();
			Vector3 localScale = Vector3.one * extraScale * scale.value;
			float value = scaleX.value;
			if (value > 0f)
			{
				localScale.x *= value;
			}
			float value2 = scaleY.value;
			if (value2 > 0f)
			{
				localScale.y *= value2;
			}
			component.renderer.flipX = flipX;
			component.renderer.flipY = flipY;
			if (flip)
			{
				if (flipXByOwnerDirection)
				{
					localScale.x *= -1f;
				}
				if (flipYByOwnerDirection)
				{
					localScale.y *= -1f;
				}
			}
			component.transform.localScale = localScale;
			return component;
		}

		public ReusableChronoSpriteEffect Spawn(Vector3 position, float extraAngle = 0f, float extraScale = 1f)
		{
			if (effect != null)
			{
				return Spawn(position, effect.GetComponent<Animator>().runtimeAnimatorController, extraAngle, extraScale);
			}
			return Spawn(position, animation, extraAngle, extraScale);
		}

		public ReusableChronoSpriteEffect Spawn(Vector3 position, RuntimeAnimatorController animation, float extraAngle = 0f, float extraScale = 1f, bool flip = false)
		{
			_003C_003Ec__DisplayClass40_0 _003C_003Ec__DisplayClass40_ = new _003C_003Ec__DisplayClass40_0();
			_003C_003Ec__DisplayClass40_._003C_003E4__this = this;
			if (animation == null)
			{
				return null;
			}
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect = SpawnEffectObject(Resource.instance.emptyEffect, position, extraAngle, extraScale, flip);
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.animator.runtimeAnimatorController = animation;
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.gameObject.SetActive(true);
			if (SortingLayer.IsValid(sortingLayerId))
			{
				_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.renderer.sortingLayerID = sortingLayerId;
			}
			else
			{
				int num = SortingLayer.NameToID("Effect");
				Debug.LogError($"The sorting layer id of effect is invalid! id : {sortingLayerId}, effect id : {num}");
				_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.renderer.sortingLayerID = num;
			}
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.hue = hue;
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.renderer.color = color;
			Material sharedMaterial;
			switch (blend)
			{
			case Blend.Darken:
				sharedMaterial = Materials.effect_darken;
				break;
			case Blend.Lighten:
				sharedMaterial = Materials.effect_lighten;
				break;
			case Blend.LinearBurn:
				sharedMaterial = Materials.effect_linearBurn;
				break;
			case Blend.LinearDodge:
				sharedMaterial = Materials.effect_linearDodge;
				break;
			default:
				sharedMaterial = Materials.effect;
				break;
			}
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.renderer.sharedMaterial = sharedMaterial;
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.renderer.sortingOrder = (autoLayerOrder ? Effects.GetSortingOrderAndIncrease() : sortingLayerOrder);
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.chronometer = chronometer;
			_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.Play(delay, duration, loop, fadeOutCurve, fadeOutDuration);
			if (trackChildren)
			{
				_children.Add(_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect);
				_003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect.reusable.onDespawn += _003C_003Ec__DisplayClass40_._003CSpawn_003Eg__RemoveFromList_007C0;
			}
			return _003C_003Ec__DisplayClass40_.reusableChronoSpriteEffect;
		}

		public ReusableChronoSpriteEffect Spawn(Vector3 position, Character target, float extraAngle = 0f, float extraScale = 1f)
		{
			if (attachInfo.attach && target.sizeForEffect == Character.SizeForEffect.None)
			{
				return null;
			}
			RuntimeAnimatorController runtimeAnimatorController;
			if (effect != null)
			{
				runtimeAnimatorController = effect.GetComponent<Animator>().runtimeAnimatorController;
			}
			else
			{
				runtimeAnimatorController = animationBySize?[target.sizeForEffect];
				if (runtimeAnimatorController == null)
				{
					runtimeAnimatorController = animation;
				}
			}
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = Spawn(position, runtimeAnimatorController, extraAngle, extraScale, target.lookingDirection == Character.LookingDirection.Left);
			if (reusableChronoSpriteEffect == null)
			{
				return null;
			}
			reusableChronoSpriteEffect.chronometer = target.chronometer.effect;
			if (attachInfo.attach)
			{
				reusableChronoSpriteEffect.transform.parent = (flipXByOwnerDirection ? target.attachWithFlip.transform : target.attach.transform);
				Vector3 position2 = target.transform.position;
				position2.x += target.collider.offset.x;
				position2.y += target.collider.offset.y;
				Vector3 size = target.collider.bounds.size;
				size.x *= attachInfo.pivotValue.x;
				size.y *= attachInfo.pivotValue.y;
				reusableChronoSpriteEffect.transform.position = position2 + size;
			}
			return reusableChronoSpriteEffect;
		}

		public void DespawnChildren()
		{
			for (int num = _children.Count - 1; num >= 0; num--)
			{
				_children[num].reusable.Despawn();
			}
		}
	}
}
