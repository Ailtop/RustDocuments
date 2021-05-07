using System.Collections;
using Level;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Runnables
{
	public class ChangeGlobalLightIntensity : CRunnable
	{
		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private float _intensity;

		public override IEnumerator CRun()
		{
			Light2D globalLight = Map.Instance.globalLight;
			float startIntensity = globalLight.intensity;
			float elapsed = 0f;
			while (elapsed < _curve.duration)
			{
				elapsed += Chronometer.global.deltaTime;
				globalLight.intensity = Mathf.Lerp(startIntensity, _intensity, _curve.Evaluate(elapsed));
				yield return null;
			}
			globalLight.intensity = _intensity;
		}
	}
}
