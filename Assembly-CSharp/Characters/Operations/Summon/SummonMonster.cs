using Characters.Monsters;
using Level;
using UnityEngine;

namespace Characters.Operations.Summon
{
	public class SummonMonster : CharacterOperation
	{
		[SerializeField]
		private bool _containSummonWave;

		[Information("비어 있어도 문제 없음,", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private MonsterContainer _container;

		[SerializeField]
		private Monster _monsterPrefab;

		[Information("비워둘 경우 플레이어 위치에 1마리 소환, 그 외에는 지정된 위치마다 소환됨", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private Transform[] _spawnPositions;

		[SerializeField]
		[Information("해당 개수만큼 하수인을 미리 로드해두어 하수인이 소환되는 순간의 프레임 드랍을 없애줌", InformationAttribute.InformationType.Info, false)]
		private int _preloadCount = 1;

		private void Awake()
		{
			_monsterPrefab.poolObject.Preload(_preloadCount);
		}

		public override void Run(Character owner)
		{
			if (_spawnPositions.Length == 0)
			{
				Monster monster = _monsterPrefab.Summon(owner.transform.position);
				if (_container != null)
				{
					AddContainer(monster);
				}
				if (_containSummonWave)
				{
					Map.Instance.waveContainer.summonWave.Attach(monster.character);
				}
				return;
			}
			Transform[] spawnPositions = _spawnPositions;
			foreach (Transform transform in spawnPositions)
			{
				Monster monster2 = _monsterPrefab.Summon(transform.position);
				if (_container != null)
				{
					AddContainer(monster2);
				}
				if (_containSummonWave)
				{
					Map.Instance.waveContainer.summonWave.Attach(monster2.character);
				}
			}
		}

		private void AddContainer(Monster summoned)
		{
			_003C_003Ec__DisplayClass7_0 _003C_003Ec__DisplayClass7_ = new _003C_003Ec__DisplayClass7_0();
			_003C_003Ec__DisplayClass7_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass7_.summoned = summoned;
			_container.Add(_003C_003Ec__DisplayClass7_.summoned);
			_003C_003Ec__DisplayClass7_.summoned.character.health.onDied += _003C_003Ec__DisplayClass7_._003CAddContainer_003Eg__OnDied_007C0;
		}
	}
}
