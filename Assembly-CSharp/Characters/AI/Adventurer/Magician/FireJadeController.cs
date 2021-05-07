using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Adventurer.Magician
{
	public class FireJadeController : AIController
	{
		[SerializeField]
		private Action _attack;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			_attack.TryStart();
		}
	}
}
