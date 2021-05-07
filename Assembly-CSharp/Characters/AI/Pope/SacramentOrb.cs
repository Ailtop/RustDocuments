using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope
{
	public class SacramentOrb : MonoBehaviour
	{
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onReady;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAttack;

		[SerializeField]
		private float _delay;

		private Character _character;

		public void Initialize(Character character)
		{
			_onReady.Initialize();
			_onAttack.Initialize();
			_character = character;
		}

		private void OnEnable()
		{
			StartCoroutine(CRun());
		}

		public void ShowSign()
		{
			_onReady.gameObject.SetActive(true);
			_onReady.Run(_character);
		}

		public void Run()
		{
			_onAttack.gameObject.SetActive(true);
			_onAttack.Run(_character);
		}

		private IEnumerator CRun()
		{
			ShowSign();
			yield return _character.chronometer.master.WaitForSeconds(_delay);
			Run();
		}
	}
}
