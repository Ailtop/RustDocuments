using System.Collections.Generic;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Pope
{
	public sealed class Navigation : MonoBehaviour
	{
		[SerializeField]
		private Transform _pointContainer;

		[SerializeField]
		private Collider2D _platformArea;

		private Point.Tag _destinationTag;

		private Point _top;

		private Point _center;

		private List<Point> _inners = new List<Point>();

		public Transform destination { get; set; }

		public Point.Tag destinationTag
		{
			get
			{
				return _destinationTag;
			}
			set
			{
				switch (value)
				{
				case Point.Tag.Center:
					destination = _center.transform;
					break;
				case Point.Tag.Top:
					destination = _top.transform;
					break;
				case Point.Tag.None:
					destination = _pointContainer.GetChild(Random.Range(0, _pointContainer.childCount - 1));
					break;
				case Point.Tag.Opposition:
				{
					Character player = Singleton<Service>.Instance.levelManager.player;
					int floor = GetFloor(player.transform.position.y);
					Point[] componentsInChildren = _pointContainer.GetComponentsInChildren<Point>();
					foreach (Point point in componentsInChildren)
					{
						if (point.tag == Point.Tag.Opposition && point.floor == floor && Mathf.Sign(Map.Instance.bounds.center.x - player.transform.position.x) != Mathf.Sign(Map.Instance.bounds.center.x - point.transform.position.x))
						{
							destination = point.transform;
							break;
						}
					}
					break;
				}
				case Point.Tag.Inner:
					destination = _inners.Random().transform;
					break;
				}
			}
		}

		private int GetFloor(float target)
		{
			Bounds bounds = _platformArea.bounds;
			float num = (bounds.max.y - bounds.min.y) / 5f;
			if (target < bounds.min.y + num)
			{
				return 1;
			}
			if (target < bounds.min.y + num * 2f)
			{
				return 2;
			}
			if (target < bounds.min.y + num * 3f)
			{
				return 3;
			}
			if (target < bounds.min.y + num * 4f)
			{
				return 4;
			}
			return 5;
		}

		private void Awake()
		{
			Point[] componentsInChildren = _pointContainer.GetComponentsInChildren<Point>();
			foreach (Point point in componentsInChildren)
			{
				if (point.tag == Point.Tag.Top)
				{
					_top = point;
				}
				else if (point.tag == Point.Tag.Center)
				{
					_center = point;
				}
				else if (point.tag == Point.Tag.Inner)
				{
					_inners.Add(point);
				}
			}
		}
	}
}
