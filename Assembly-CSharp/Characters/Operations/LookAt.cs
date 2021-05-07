using Characters.Operations.LookAtTargets;
using UnityEngine;

namespace Characters.Operations
{
	public class LookAt : CharacterOperation
	{
		[SerializeField]
		[Characters.Operations.LookAtTargets.Target.Subcomponent]
		private Characters.Operations.LookAtTargets.Target _target;

		public override void Run(Character owner)
		{
			owner.ForceToLookAt(_target.GetDirectionFrom(owner));
		}
	}
}
