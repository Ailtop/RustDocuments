using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class ArchlichSoulLootingPassive : Ability, IAbilityInstance
	{
		[NonSerialized]
		public CharacterOperation[] operationsOnStacked;

		[SerializeField]
		private Stat.Values _statPerStack;

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		private string _skillKey;

		private Stat.Values _stat;

		private int _stacks;

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

		public Sprite icon
		{
			get
			{
				if (_stacks <= 0)
				{
					return null;
				}
				return _defaultIcon;
			}
		}

		public float iconFillAmount => 0f;

		public int iconStacks => _stacks;

		public bool expired => false;

		public override void Initialize()
		{
			base.Initialize();
			_stat = _statPerStack.Clone();
			for (int i = 0; i < operationsOnStacked.Length; i++)
			{
				operationsOnStacked[i].Initialize();
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
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
			Character character = owner;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnKilled));
			owner.health.onTookDamage += OnTookDamage;
			UpdateStack();
			owner.stat.AttachValues(_stat);
		}

		public void Detach()
		{
			Character character = owner;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnKilled));
			owner.health.onTookDamage -= OnTookDamage;
			owner.stat.DetachValues(_stat);
		}

		private void OnKilled(ITarget target, ref Damage damage)
		{
			if (!(target.character == null) && target.character.type != Character.Type.Dummy && target.character.type != Character.Type.Trap && damage.key.Equals(_skillKey, StringComparison.CurrentCultureIgnoreCase) && _stacks != _maxStack)
			{
				_stacks++;
				UpdateStack();
				for (int i = 0; i < operationsOnStacked.Length; i++)
				{
					operationsOnStacked[i].Run(owner);
				}
			}
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (tookDamage.attackType != 0)
			{
				Damage damage = tookDamage;
				if (damage.amount != 0.0)
				{
					_stacks /= 2;
					UpdateStack();
				}
			}
		}

		private void UpdateStack()
		{
			for (int i = 0; i < _stat.values.Length; i++)
			{
				_stat.values[i].value = _statPerStack.values[i].GetStackedValue(_stacks);
			}
			owner.stat.SetNeedUpdate();
		}
	}
}
