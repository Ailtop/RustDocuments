using System.Collections;
using Data;
using TMPro;
using UnityEngine;

namespace Level.Specials
{
	public class TimeCostEventDisplay : MonoBehaviour
	{
		[SerializeField]
		private TimeCostEvent _costEvent;

		[SerializeField]
		private TextMeshPro _text;

		[SerializeField]
		private InteractiveObject _reward;

		private const string soldOutString = "----------";

		private string _soldOutStringcache;

		private void Awake()
		{
			UpdateText((int)_costEvent.GetValue());
		}

		private void Update()
		{
			if (!(_costEvent == null))
			{
				if (!_reward.activated && _soldOutStringcache != "----------")
				{
					_text.color = Color.white;
					_text.text = "----------";
				}
				else
				{
					UpdateTextColor((int)_costEvent.GetValue());
				}
			}
		}

		public void UpdateDisplay()
		{
			StartCoroutine("CAnimate");
		}

		private IEnumerator CAnimate()
		{
			float elapsed = 0f;
			int start = int.Parse(_text.text);
			int dest = (int)_costEvent.GetValue();
			while (elapsed < _costEvent.updateInterval && _reward.activated)
			{
				int value = (int)Mathf.Lerp(start, dest, elapsed / _costEvent.updateInterval);
				UpdateText(value);
				elapsed += Chronometer.global.deltaTime;
				yield return null;
			}
			UpdateText(dest);
		}

		private void UpdateText(int value)
		{
			_text.text = value.ToString();
		}

		private void UpdateTextColor(int cost)
		{
			_text.color = (GameData.Currency.gold.Has(cost) ? Color.white : Color.red);
		}
	}
}
