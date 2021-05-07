using UnityEngine;

namespace Characters.AI.Pope
{
	public class Point : MonoBehaviour
	{
		public enum Tag
		{
			None,
			Top,
			Center,
			Opposition,
			Inner
		}

		[SerializeField]
		private Tag _tag;

		[SerializeField]
		[Range(1f, 5f)]
		private int _floor;

		public Tag tag => _tag;

		public int floor => _floor;
	}
}
