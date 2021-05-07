using System.Collections;
using Characters.Operations;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class VenomBreath : Behaviour
	{
		[Header("Ready")]
		[SerializeField]
		private OperationInfos _readyOperations;

		[Header("Fire")]
		[SerializeField]
		private OperationInfos _fireOperations;

		[Header("End")]
		[SerializeField]
		private OperationInfos _endOperations;

		private void Awake()
		{
			_readyOperations.Initialize();
			_fireOperations.Initialize();
			_endOperations.Initialize();
		}

		public void Ready(Character character)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void End(Character character)
		{
			_endOperations.gameObject.SetActive(true);
			_endOperations.Run(character);
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_fireOperations.gameObject.SetActive(true);
			_fireOperations.Run(controller.character);
			base.result = Result.Done;
			yield break;
		}
	}
}
