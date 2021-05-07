using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
	public class BossAppearnaceText : MonoBehaviour
	{
		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private TextMeshProUGUI _name;

		[SerializeField]
		private TextMeshProUGUI _subname;

		public new string name
		{
			get
			{
				return _name.text;
			}
			set
			{
				_name.text = value;
			}
		}

		public string subname
		{
			get
			{
				return _subname.text;
			}
			set
			{
				_subname.text = value;
			}
		}

		public bool visible
		{
			get
			{
				return _container.activeSelf;
			}
			set
			{
				_container.SetActive(value);
			}
		}

		public void Appear(string name, string subName, float duration = 1.7f)
		{
			_name.text = name;
			_subname.text = subName;
			visible = true;
			StartCoroutine(CAppear(duration));
		}

		private IEnumerator CAppear(float duration)
		{
			_name.color = new Color(1f, 1f, 1f, 0f);
			_subname.color = new Color(1f, 1f, 1f, 0f);
			Color baseColor = new Color(1f, 1f, 1f, 0f);
			Color destinationColor = new Color(1f, 1f, 1f, 1f);
			float elapsed = 0f;
			while (true)
			{
				yield return null;
				Color color = Color.Lerp(baseColor, destinationColor, elapsed / duration);
				_name.color = color;
				_subname.color = color;
				if (!(elapsed > duration))
				{
					elapsed += Chronometer.global.deltaTime;
					continue;
				}
				break;
			}
		}

		public void Disappear(float duration = 1.7f)
		{
			StartCoroutine(CDisappear(duration));
		}

		private IEnumerator CDisappear(float duration)
		{
			Color baseColor = new Color(1f, 1f, 1f, 1f);
			Color destinationColor = new Color(1f, 1f, 1f, 0f);
			float elapsed = 0f;
			while (true)
			{
				yield return null;
				Color color = Color.Lerp(baseColor, destinationColor, elapsed / duration);
				_name.color = color;
				_subname.color = color;
				if (elapsed > duration)
				{
					break;
				}
				elapsed += Chronometer.global.deltaTime;
			}
			visible = false;
		}

		public IEnumerator ShowAndHideText(string name, string subName)
		{
			Appear(name, subName, 0.5f);
			yield return Chronometer.global.WaitForSeconds(2.5f);
			yield return CDisappear(0.5f);
		}
	}
}
