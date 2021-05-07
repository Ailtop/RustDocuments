using System.Collections;
using Characters.Actions;
using Characters.AI.Hero;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class KilivanFinish : Behaviour
	{
		[SerializeField]
		private Action _ready;

		[SerializeField]
		private Action _throw;

		[SerializeField]
		private Action _attack;

		[SerializeField]
		private Characters.AI.Hero.KilivanFinish _kilivanProjectile;

		public override IEnumerator CRun(AIController controller)
		{
			_ready.TryStart();
			while (_ready.running)
			{
				yield return null;
			}
			_throw.TryStart();
			yield return _kilivanProjectile.CFire();
			_attack.TryStart();
			while (_attack.running)
			{
				yield return null;
			}
		}
	}
}
