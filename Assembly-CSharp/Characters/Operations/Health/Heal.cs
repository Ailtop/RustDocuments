using UnityEngine;

namespace Characters.Operations.Health
{
	public class Heal : CharacterOperation
	{
		private enum Type
		{
			Percent,
			Constnat
		}

		[SerializeField]
		private Character _target;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private CustomFloat _amount;

		public override void Run(Character owner)
		{
			if (_target == null)
			{
				_target = owner;
			}
			_target.health.Heal(GetAmount());
		}

		private double GetAmount()
		{
			switch (_type)
			{
			case Type.Percent:
				return (double)_amount.value * _target.health.maximumHealth * 0.01;
			case Type.Constnat:
				return _amount.value;
			default:
				return 0.0;
			}
		}
	}
}
