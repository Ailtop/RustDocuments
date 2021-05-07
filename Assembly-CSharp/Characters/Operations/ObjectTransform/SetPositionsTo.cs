using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class SetPositionsTo : CharacterOperation
	{
		[SerializeField]
		private Transform[] _objects;

		[SerializeField]
		private Transform[] _targets;

		public override void Run(Character owner)
		{
			_targets.Shuffle();
			for (int i = 0; i < _objects.Length; i++)
			{
				_objects[i].position = _targets[i].position;
			}
		}
	}
}
