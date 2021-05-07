using System.Collections;
using Characters;
using Characters.Monsters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Chapter4
{
	public class Platform : MonoBehaviour, IPurification, IDivineCrossHelp
	{
		[Header("Purification")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _readyOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _operations;

		[Space]
		[SerializeField]
		private float _duration;

		[SerializeField]
		private Monster _tentaclePrefab;

		[SerializeField]
		private Transform _spawnPoint;

		[Header("Divine Cross")]
		[SerializeField]
		private Transform _divineCrossFirePosition;

		public bool tentacleAlives { get; set; }

		public Transform firePosition => _divineCrossFirePosition;

		private void Awake()
		{
			_readyOperations.Initialize();
			_operations.Initialize();
		}

		public void ShowSign(Character owner)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(owner);
		}

		public void Purifiy(Character owner)
		{
			StartCoroutine(CRunPurifiy(owner));
		}

		private IEnumerator CRunPurifiy(Character owner)
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass15_.summoned = _tentaclePrefab.Summon(_spawnPoint.position);
			Map.Instance.waveContainer.summonWave.Attach(_003C_003Ec__DisplayClass15_.summoned.character);
			_003C_003Ec__DisplayClass15_.summoned.character.health.onDied += _003C_003Ec__DisplayClass15_._003CCRunPurifiy_003Eg__OnDied_007C0;
			tentacleAlives = true;
			_operations.gameObject.SetActive(true);
			_operations.Run(owner);
			yield return owner.chronometer.master.WaitForSeconds(_duration);
		}

		public Vector3 GetFirePosition()
		{
			return _divineCrossFirePosition.position;
		}
	}
}
