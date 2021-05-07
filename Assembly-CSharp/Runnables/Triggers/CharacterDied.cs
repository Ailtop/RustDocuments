using Characters;
using UnityEngine;

namespace Runnables.Triggers
{
	public class CharacterDied : Trigger
	{
		[SerializeField]
		private Character _character;

		protected override bool Check()
		{
			if (_character == null)
			{
				return false;
			}
			return _character.health.dead;
		}
	}
}
