using System;
using System.Collections;
using UnityEngine;

namespace Runnables
{
	public abstract class CRunnable : MonoBehaviour
	{
		public static readonly Type[] types = new Type[8]
		{
			typeof(CharacterTranslateTo),
			typeof(FadeIn),
			typeof(FadeOut),
			typeof(GameFadeIn),
			typeof(GameFadeOut),
			typeof(TransformTranslateTo),
			typeof(WaitForTime),
			typeof(WaitForWeaponUpgrade)
		};

		public abstract IEnumerator CRun();
	}
}
