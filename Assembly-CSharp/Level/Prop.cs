using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using FX;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
	public class Prop : DestructibleObject
	{
		[Serializable]
		public class DestructionPhaseInfo : ReorderableArray<DestructionPhaseInfo.PhaseSprite>
		{
			[Serializable]
			public class PhaseSprite
			{
				internal double health;

				[SerializeField]
				private float _weight = 1f;

				[SerializeField]
				private Sprite _sprite;

				[SerializeField]
				private RuntimeAnimatorController _animation;

				[SerializeField]
				private ParticleEffectInfo _particle;

				[SerializeField]
				private GameObject _toDeactivate;

				[SerializeField]
				[FormerlySerializedAs("_particleSpawnPoint")]
				public Transform particleSpawnPoint;

				public float weight => _weight;

				public Sprite sprite => _sprite;

				public RuntimeAnimatorController animation => _animation;

				public ParticleEffectInfo particle => _particle;

				public GameObject toDeactivate => _toDeactivate;
			}

			private Prop _prop;

			private float _totalWeight;

			public int current { get; protected set; }

			public void Initialize(Prop prop)
			{
				_prop = prop;
				_totalWeight = values.Sum((PhaseSprite v) => v.weight);
				PhaseSprite[] array = values;
				foreach (PhaseSprite phaseSprite in array)
				{
					phaseSprite.health = phaseSprite.weight / _totalWeight * _prop._health;
					if (phaseSprite.particleSpawnPoint == null)
					{
						phaseSprite.particleSpawnPoint = prop.transform;
					}
				}
			}

			public PhaseSprite TakeDamage(Vector3 position, Character owner, double damage, Vector2 force)
			{
				while (damage > 0.0)
				{
					if (values.Length <= current)
					{
						return null;
					}
					PhaseSprite phaseSprite = values[current];
					if (phaseSprite.health > damage)
					{
						phaseSprite.health -= damage;
						break;
					}
					damage -= phaseSprite.health;
					if (phaseSprite.toDeactivate != null)
					{
						phaseSprite.toDeactivate.SetActive(false);
					}
					if (phaseSprite.particle != null)
					{
						phaseSprite.particle.Emit(phaseSprite.particleSpawnPoint.position, _prop.collider.bounds, force);
					}
					current++;
					if (values.Length <= current)
					{
						return null;
					}
				}
				return values[current];
			}
		}

		public delegate void DidHitDelegate(Character owner, [In][IsReadOnly] ref Damage damage, Vector2 force);

		[SerializeField]
		private Key _key = Key.SmallProp;

		[SerializeField]
		private float _health;

		private Collider2D _collider;

		[SerializeField]
		private Color _startColor;

		[SerializeField]
		private Color _endColor;

		[SerializeField]
		private Curve _hitColorCurve;

		private CoroutineReference _cEaseColorReference;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private DestructionPhaseInfo _destructionPhase;

		[SerializeField]
		private Sprite _wreckage;

		[SerializeField]
		private SoundInfo _hitSound;

		[SerializeField]
		private SoundInfo _destroySound;

		private Target[] _targets;

		public Key key => _key;

		public int phase => _destructionPhase.current;

		public override Collider2D collider => _collider;

		public event DidHitDelegate onDidHit;

		private void Awake()
		{
			_targets = GetComponentsInChildren<Target>(true);
			_collider = _targets[0].collider;
			_health *= Singleton<Service>.Instance.levelManager.currentChapter.currentStage.healthMultiplier;
			_destructionPhase.Initialize(this);
		}

		public override void Hit(Character owner, ref Damage damage, Vector2 force)
		{
			damage.Evaluate(false);
			DestructionPhaseInfo.PhaseSprite phaseSprite = _destructionPhase.TakeDamage(base.transform.position, owner, damage.amount, force);
			this.onDidHit?.Invoke(owner, ref damage, force);
			if (phaseSprite == null)
			{
				if (base.destroyed)
				{
					return;
				}
				base.destroyed = true;
				_onDestroy?.Invoke();
				PersistentSingleton<SoundManager>.Instance.PlaySound(_destroySound, base.transform.position);
				switch (key)
				{
				case Key.SmallProp:
					Settings.instance.smallPropGoldPossibility.Drop(base.transform.position);
					break;
				case Key.LargeProp:
					Settings.instance.largePropGoldPossibility.Drop(base.transform.position);
					break;
				}
				if (_wreckage == null)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				for (int i = 0; i < _targets.Length; i++)
				{
					Target obj = _targets[i];
					obj.collider.enabled = false;
					obj.enabled = false;
				}
				_spriteRenderer.sprite = _wreckage;
				if (_animator != null)
				{
					UnityEngine.Object.Destroy(_animator);
				}
			}
			else
			{
				if (_animator != null)
				{
					_animator.runtimeAnimatorController = phaseSprite.animation;
				}
				_spriteRenderer.sprite = phaseSprite.sprite;
				PersistentSingleton<SoundManager>.Instance.PlaySound(_hitSound, base.transform.position);
				_cEaseColorReference.Stop();
				_cEaseColorReference = this.StartCoroutineWithReference(CEaseColor());
			}
		}

		private IEnumerator CEaseColor()
		{
			float duration = _hitColorCurve.duration;
			for (float time = 0f; time < duration; time += Chronometer.global.deltaTime)
			{
				_spriteRenderer.color = Color.Lerp(_startColor, _endColor, _hitColorCurve.Evaluate(time));
				yield return null;
			}
			_spriteRenderer.color = _endColor;
			if (base.destroyed)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}
}
