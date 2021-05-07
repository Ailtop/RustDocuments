using System.Collections;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class GiganticSaintSword : MonoBehaviour
	{
		public delegate void OnStuckDelegate();

		[Header("Projectiles")]
		[SerializeField]
		private GameObject _projectile;

		[SerializeField]
		private float _dropDuration;

		[Header("Stuck")]
		[SerializeField]
		private GameObject _stuck;

		public bool isStuck => _stuck.gameObject.activeSelf;

		public event OnStuckDelegate OnStuck;

		public void Fire(Vector2 firePosition, float destY)
		{
			_projectile.transform.position = firePosition;
			_projectile.SetActive(true);
			StartCoroutine(CMove(destY));
		}

		private IEnumerator CMove(float destY)
		{
			float elapsed = 0f;
			Vector3 source = _projectile.transform.position;
			Vector2 dest = new Vector2(source.x, destY);
			while (elapsed < _dropDuration)
			{
				elapsed += Chronometer.global.deltaTime;
				_projectile.transform.position = Vector2.Lerp(source, dest, elapsed / _dropDuration);
				yield return null;
			}
			_projectile.transform.position = dest;
			Stuck(dest);
		}

		private void Stuck(Vector2 point)
		{
			this.OnStuck?.Invoke();
			_projectile.SetActive(false);
			_stuck.transform.position = point;
			_stuck.SetActive(true);
		}

		public void Despawn()
		{
			_projectile.SetActive(false);
			_stuck.SetActive(false);
		}
	}
}
