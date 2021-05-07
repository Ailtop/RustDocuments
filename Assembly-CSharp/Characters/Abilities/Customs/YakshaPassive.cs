using System;
using Characters.Gear.Weapons.Gauges;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class YakshaPassive : Ability, IAbilityInstance
	{
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private int _stacksToAttack;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private CoroutineReference _operationRunner;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool attached => true;

		public Sprite icon => _defaultIcon;

		public float iconFillAmount => 0f;

		public int iconStacks { get; protected set; }

		public bool expired => false;

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public void UpdateTime(float deltaTime)
		{
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
		}

		public void Detach()
		{
			_operationRunner.Stop();
		}

		public void AddStack()
		{
			_gauge.Add(1f);
			iconStacks++;
			if (iconStacks >= _stacksToAttack)
			{
				_gauge.Clear();
				iconStacks = 0;
				_operationRunner.Stop();
				_operationRunner = owner.StartCoroutineWithReference(_operations.CRun(owner));
			}
		}
	}
}
