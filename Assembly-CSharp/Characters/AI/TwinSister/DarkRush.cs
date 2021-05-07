using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class DarkRush : MonoBehaviour
	{
		[SerializeField]
		private Action _standing;

		[Header("MeleeAttack")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(TeleportBehind))]
		private TeleportBehind _teleportBehind;

		[SerializeField]
		private Action _fristAttack;

		[SerializeField]
		private Action _secondAttack;

		[Header("Last Attack")]
		[SerializeField]
		private Action _finishAttack;

		[Space]
		[SerializeField]
		private ParentPool _parentPool;

		public IEnumerator CRun(DarkAideAI darkAideAI)
		{
			yield return _teleportBehind.CRun(darkAideAI);
			_fristAttack.TryStart();
			while (_fristAttack.running)
			{
				yield return null;
			}
			yield return _teleportBehind.CRun(darkAideAI);
			_secondAttack.TryStart();
			while (_secondAttack.running)
			{
				yield return null;
			}
			yield return CFinishAttack();
			while (_standing.running)
			{
				yield return null;
			}
		}

		private IEnumerator CFinishAttack()
		{
			_finishAttack.TryStart();
			while (_finishAttack.running)
			{
				yield return null;
			}
			DarkRushEffect[] componentsInChildren = _parentPool.currentEffectParent.GetComponentsInChildren<DarkRushEffect>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].HideSign();
			}
			_standing.TryStart();
			componentsInChildren = _parentPool.currentEffectParent.GetComponentsInChildren<DarkRushEffect>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ShowImpact();
			}
			yield return Chronometer.global.WaitForSeconds(1f);
			componentsInChildren = _parentPool.currentEffectParent.GetComponentsInChildren<DarkRushEffect>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].HideImpact();
			}
		}

		public bool CanUse()
		{
			return _finishAttack.canUse;
		}
	}
}
