using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Abilities;
using Characters.Abilities.Constraints;
using Characters.Operations;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Static : Keyword
	{
		[Serializable]
		protected class Ability : IAbility, IAbilityInstance
		{
			[SerializeField]
			[Constraint.Subcomponent]
			private Constraint.Subcomponents _constraints;

			[SerializeField]
			private Sprite _icon;

			[Space]
			[SerializeField]
			[Tooltip("수평 이동거리에 해당 숫자를 곱한값만큼 쿨타임이 감소됨. 예를 들어 4타일 이동했고 0.25일 경우 (4 x 0.25) = 1초 감소")]
			private float _cooldownReductionRatioByMove;

			[Space]
			[SerializeField]
			private OperationRunner _operationRunner;

			[Space]
			[SerializeField]
			private Collider2D _searchRange;

			private TargetLayer _layer;

			private NonAllocOverlapper _overlapper;

			private RayCaster _groundFinder;

			public Character owner { get; set; }

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon => _icon;

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired => false;

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner)
			{
				this.owner = owner;
			}

			public void Initialize()
			{
				_layer = new TargetLayer(0, false, true, false, false);
				_overlapper = new NonAllocOverlapper(1);
				_groundFinder = new RayCaster
				{
					direction = Vector2.down,
					distance = 5f
				};
				_groundFinder.contactFilter.SetLayerMask(Layers.groundMask);
				_searchRange.enabled = false;
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					remainTime += duration;
					if (_constraints.components.Pass())
					{
						SpawnThunderbolt();
					}
				}
			}

			public void SpawnThunderbolt()
			{
				_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
				_searchRange.enabled = true;
				_overlapper.OverlapCollider(_searchRange);
				List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
				if (components.Count == 0)
				{
					_overlapper.contactFilter.SetLayerMask(2048);
					_overlapper.OverlapCollider(_searchRange);
					components = _overlapper.results.GetComponents<Collider2D, Target>();
					if (components.Count == 0)
					{
						_searchRange.enabled = false;
						SpawnThunderbolt(owner.transform.position);
						return;
					}
				}
				_searchRange.enabled = false;
				Target target = components.Random();
				_groundFinder.origin = target.transform.position;
				RaycastHit2D raycastHit2D = _groundFinder.SingleCast();
				if (!raycastHit2D)
				{
					SpawnThunderbolt(owner.transform.position);
				}
				else
				{
					SpawnThunderbolt(raycastHit2D.point);
				}
			}

			private void SpawnThunderbolt(Vector3 position)
			{
				OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
				operationInfos.transform.SetPositionAndRotation(position, Quaternion.identity);
				operationInfos.Run(owner);
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				remainTime = 5f;
				owner.movement.onMoved += OnMoved;
			}

			void IAbilityInstance.Detach()
			{
				owner.movement.onMoved -= OnMoved;
			}

			private void OnMoved(Vector2 amount)
			{
				remainTime -= Mathf.Abs(amount.x) * _cooldownReductionRatioByMove;
			}
		}

		[SerializeField]
		private float[] _cooldownTimeByLevel = new float[4] { 0f, 10f, 8f, 3f };

		[SerializeField]
		private Ability _ability;

		public override Key key => Key.Static;

		protected override IList valuesByLevel => _cooldownTimeByLevel;

		protected override void Initialize()
		{
			_ability.owner = base.character;
			_ability.Initialize();
		}

		protected override void UpdateBonus()
		{
			_ability.duration = _cooldownTimeByLevel[base.level];
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_ability);
		}

		protected override void OnDetach()
		{
			base.character.ability.Remove(_ability);
		}
	}
}
