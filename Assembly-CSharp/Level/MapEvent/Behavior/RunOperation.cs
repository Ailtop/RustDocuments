using Characters.Operations;
using Runnables;
using UnityEditor;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class RunOperation : Behavior
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
