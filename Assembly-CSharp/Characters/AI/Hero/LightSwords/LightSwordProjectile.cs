using System.Collections;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class LightSwordProjectile : MonoBehaviour
	{
		[SerializeField]
		private GameObject _body;

		[SerializeField]
		private float _duration = 0.5f;

		public IEnumerator CFire(Vector2 firePosition, Vector2 destination, float angle)
		{
			Initialize(firePosition, angle);
			Show();
			yield return CMove(firePosition, destination);
			Hide();
		}

		private IEnumerator CMove(Vector2 src, Vector2 dest)
		{
			float elapsed = 0f;
			while (elapsed < _duration)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				base.transform.position = Vector2.Lerp(src, dest, elapsed / _duration);
			}
			base.transform.position = dest;
		}

		private void Initialize(Vector2 position, float angle)
		{
			base.transform.position = position;
			_body.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		}

		private void Show()
		{
			_body.SetActive(true);
		}

		public void Hide()
		{
			_body.SetActive(false);
		}
	}
}
