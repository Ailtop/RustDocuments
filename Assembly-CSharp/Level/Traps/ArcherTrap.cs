using System.Collections;
using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class ArcherTrap : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _readyOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _activeOperations;

		[SerializeField]
		private float _activeDelay;

		[SerializeField]
		private float _lifeTime;

		private void Awake()
		{
			_readyOperations.Initialize();
			_activeOperations.Initialize();
		}

		private void OnEnable()
		{
			Ready();
			StartCoroutine(CActivate());
		}

		private void Ready()
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(_character);
		}

		private void Hide()
		{
			_character.gameObject.SetActive(false);
		}

		private IEnumerator CActivate()
		{
			yield return Chronometer.global.WaitForSeconds(_activeDelay);
			_activeOperations.gameObject.SetActive(true);
			_activeOperations.Run(_character);
			StartCoroutine(CSleep());
		}

		private IEnumerator CSleep()
		{
			yield return Chronometer.global.WaitForSeconds(_lifeTime);
			Hide();
		}
	}
}
