using System.Collections;
using Characters;
using Characters.Actions;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Tutorials
{
	public class Hero : MonoBehaviour
	{
		[SerializeField]
		private Character _caharacter;

		[SerializeField]
		private Character _darkOgre;

		[SerializeField]
		private Action _landing;

		[SerializeField]
		private Action _attack;

		public IEnumerator CAppear()
		{
			_caharacter.ForceToLookAt(Character.LookingDirection.Left);
			_landing.TryStart();
			while (_landing.running)
			{
				yield return null;
			}
		}

		public IEnumerator CAttack()
		{
			_attack.TryStart();
			yield return Chronometer.global.WaitForSeconds(1f);
			Scene<GameBase>.instance.cameraController.Shake(0.1f, 2.5f);
			yield return Chronometer.global.WaitForSeconds(2.5f);
			if (!_darkOgre.health.dead)
			{
				_darkOgre.health.Kill();
			}
			Singleton<Service>.Instance.fadeInOut.SetFadeColor(Color.white);
			yield return Singleton<Service>.Instance.fadeInOut.CFadeOut();
			Singleton<Service>.Instance.fadeInOut.SetFadeColor(Color.black);
			while (_attack.running)
			{
				yield return null;
			}
			yield return Chronometer.global.WaitForSeconds(2f);
		}
	}
}
