using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class RunOperations : Node
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private void Awake()
		{
			_operations.Initialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			Character target = context.Get<Character>(Key.OwnerCharacter);
			_operations.StopAll();
			StartCoroutine(_operations.CRun(target));
			return NodeState.Success;
		}
	}
}
