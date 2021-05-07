using Services;
using UnityEngine;

namespace Utilities
{
	public class ObjectContainer : MonoBehaviour
	{
		[SerializeField]
		private GameObject _element;

		private void Awake()
		{
			_element.transform.SetParent(null, false);
		}

		private void OnEnable()
		{
			_element.SetActive(true);
		}

		private void OnDisable()
		{
			_element.SetActive(false);
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Object.Destroy(_element);
			}
		}
	}
}
