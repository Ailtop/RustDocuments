using UnityEngine;

namespace Characters.Operations
{
	public class Polymorph : CharacterOperation
	{
		[SerializeField]
		private PolymorphBody _polymorph;

		[SerializeField]
		private float _duration;

		public override void Run(Character target)
		{
			_polymorph.character = target;
			_polymorph.StartPolymorph(_duration);
		}
	}
}
