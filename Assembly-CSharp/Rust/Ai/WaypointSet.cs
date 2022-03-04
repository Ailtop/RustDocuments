using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai
{
	public class WaypointSet : MonoBehaviour, IServerComponent
	{
		public enum NavModes
		{
			Loop = 0,
			PingPong = 1
		}

		[Serializable]
		public struct Waypoint
		{
			public Transform Transform;

			public float WaitTime;

			public Transform[] LookatPoints;

			[NonSerialized]
			public bool IsOccupied;
		}

		[SerializeField]
		private List<Waypoint> _points = new List<Waypoint>();

		[SerializeField]
		private NavModes navMode;

		public List<Waypoint> Points
		{
			get
			{
				return _points;
			}
			set
			{
				_points = value;
			}
		}

		public NavModes NavMode => navMode;

		private void OnDrawGizmos()
		{
			for (int i = 0; i < Points.Count; i++)
			{
				Transform transform = Points[i].Transform;
				if (transform != null)
				{
					if (Points[i].IsOccupied)
					{
						Gizmos.color = Color.red;
					}
					else
					{
						Gizmos.color = Color.cyan;
					}
					Gizmos.DrawSphere(transform.position, 0.25f);
					Gizmos.color = Color.cyan;
					if (i + 1 < Points.Count)
					{
						Gizmos.DrawLine(transform.position, Points[i + 1].Transform.position);
					}
					else if (NavMode == NavModes.Loop)
					{
						Gizmos.DrawLine(transform.position, Points[0].Transform.position);
					}
					Gizmos.color = Color.magenta - new Color(0f, 0f, 0f, 0.5f);
					Transform[] lookatPoints = Points[i].LookatPoints;
					foreach (Transform transform2 in lookatPoints)
					{
						Gizmos.DrawSphere(transform2.position, 0.1f);
						Gizmos.DrawLine(transform.position, transform2.position);
					}
				}
			}
		}
	}
}
