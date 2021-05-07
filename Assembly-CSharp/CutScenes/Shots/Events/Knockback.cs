using Characters;
using Characters.Movements;
using Runnables;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public sealed class Knockback : Event
	{
		[SerializeField]
		private Runnables.Target _giver;

		[SerializeField]
		private Runnables.Target _taker;

		[SerializeField]
		private PushInfo _pushInfo = new PushInfo(false, false);

		public override void Run()
		{
			Character character = _taker.character;
			Character character2 = _giver.character;
			character.movement.push.ApplyKnockback(character2, _pushInfo);
		}
	}
}
