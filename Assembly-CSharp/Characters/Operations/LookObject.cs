using UnityEngine;

namespace Characters.Operations
{
	public class LookObject : CharacterOperation
	{
		[SerializeField]
		private GameObject _target;

		public override void Run(Character owner)
		{
			owner.ForceToLookAt(_target.transform.position.x);
		}
	}
}
