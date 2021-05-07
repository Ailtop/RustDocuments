using System.Collections;
using Characters;
using FX;
using Singletons;
using UnityEngine;

namespace Tutorials
{
	public class Witch : MonoBehaviour
	{
		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SoundInfo _meowSoundInfo;

		[SerializeField]
		private Character _summon;

		public IEnumerator EscapeCage()
		{
			_animator.Play("Idle_Human");
			yield return Chronometer.global.WaitForSeconds(1f);
		}

		public IEnumerator TurnIntoCat()
		{
			_animator.Play("Polymorph_Cat");
			yield return Chronometer.global.WaitForSeconds(2f);
			_summon.gameObject.SetActive(true);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_meowSoundInfo, base.transform.position);
		}
	}
}
