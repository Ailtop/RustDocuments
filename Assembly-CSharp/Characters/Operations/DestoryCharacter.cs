using UnityEngine;

namespace Characters.Operations
{
	public class DestoryCharacter : CharacterOperation
	{
		[SerializeField]
		private Character _character;

		public override void Run(Character owner)
		{
			_character.gameObject.SetActive(false);
		}
	}
}
