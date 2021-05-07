using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI
{
	public class ScareCrawAI : AIController
	{
		[SerializeField]
		private Action _appear;

		protected override IEnumerator CProcess()
		{
			yield break;
		}

		public void Appear()
		{
			_appear.TryStart();
		}
	}
}
