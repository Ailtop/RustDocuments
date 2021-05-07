using UnityEngine;

namespace Characters.Operations
{
	public class MoveTo : TargetedCharacterOperation
	{
		[SerializeField]
		private Transform _position;

		public override void Run(Character owner, Character target)
		{
			target.transform.position = _position.position;
		}
	}
}
