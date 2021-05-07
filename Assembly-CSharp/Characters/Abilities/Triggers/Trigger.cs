using System;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	public abstract class Trigger : ITrigger
	{
		protected Action _onTriggered;

		[SerializeField]
		[Range(0f, 100f)]
		protected int _possibility = 100;

		[SerializeField]
		private float _cooldownTime;

		protected float _remainCooldownTime;

		public float cooldownTime => _cooldownTime;

		public float remainCooldownTime => _remainCooldownTime;

		protected bool canBeTriggered
		{
			get
			{
				if (_remainCooldownTime <= 0f)
				{
					return MMMaths.PercentChance(_possibility);
				}
				return false;
			}
		}

		public event Action onTriggered
		{
			add
			{
				_onTriggered = (Action)Delegate.Combine(_onTriggered, value);
			}
			remove
			{
				_onTriggered = (Action)Delegate.Remove(_onTriggered, value);
			}
		}

		public abstract void Attach(Character character);

		public abstract void Detach();

		public virtual void UpdateTime(float deltaTime)
		{
			_remainCooldownTime -= deltaTime;
		}

		protected void Invoke()
		{
			if (canBeTriggered)
			{
				_onTriggered?.Invoke();
				_remainCooldownTime = _cooldownTime;
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
