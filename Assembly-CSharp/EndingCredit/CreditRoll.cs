using System.Collections;
using Services;
using Singletons;
using UI.Pause;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndingCredit
{
	public class CreditRoll : MonoBehaviour
	{
		[SerializeField]
		[PauseEvent.Subcomponent]
		private PauseEvent _pauseEvent;

		[SerializeField]
		private PauseEventSystem _pauseEventSystem;

		[SerializeField]
		private Input _input;

		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Transform _lastSupporterList;

		[SerializeField]
		private float _delay;

		public IEnumerator CRun()
		{
			_pauseEventSystem.PushEvent(_pauseEvent);
			Vector3 vector = _destination.transform.position - _lastSupporterList.transform.position;
			while (vector.y > 0f)
			{
				yield return null;
				_target.transform.Translate(Vector2.up * Chronometer.global.deltaTime * _input.speed);
				vector = _destination.transform.position - _lastSupporterList.transform.position;
			}
			yield return Chronometer.global.WaitForSeconds(_delay);
			yield return CLoadScene();
		}

		public IEnumerator CLoadScene()
		{
			yield return Singleton<Service>.Instance.fadeInOut.CFadeOut();
			yield return Chronometer.global.WaitForSeconds(2f);
			Hide();
			SceneManager.LoadScene(0);
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			Singleton<Service>.Instance.fadeInOut.FadeIn();
			Hide();
		}
	}
}
