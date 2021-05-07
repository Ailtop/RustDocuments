using System;
using System.Collections;
using System.Linq;
using Characters.Abilities;
using Characters.Abilities.Triggers;
using Characters.Operations;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Brawl : Keyword
	{
		[Serializable]
		public class OperationByTrigger : Ability
		{
			public class Instance : AbilityInstance<OperationByTrigger>
			{
				public override float iconFillAmount
				{
					get
					{
						float value = ability._cooldownTimeByLevel.GetValue();
						if (value > 0f)
						{
							return ability._brawl.remainCooldownTime / value;
						}
						return base.iconFillAmount;
					}
				}

				public Instance(Character owner, OperationByTrigger ability)
					: base(owner, ability)
				{
				}

				protected override void OnAttach()
				{
					ability._triggerComponent.Attach(owner);
					ability._triggerComponent.onTriggered += OnTriggered;
				}

				protected override void OnDetach()
				{
					ability._triggerComponent.Detach();
					ability._triggerComponent.onTriggered -= OnTriggered;
				}

				public override void UpdateTime(float deltaTime)
				{
				}

				private void OnTriggered()
				{
					if (!(ability._brawl.remainCooldownTime > 0f))
					{
						owner.StartCoroutine(ability._operations.CRun(owner));
						ability._brawl.remainCooldownTime = ability._cooldownTimeByLevel.GetValue();
					}
				}
			}

			[SerializeField]
			private Brawl _brawl;

			[SerializeField]
			private ValueByLevel _cooldownTimeByLevel;

			[SerializeField]
			[TriggerComponent.Subcomponent]
			private TriggerComponent _triggerComponent;

			[SerializeField]
			[Subcomponent(typeof(OperationInfo))]
			private OperationInfo.Subcomponents _operations;

			public void UpdateTime(float deltaTime)
			{
				_triggerComponent.UpdateTime(deltaTime);
			}

			public override void Initialize()
			{
				base.Initialize();
				_operations.Initialize();
			}

			public override IAbilityInstance CreateInstance(Character owner)
			{
				return new Instance(owner, this);
			}
		}

		private enum Type
		{
			GreaterThanOrEqual,
			LessThan
		}

		[Header("Ablity")]
		[SerializeField]
		private OperationByTrigger _operationByTrigger;

		[Subcomponent(typeof(ValueByLevel))]
		[SerializeField]
		private ValueByLevel _valueByLevel;

		[Header("Enemy Count WithIn Range")]
		[SerializeField]
		private CharacterTypeBoolArray _characterTypes;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private int _numberOfEnemy;

		[SerializeField]
		private Collider2D _range;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizeRange = true;

		[SerializeField]
		private float _checkInterval = 0.25f;

		private NonAllocOverlapper _overlapper;

		private bool _attached;

		public override Key key => Key.Brawl;

		public float remainCooldownTime { get; set; }

		protected override IList valuesByLevel => _valueByLevel.values;

		protected override void Initialize()
		{
			_overlapper = new NonAllocOverlapper(_numberOfEnemy);
			_overlapper.contactFilter.SetLayerMask(1024);
			_operationByTrigger.Initialize();
		}

		protected override void UpdateBonus()
		{
			_valueByLevel.level = base.level;
		}

		protected override void OnAttach()
		{
			StartCoroutine("CCooldown");
			StartCoroutine("CCheckWithinRange");
		}

		protected override void OnDetach()
		{
			StopCoroutine("CCooldown");
			StopCoroutine("CCheckWithinRange");
			if (!(base.character == null))
			{
				base.character.ability.Remove(_operationByTrigger);
			}
		}

		private IEnumerator CCheckWithinRange()
		{
			while (true)
			{
				using (new UsingCollider(_range, _optimizeRange))
				{
					_overlapper.OverlapCollider(_range);
				}
				int num = _overlapper.results.Where(delegate(Collider2D result)
				{
					Character component = result.GetComponent<Character>();
					return !(component == null) && _characterTypes[component.type];
				}).Count();
				if ((_type == Type.GreaterThanOrEqual && num >= _numberOfEnemy) || (_type == Type.LessThan && num < _numberOfEnemy))
				{
					Attach();
				}
				else
				{
					Detach();
				}
				yield return Chronometer.global.WaitForSeconds(_checkInterval);
			}
		}

		private IEnumerator CCooldown()
		{
			while (true)
			{
				yield return null;
				float deltaTime = base.character.chronometer.master.deltaTime;
				remainCooldownTime -= deltaTime;
				_operationByTrigger.UpdateTime(deltaTime);
			}
		}

		private void Attach()
		{
			if (!_attached)
			{
				_attached = true;
				base.character.ability.Add(_operationByTrigger);
			}
		}

		private void Detach()
		{
			if (_attached)
			{
				_attached = false;
				base.character.ability.Remove(_operationByTrigger);
			}
		}
	}
}
