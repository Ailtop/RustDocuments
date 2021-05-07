using UnityEngine;

namespace Characters.Operations
{
	public class EnergyBombTakeAim : CharacterOperation
	{
		[SerializeField]
		private Transform[] _centerAxisPositions;

		[SerializeField]
		private float _term = 2f;

		public override void Run(Character owner)
		{
			Bounds bounds = GetBounds(owner);
			int num = _centerAxisPositions.Length;
			float num2 = (bounds.size.x - _term * (float)num) / (float)num;
			for (int i = 0; i < num; i++)
			{
				float min = ((i == 0) ? 0f : (num2 * (float)i + _term * (float)i));
				float max = num2 * (float)(i + 1) + _term * (float)i;
				float num3 = Random.Range(min, max);
				Vector3 vector = new Vector3(bounds.min.x + num3, bounds.max.y) - _centerAxisPositions[i].transform.position;
				float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				_centerAxisPositions[i].rotation = Quaternion.Euler(0f, 0f, z);
			}
		}

		private Bounds GetBounds(Character owner)
		{
			return Physics2D.Raycast(owner.transform.position, Vector2.down, 20f, 262144).collider.bounds;
		}
	}
}
