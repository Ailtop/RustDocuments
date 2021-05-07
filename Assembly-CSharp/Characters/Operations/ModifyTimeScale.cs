using UnityEngine;

namespace Characters.Operations
{
	public class ModifyTimeScale : CharacterOperation
	{
		[SerializeField]
		protected ChronoInfo _chronoToGlobe;

		[SerializeField]
		protected ChronoInfo _chronoToTarget;

		public override void Run(Character target)
		{
			_chronoToTarget.ApplyTo(target);
			_chronoToGlobe.ApplyGlobe();
		}
	}
}
