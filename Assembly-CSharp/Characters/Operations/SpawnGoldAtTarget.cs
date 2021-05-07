using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class SpawnGoldAtTarget : TargetedCharacterOperation
	{
		[SerializeField]
		[Range(0f, 100f)]
		private int _possibility;

		[SerializeField]
		private int _gold;

		[SerializeField]
		private int _count;

		[SerializeField]
		private bool _spawnAtOwner;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter;

		public override void Run(Character owner, Character target)
		{
			if (_characterTypeFilter[target.type] && MMMaths.PercentChance(_possibility))
			{
				Vector3 position = (_spawnAtOwner ? owner.transform.position : target.transform.position);
				Singleton<Service>.Instance.levelManager.DropGold(_gold, _count, position);
			}
		}
	}
}
