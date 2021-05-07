using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Runnables
{
	public sealed class RunOperations : Runnable
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		[SerializeField]
		private Target _owner;

		private void Awake()
		{
			_operations.Initialize();
		}

		public override void Run()
		{
			StartCoroutine(_operations.CRun(_owner.character));
		}
	}
}
