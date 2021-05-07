using System.Collections.Generic;
using Characters.AI;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	public sealed class CommandToSacrifice : CharacterOperation
	{
		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private EffectInfo _effect;

		public override void Run(Character owner)
		{
			List<Character> list = _aiController.FindEnemiesInRange(_range);
			if (list == null || list.Count <= 0)
			{
				return;
			}
			foreach (Character item in list)
			{
				SacrificeCharacter component = item.GetComponent<SacrificeCharacter>();
				if (!(component == null))
				{
					component.Run();
					_effect.Spawn(component.transform.position);
				}
			}
		}
	}
}
