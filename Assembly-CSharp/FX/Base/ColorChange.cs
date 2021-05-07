using System;
using System.Collections;
using UnityEngine;

namespace FX.Base
{
	public class ColorChange : MonoBehaviour
	{
		private const string shader = "2DxFX/Standard/ColorRGB";

		[NonSerialized]
		public bool ActiveChange = true;

		[NonSerialized]
		public bool AddShadow = true;

		[NonSerialized]
		public bool ReceivedShadow;

		[NonSerialized]
		public int BlendMode;

		[SerializeField]
		[Range(0f, 1f)]
		public float _Alpha = 1f;

		[SerializeField]
		[Range(-1f, 1f)]
		public float _ColorR = 1f;

		[SerializeField]
		[Range(-1f, 1f)]
		public float _ColorG = 1f;

		[SerializeField]
		[Range(-1f, 1f)]
		public float _ColorB = 1f;

		[SerializeField]
		public float runTime = 0.2f;

		[SerializeField]
		private EasingFunction.Method _runEaseMethod = EasingFunction.Method.Linear;

		[SerializeField]
		private Renderer _renderer;

		private EasingFunction _runEase;

		private float elapsedTime;

		private Material tempMaterial;

		private Material defaultMaterial;

		private void Awake()
		{
			_runEase = new EasingFunction(_runEaseMethod);
			defaultMaterial = new Material(Shader.Find("Sprites/Default"));
		}

		public void Run(Chronometer chronometer)
		{
			ActiveChange = true;
			tempMaterial = new Material(Shader.Find("2DxFX/Standard/ColorRGB"));
			tempMaterial.hideFlags = HideFlags.None;
			_renderer.sharedMaterial = tempMaterial;
			_renderer.sharedMaterial.SetFloat("_Alpha", 1f - _Alpha);
			_renderer.sharedMaterial.SetFloat("_ColorR", _ColorR);
			_renderer.sharedMaterial.SetFloat("_ColorG", _ColorG);
			_renderer.sharedMaterial.SetFloat("_ColorB", _ColorB);
			StartCoroutine(ChangeColor(chronometer));
		}

		private IEnumerator ChangeColor(Chronometer chronometer)
		{
			float t = 0f;
			do
			{
				yield return null;
				_renderer.sharedMaterial.SetFloat("_ColorR", _runEase.function(_ColorR, 0f, t / runTime));
				_renderer.sharedMaterial.SetFloat("_ColorG", _runEase.function(_ColorG, 0f, t / runTime));
				_renderer.sharedMaterial.SetFloat("_ColorB", _runEase.function(_ColorB, 0f, t / runTime));
				t += chronometer.deltaTime;
			}
			while (!(t > runTime));
			_renderer.sharedMaterial = defaultMaterial;
			ActiveChange = false;
		}
	}
}
