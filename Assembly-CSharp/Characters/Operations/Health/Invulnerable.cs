using System.Collections;
using UnityEngine;

namespace Characters.Operations.Health
{
	public class Invulnerable : CharacterOperation
	{
		[SerializeField]
		[FrameTime]
		private float _duration;

		private CoroutineReference _runReference;

		private Character _owner;

		public override void Run(Character owner)
		{
			_owner = owner;
			_runReference.Stop();
			_runReference = this.StartCoroutineWithReference(CRun(owner));
		}

		private IEnumerator CRun(Character character)
		{
			character.invulnerable.Attach(this);
			yield return character.chronometer.master.WaitForSeconds(_duration);
			character.invulnerable.Detach(this);
		}

		public override void Stop()
		{
			_owner?.invulnerable.Detach(this);
		}
	}
}
