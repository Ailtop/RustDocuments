using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class TripleDance : Behaviour
	{
		[SerializeField]
		private Behaviour _jumpAndThrow;

		[SerializeField]
		private TripleDanceMove _lightMove;

		[SerializeField]
		private Behaviour _slash;

		[SerializeField]
		private Behaviour _strike;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _throwGhost;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _slashGhost;

		private void Awake()
		{
			_throwGhost.Initialize();
			_slashGhost.Initialize();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _jumpAndThrow.CRun(controller);
			_throwGhost.gameObject.SetActive(true);
			_throwGhost.Run(controller.character);
			yield return _lightMove.CRun(controller);
			yield return _slash.CRun(controller);
			_slashGhost.gameObject.SetActive(true);
			_slashGhost.Run(controller.character);
			yield return _lightMove.CRun(controller);
			yield return _slash.CRun(controller);
			_slashGhost.gameObject.SetActive(true);
			_slashGhost.Run(controller.character);
			yield return _lightMove.CRun(controller);
			yield return _strike.CRun(controller);
			base.result = Result.Success;
		}
	}
}
