using System;
using UnityEngine;

namespace Level
{
	public class AdventurerRewardSlot : MonoBehaviour
	{
		[NonSerialized]
		public DroppedGear droppedGear;

		[SerializeField]
		private Transform _displayPosition;

		public Vector3 displayPosition => _displayPosition.position;
	}
}
