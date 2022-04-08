using UnityEngine;

namespace Facepunch.UI
{
	public class ESPCanvas : MonoBehaviour
	{
		[Tooltip("Max amount of elements to show at once")]
		public int MaxElements;

		[Tooltip("Amount of times per second we should update the visible panels")]
		public float RefreshRate = 5f;

		[Tooltip("This object will be duplicated in place")]
		public ESPPlayerInfo Source;

		[Tooltip("Entities this far away won't be overlayed")]
		public float MaxDistance = 64f;
	}
}
