using System.Collections;
using Characters.AI.Hero;
using UnityEngine;

namespace Characters.Operations
{
	public class PillarOfLightAttack : CharacterOperation
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private float _attackDelay;

		public override void Run(Character owner)
		{
			PillarOfLightContainer component = _container.GetChild(Random.Range(0, _container.childCount)).GetComponent<PillarOfLightContainer>();
			StartCoroutine(CRun(owner, component));
		}

		private IEnumerator CRun(Character owner, PillarOfLightContainer container)
		{
			container.Sign(owner);
			yield return owner.chronometer.master.WaitForSeconds(_attackDelay);
			container.Attack(owner);
		}
	}
}
