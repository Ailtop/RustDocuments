using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using Characters.Projectiles;
using UnityEngine;

namespace Level.Traps
{
	public class PlateTable : MonoBehaviour
	{
		[SerializeField]
		private Prop _prop;

		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private float _damage;

		[SerializeField]
		private int _quantity;

		[SerializeField]
		private Collider2D _fireRange;

		private void Awake()
		{
			_prop.onDidHit += OnPropDidHit;
		}

		private void OnPropDidHit(Character from, [In][IsReadOnly] ref Damage damage, Vector2 force)
		{
			if (_prop.phase != 0)
			{
				_prop.onDidHit -= OnPropDidHit;
				float num = Mathf.Atan2(force.y, force.x) * 57.29578f;
				for (int i = 0; i < _quantity; i++)
				{
					float speedMultiplier = Mathf.Clamp(force.magnitude, 2f, 6f);
					Vector2 vector = MMMaths.RandomPointWithinBounds(_fireRange.bounds);
					_projectile.reusable.Spawn(vector).GetComponent<Projectile>().Fire(from, _damage, num + Random.Range(-10f, 10f), false, false, speedMultiplier);
				}
			}
		}
	}
}
