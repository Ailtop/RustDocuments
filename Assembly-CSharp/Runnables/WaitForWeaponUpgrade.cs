using System.Collections;
using Level.Npc;
using UnityEngine;

namespace Runnables
{
	public sealed class WaitForWeaponUpgrade : CRunnable
	{
		[SerializeField]
		private Arachne _arachne;

		public override IEnumerator CRun()
		{
			yield return _arachne.CUpgrade();
		}
	}
}
