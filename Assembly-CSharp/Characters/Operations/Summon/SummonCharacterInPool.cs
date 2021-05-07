using Services;
using UnityEngine;

namespace Characters.Operations.Summon
{
	public class SummonCharacterInPool : CharacterOperation
	{
		[SerializeField]
		private Character _characterToSummon;

		[SerializeField]
		private Transform _summonTransform;

		[SerializeField]
		[Range(1f, 10f)]
		private int _cacheCount = 1;

		[SerializeField]
		[Range(0f, 100f)]
		private int _spawnChance = 50;

		private Character[] _pool;

		private void Awake()
		{
			_pool = new Character[_cacheCount];
			for (int i = 0; i < _cacheCount; i++)
			{
				Character character = Object.Instantiate(_characterToSummon, _summonTransform.position, Quaternion.identity);
				character.gameObject.SetActive(false);
				_pool[i] = character;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!Service.quitting)
			{
				for (int i = 0; i < _cacheCount; i++)
				{
					Object.Destroy(_pool[i].gameObject);
				}
			}
		}

		public override void Run(Character owner)
		{
			if (!MMMaths.PercentChance(_spawnChance))
			{
				return;
			}
			for (int i = 0; i < _cacheCount; i++)
			{
				Character character = _pool[i];
				if (!character.gameObject.activeSelf)
				{
					character.transform.position = _summonTransform.position;
					character.gameObject.SetActive(true);
					break;
				}
			}
		}
	}
}
