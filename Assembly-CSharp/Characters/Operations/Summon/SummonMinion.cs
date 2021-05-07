using UnityEngine;

namespace Characters.Operations.Summon
{
	public class SummonMinion : CharacterOperation
	{
		[SerializeField]
		private Minion _minion;

		[Information("비워둘 경우 플레이어 위치에 1마리 소환, 그 외에는 지정된 위치마다 소환됨", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private Transform[] _spawnPositions;

		[SerializeField]
		private float _lifetime;

		[SerializeField]
		[Information("해당 개수만큼 하수인을 미리 로드해두어 하수인이 소환되는 순간의 프레임 드랍을 없애줌", InformationAttribute.InformationType.Info, false)]
		private int _preloadCount = 1;

		private void Awake()
		{
			if (_lifetime == 0f)
			{
				_lifetime = float.MaxValue;
			}
			_minion.poolObject.Preload(_preloadCount);
		}

		public override void Run(Character owner)
		{
			if (owner.playerComponents == null)
			{
				return;
			}
			if (_spawnPositions.Length == 0)
			{
				owner.playerComponents.minionLeader.Summon(_minion, owner.transform.position, _lifetime);
				return;
			}
			Transform[] spawnPositions = _spawnPositions;
			foreach (Transform transform in spawnPositions)
			{
				owner.playerComponents.minionLeader.Summon(_minion, transform.position, _lifetime);
			}
		}
	}
}
