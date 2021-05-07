using UnityEngine;

namespace Characters.Operations
{
	public class TranslateDestination : CharacterOperation
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Transform _destination;

		public override void Run(Character owner)
		{
			_target.position = _destination.position;
		}
	}
}
