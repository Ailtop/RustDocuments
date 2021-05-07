using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters
{
	public static class TargetFinder
	{
		public static Character FindClosestTarget(NonAllocOverlapper overlapper, Collider2D range, Collider2D ownerCollider)
		{
			List<Target> components = overlapper.OverlapCollider(range).GetComponents<Target>();
			if (components.Count == 0)
			{
				return null;
			}
			if (components.Count == 1)
			{
				return components[0].character;
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < components.Count; i++)
			{
				Collider2D collider = components[i].collider;
				if (components[i].character != null)
				{
					collider = components[i].character.collider;
				}
				float distance = Physics2D.Distance(collider, ownerCollider).distance;
				if (num > distance)
				{
					index = i;
					num = distance;
				}
			}
			return components[index].character;
		}
	}
}
