using UnityEngine;

namespace Characters.Projectiles.Movements.SubMovements
{
	public abstract class SubMovement : MonoBehaviour
	{
		public abstract void Move(Projectile projectile);
	}
}
