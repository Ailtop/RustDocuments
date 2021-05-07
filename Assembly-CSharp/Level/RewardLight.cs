using System.Collections;
using Characters;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Level
{
	public class RewardLight : MonoBehaviour
	{
		[SerializeField]
		[GetComponentInParent(false)]
		private Map _map;

		[SerializeField]
		private float _intensityMultifly = 0.7f;

		[SerializeField]
		private Light2D _godRay;

		[SerializeField]
		private float _fadeDuration;

		private Character _player;

		private Coroutine _fade;

		private float _defaultIntensity;

		private float _defaultGodRayIntensity;

		private void Awake()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
			_defaultIntensity = _map.globalLight.intensity;
			_defaultGodRayIntensity = _godRay.intensity;
			_godRay.intensity = 0f;
			_godRay.gameObject.SetActive(false);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Character component = collision.GetComponent<Character>();
			if (!(component == null) && !(component != _player))
			{
				Activate();
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			Character component = collision.GetComponent<Character>();
			if (!(component == null) && !(component != _player))
			{
				Deactivate();
			}
		}

		private void Activate()
		{
			if (_fade != null)
			{
				StopCoroutine(_fade);
			}
			_godRay.gameObject.SetActive(true);
			_fade = StartCoroutine(CFade(_defaultIntensity * _intensityMultifly, _defaultGodRayIntensity, _fadeDuration));
		}

		private void Deactivate()
		{
			if (_fade != null)
			{
				StopCoroutine(_fade);
			}
			_fade = StartCoroutine(CFade(_defaultIntensity, 0f, _fadeDuration));
		}

		private IEnumerator CFade(float targetIntensity, float targetGodRayIntensity, float duration)
		{
			while (_map.waveContainer.state == EnemyWaveContainer.State.Remain)
			{
				yield return null;
			}
			float start = Time.time;
			float defaultIntensity = _map.globalLight.intensity;
			float defaultGodRayIntensity = _godRay.intensity;
			while (Time.time - start < duration)
			{
				yield return null;
				float t = (Time.time - start) / duration;
				_map.globalLight.intensity = Mathf.Lerp(defaultIntensity, targetIntensity, t);
				_godRay.intensity = Mathf.Lerp(defaultGodRayIntensity, targetGodRayIntensity, t);
			}
			_map.globalLight.intensity = targetIntensity;
			_godRay.intensity = targetGodRayIntensity;
			if (targetGodRayIntensity <= 0f)
			{
				_godRay.gameObject.SetActive(false);
			}
		}
	}
}
