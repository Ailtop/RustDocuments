using System;
using Characters.Gear.Weapons.Gauges;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hud
{
	public class GaugeBar : MonoBehaviour
	{
		[Serializable]
		private class Bar
		{
			[SerializeField]
			private Image _mainBar;

			[SerializeField]
			private Vector3 _defaultMainBarScale = Vector3.one;

			private Vector3 _mainScale = Vector3.one;

			public Image mainBar => _mainBar;

			public Vector3 defaultMainBarScale => _defaultMainBarScale;

			public void Reset()
			{
				_mainScale.x = 0f;
			}

			public void SetColor(Color color)
			{
				_mainBar.color = color;
			}

			public void Lerp(float targetPercent)
			{
				_mainScale.x = Mathf.Lerp(_mainScale.x, targetPercent, 0.25f);
				_mainBar.transform.localScale = Vector3.Scale(_mainScale, _defaultMainBarScale);
			}

			public void SetActive(bool value)
			{
				_mainBar.enabled = value;
			}
		}

		[SerializeField]
		private RectTransform _container;

		[SerializeField]
		private TMP_Text _displayText;

		[SerializeField]
		private Bar bar1;

		[SerializeField]
		private Bar bar2;

		private Gauge _gauge;

		public Gauge gauge
		{
			get
			{
				return _gauge;
			}
			set
			{
				_gauge = value;
				Update();
				base.gameObject.SetActive(_gauge != null);
			}
		}

		private void OnEnable()
		{
			bar1.Reset();
			bar2.Reset();
		}

		private void Update()
		{
			if (gauge == null)
			{
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(false);
				}
				return;
			}
			float num = math.clamp(gauge.gaugePercent, 0f, 1f);
			if (!gauge.secondBar)
			{
				bar1.SetColor(_gauge.barColor);
				bar2.SetActive(false);
				bar1.Lerp(num);
			}
			else
			{
				bar1.SetColor(_gauge.barColor);
				bar2.SetColor(_gauge.secondBarColor);
				if (num < 0.5f)
				{
					bar2.SetActive(false);
					bar1.Lerp(num * 2f);
				}
				else
				{
					bar2.SetActive(true);
					bar1.Lerp(1f);
					bar2.Lerp((num - 0.5f) * 2f);
				}
			}
			_displayText.text = gauge.displayText;
		}
	}
}
