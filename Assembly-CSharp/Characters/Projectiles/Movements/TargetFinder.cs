using System;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	[Serializable]
	public class TargetFinder
	{
		public enum Method
		{
			Closest,
			First,
			Random
		}

		private delegate Target FindDelegate(IReadOnlyList<Collider2D> result);

		private static readonly NonAllocOverlapper _overlapper = new NonAllocOverlapper(15);

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

		[SerializeField]
		private Method _method;

		[SerializeField]
		private Collider2D _range;

		private Projectile _projectile;

		private FindDelegate _finder;

		public Collider2D range => _range;

		internal void Initialize(Projectile projectile)
		{
			_projectile = projectile;
			switch (_method)
			{
			case Method.Closest:
				_finder = FindClosest;
				break;
			case Method.First:
				_finder = FindFirst;
				break;
			case Method.Random:
				_finder = FindRandom;
				break;
			}
		}

		public Target Find()
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(_projectile.gameObject));
			_range.enabled = true;
			_overlapper.OverlapCollider(_range);
			_range.enabled = false;
			return _finder(_overlapper.results);
		}

		private Target FindClosest(IReadOnlyList<Collider2D> result)
		{
			Target result2 = null;
			float num = float.MaxValue;
			for (int i = 0; i < result.Count; i++)
			{
				Target component = result[i].GetComponent<Target>();
				if (!(component == null))
				{
					Vector2 vector = _projectile.transform.position;
					float sqrMagnitude = ((Vector2)component.transform.position - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result2 = component;
					}
				}
			}
			return result2;
		}

		private Target FindFirst(IReadOnlyList<Collider2D> result)
		{
			return result.GetComponent<Collider2D, Target>();
		}

		private Target FindRandom(IReadOnlyList<Collider2D> result)
		{
			List<Target> components = result.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				return null;
			}
			return components.Random();
		}
	}
}
