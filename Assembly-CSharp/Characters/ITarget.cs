using Level;
using UnityEngine;

namespace Characters
{
	public interface ITarget
	{
		Collider2D collider { get; }

		Transform transform { get; }

		Character character { get; }

		DestructibleObject damageable { get; }
	}
}
