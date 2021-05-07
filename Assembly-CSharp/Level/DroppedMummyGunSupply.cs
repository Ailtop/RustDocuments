using System.Collections;
using UnityEngine;

namespace Level
{
	public class DroppedMummyGunSupply : MonoBehaviour
	{
		[SerializeField]
		private PoolObject _poolObject;

		[SerializeField]
		private SpriteRenderer _parachuteRenderer;

		[SerializeField]
		private float _fallSpeed;

		private DroppedMummyGun _gun;

		private float _targetY;

		private RigidbodyConstraints2D _rigidbodyConstraints;

		public void Spawn(DroppedMummyGun droppedMummyGun, Vector3 position, float targetY)
		{
			_poolObject.Spawn(position).GetComponent<DroppedMummyGunSupply>().Initialize(droppedMummyGun, targetY);
		}

		private void Initialize(DroppedMummyGun droppedMummyGun, float targetY)
		{
			_gun = droppedMummyGun;
			_gun.onPickedUp += OnGunPickedUp;
			_gun.transform.SetParent(base.transform, false);
			_gun.transform.localPosition = Vector3.zero;
			_rigidbodyConstraints = _gun.rigidbody.constraints;
			_gun.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
			_targetY = targetY;
			_parachuteRenderer.enabled = true;
			StartCoroutine(CFall());
		}

		private void OnGunPickedUp()
		{
			_gun.onPickedUp -= OnGunPickedUp;
			_gun.rigidbody.constraints = _rigidbodyConstraints;
			_poolObject.Despawn();
		}

		private IEnumerator CFall()
		{
			do
			{
				yield return null;
				base.transform.Translate(0f, (0f - _fallSpeed) * Chronometer.global.deltaTime, 0f);
			}
			while (!(base.transform.position.y < _targetY));
			Vector3 position = base.transform.position;
			position.y = _targetY;
			base.transform.position = position;
			_parachuteRenderer.enabled = false;
		}
	}
}
