using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class RiderEndingOperations : MonoBehaviour
	{
		[SerializeField]
		[Subcomponent(typeof(Operation))]
		private CharacterOperation.Subcomponents _operations;

		public void Initialize()
		{
			_operations.Initialize();
		}

		public void Run(Character owner)
		{
			_operations.Run(owner);
		}
	}
}
