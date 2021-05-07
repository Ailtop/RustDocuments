using Data;
using Level;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class SpawnThiefGoldAtTarget : TargetedCharacterOperation
	{
		[SerializeField]
		private PoolObject _thiefGold;

		[SerializeField]
		[Range(0f, 100f)]
		private int _possibility;

		[SerializeField]
		private int _amount;

		[SerializeField]
		private int _count;

		[SerializeField]
		private bool _spawnAtOwner;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter;

		public override void Run(Character owner, Character target)
		{
			if (_amount != 0 && _count != 0 && _characterTypeFilter[target.type] && MMMaths.PercentChance(_possibility))
			{
				Vector3 position = (_spawnAtOwner ? owner.transform.position : target.transform.position);
				SpawnGold(position);
			}
		}

		private void SpawnGold(Vector3 position)
		{
			int currencyAmount = _amount / _count;
			for (int i = 0; i < _count; i++)
			{
				CurrencyParticle component = _thiefGold.Spawn(position).GetComponent<CurrencyParticle>();
				component.currencyType = GameData.Currency.Type.Gold;
				component.currencyAmount = currencyAmount;
				if (i == 0)
				{
					component.currencyAmount += _amount % _count;
				}
			}
		}
	}
}
