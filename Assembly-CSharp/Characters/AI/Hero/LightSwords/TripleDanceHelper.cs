using System;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class TripleDanceHelper : MonoBehaviour
	{
		private class TripleDanceSword
		{
			internal LightSword left;

			internal LightSword center;

			internal LightSword right;

			internal TripleDanceSword(List<LightSword> swords)
			{
				if (swords != null && swords.Count >= 3)
				{
					left = swords[0];
					center = swords[1];
					right = swords[2];
				}
			}
		}

		[SerializeField]
		private Character _owner;

		[SerializeField]
		private LightSwordPool _pool;

		[SerializeField]
		[Range(180f, 360f)]
		private float _left;

		[SerializeField]
		[Range(180f, 360f)]
		private float _center = 270f;

		[SerializeField]
		[Range(180f, 360f)]
		private float _right;

		private TripleDanceSword _sword;

		private void Start()
		{
			_sword = new TripleDanceSword(_pool.Get());
		}

		public void Fire(Transform source)
		{
			Vector2 destination = CalculateDestination(source.position, _left);
			_sword.left.Fire(_owner, source.position, destination);
			destination = CalculateDestination(source.position, _center);
			_sword.center.Fire(_owner, source.position, destination);
			destination = CalculateDestination(source.position, _right);
			_sword.right.Fire(_owner, source.position, destination);
		}

		public ValueTuple<LightSword, LightSword, LightSword> GetStuck()
		{
			return new ValueTuple<LightSword, LightSword, LightSword>(_sword.left, _sword.center, _sword.right);
		}

		private Vector2 CalculateDestination(Vector2 source, float degree)
		{
			Vector3 vector = Quaternion.Euler(0f, 0f, degree) * Vector2.right;
			return Physics2D.Raycast(source, vector, float.PositiveInfinity, Layers.groundMask).point;
		}
	}
}
