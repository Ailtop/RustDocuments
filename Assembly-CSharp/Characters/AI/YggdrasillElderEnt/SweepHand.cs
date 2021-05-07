using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class SweepHand : MonoBehaviour
	{
		[GetComponent]
		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private GameObject _monterBody;

		[SerializeField]
		private GameObject _effects;

		[SerializeField]
		private YggdrasillElderEntCollisionDetector _collisionDetector;

		public void Attack()
		{
			_collisionDetector.Initialize(_monterBody, _collider);
			StartCoroutine(_collisionDetector.CRun(base.transform));
			_effects.gameObject.SetActive(true);
		}

		public void Stop()
		{
			_collisionDetector.Stop();
			_effects.gameObject.SetActive(false);
		}
	}
}
