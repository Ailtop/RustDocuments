using Characters;
using UnityEngine;

namespace Level
{
	public class SaplingTarget : MonoBehaviour
	{
		[SerializeField]
		private Minion _entMinion;

		[SerializeField]
		private EntSapling _sapling;

		[SerializeField]
		private LayerMask _terrainLayer;

		[SerializeField]
		private float _spawnableTime = 0.1f;

		private float _spawnableCool;

		private bool _spawnable = true;

		public bool spawnable => _spawnable;

		public Minion SummonEntMinion(Character owner, float lifeTime)
		{
			Vector3 position = base.transform.position;
			Minion result = owner.playerComponents.minionLeader.Summon(_entMinion, position, lifeTime);
			_sapling.Despawn();
			_spawnable = false;
			return result;
		}

		private void OnEnable()
		{
			_spawnableCool = _spawnableTime;
			_spawnable = true;
		}

		private void OnDisable()
		{
			_spawnableCool = _spawnableTime;
			_spawnable = true;
		}

		private void Update()
		{
			if (!(_spawnableCool <= 0f))
			{
				_spawnableCool -= Chronometer.global.deltaTime;
			}
		}
	}
}
