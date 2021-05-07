using UnityEngine;

namespace Characters.Projectiles
{
	public class Rotate : MonoBehaviour
	{
		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private float _amount;

		private void Update()
		{
			float num = _amount * _projectile.owner.chronometer.projectile.deltaTime;
			num *= Mathf.Sign(_projectile.transform.localScale.x);
			base.transform.Rotate(Vector3.forward, num, Space.Self);
		}
	}
}
