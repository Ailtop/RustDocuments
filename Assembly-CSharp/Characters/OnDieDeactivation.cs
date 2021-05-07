using UnityEngine;

namespace Characters
{
	public class OnDieDeactivation : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private void Start()
		{
			if (_character == null)
			{
				_character = GetComponentInParent<Character>();
			}
			_character.health.onDied += OnDie;
		}

		private void OnDestroy()
		{
			if (_character != null)
			{
				_character.health.onDied -= OnDie;
			}
		}

		private void OnDie()
		{
			base.gameObject.SetActive(false);
			_character.health.onDied -= OnDie;
		}
	}
}
