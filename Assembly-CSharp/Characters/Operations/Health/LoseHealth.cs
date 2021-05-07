using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Health
{
	public class LoseHealth : CharacterOperation
	{
		private enum Type
		{
			Constnat,
			Percent,
			CurrentPercent
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private CustomFloat _amount;

		[SerializeField]
		[Tooltip("피해입었을 때 나타나는 숫자를 띄울지")]
		private bool _spawnFloatingText;

		public override void Run(Character owner)
		{
			double amount = GetAmount(owner);
			if (!(amount < 1.0))
			{
				owner.health.TakeHealth(amount);
				if (_spawnFloatingText)
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(amount, owner.transform.position);
				}
			}
		}

		private double GetAmount(Character owner)
		{
			switch (_type)
			{
			case Type.Constnat:
				return _amount.value;
			case Type.Percent:
				return (double)_amount.value * owner.health.maximumHealth * 0.01;
			case Type.CurrentPercent:
				return (double)_amount.value * owner.health.currentHealth * 0.01;
			default:
				return 0.0;
			}
		}
	}
}
