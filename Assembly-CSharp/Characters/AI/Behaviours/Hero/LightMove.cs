using System.Collections;
using Characters.Actions;
using Characters.AI.Hero.LightSwords;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public abstract class LightMove : Behaviour
	{
		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Action _move;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			LightSword sword = GetDestination();
			if (sword == null)
			{
				Debug.LogError("Sword is Null in LightMove");
			}
			_destination.position = sword.GetStuckPosition();
			sword.Sign();
			_move.TryStart();
			while (_move.running)
			{
				yield return null;
			}
			sword.Despawn();
			base.result = Result.Success;
		}

		protected abstract LightSword GetDestination();
	}
}
