using System.Collections;
using Characters.Gear.Synergy.Keywords.FairyTaleSummon;
using UnityEngine;

namespace Characters.Operations
{
	public class OberonBomb : CharacterOperation
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private float _activateInterval;

		[SerializeField]
		private float _bombDelay = 1f;

		private OberonBombOrb[] _orbs;

		private void Awake()
		{
			_orbs = new OberonBombOrb[_container.childCount];
			for (int i = 0; i < _container.childCount; i++)
			{
				_orbs[i] = _container.GetChild(i).GetComponent<OberonBombOrb>();
			}
		}

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			_orbs.Shuffle();
			yield return CActivate(owner);
			yield return owner.chronometer.master.WaitForSeconds(_bombDelay);
			Deactivate(owner);
		}

		private IEnumerator CActivate(Character owner)
		{
			OberonBombOrb[] orbs = _orbs;
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].Activate(owner);
				yield return owner.chronometer.master.WaitForSeconds(_activateInterval);
			}
		}

		private void Deactivate(Character owner)
		{
			OberonBombOrb[] orbs = _orbs;
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].Deactivate(owner);
			}
		}
	}
}
