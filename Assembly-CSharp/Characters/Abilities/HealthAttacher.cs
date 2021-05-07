using UnityEngine;

namespace Characters.Abilities
{
	public class HealthAttacher : AbilityAttacher
	{
		private enum Type
		{
			GreaterThanOrEqual,
			LessThan
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		[Range(0f, 100f)]
		private int _healthPercent;

		[SerializeField]
		private float _checkInterval = 0.1f;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private bool _attached;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.health.onChanged += Check;
			Check();
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.health.onChanged -= Check;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		private void Check()
		{
			if ((_type == Type.GreaterThanOrEqual && base.owner.health.percent >= (double)_healthPercent * 0.01) || (_type == Type.LessThan && base.owner.health.percent < (double)_healthPercent * 0.01))
			{
				Attach();
			}
			else
			{
				Detach();
			}
		}

		private void Attach()
		{
			if (!_attached)
			{
				_attached = true;
				base.owner.ability.Add(_abilityComponent.ability);
			}
		}

		private void Detach()
		{
			if (_attached)
			{
				_attached = false;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
