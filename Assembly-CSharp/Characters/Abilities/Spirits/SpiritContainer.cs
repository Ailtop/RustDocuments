using Services;
using UnityEngine;

namespace Characters.Abilities.Spirits
{
	public class SpiritContainer : MonoBehaviour
	{
		[SerializeField]
		private Spirit _spirit;

		private void Awake()
		{
			_spirit.transform.SetParent(null, false);
		}

		private void OnEnable()
		{
			_spirit.gameObject.SetActive(true);
		}

		private void OnDisable()
		{
			if (!Service.quitting)
			{
				_spirit.gameObject.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Object.Destroy(_spirit.gameObject);
			}
		}
	}
}
