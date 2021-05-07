using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Level.MapEvent.Behavior
{
	public class UpdateLight : Behavior
	{
		[SerializeField]
		private Light2D _light;

		[SerializeField]
		[ColorUsage(false)]
		private Color _color;

		[SerializeField]
		private float _intensity;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		[Range(0.1f, 10f)]
		private float _duration;

		private static Dictionary<Light2D, Coroutine> _coroutines = new Dictionary<Light2D, Coroutine>();

		public override void Run()
		{
			if (_coroutines.ContainsKey(_light))
			{
				StopCoroutine(_coroutines[_light]);
				_coroutines.Remove(_light);
			}
			_coroutines.Add(_light, StartCoroutine(CRun()));
		}

		private IEnumerator CRun()
		{
			float elapsed = 0f;
			Color colorFrom = _light.color;
			float intensityFrom = _light.intensity;
			while (elapsed < _duration)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				float num = _curve.Evaluate(elapsed / _duration);
				_light.color = colorFrom + (_color - colorFrom) * num;
				_light.intensity = intensityFrom + (_intensity - intensityFrom) * num;
			}
			_light.color = _color;
			_light.intensity = _intensity;
			_coroutines.Remove(_light);
		}
	}
}
