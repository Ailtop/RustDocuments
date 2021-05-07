using System;
using UnityEngine;

namespace Housing
{
	public class BuildLevel : MonoBehaviour
	{
		[SerializeField]
		private BuildLevel _next;

		[SerializeField]
		private int _order;

		[SerializeField]
		private int _cost;

		public BuildLevel next
		{
			get
			{
				return _next;
			}
			set
			{
				_next = value;
			}
		}

		public int order
		{
			get
			{
				return _order;
			}
			set
			{
				_order = value;
			}
		}

		public int cost
		{
			get
			{
				return _cost;
			}
			set
			{
				_cost = value;
			}
		}

		public event Action onBuild;

		public event Action onNew;

		public void Build(int buildedOrder, int seen)
		{
			Build(buildedOrder);
			_next?.Build(buildedOrder, seen);
			New(seen);
		}

		public BuildLevel GetLevelAfterPoint(int point)
		{
			if (_order > point)
			{
				return this;
			}
			if (!(_next == null))
			{
				return _next.GetLevelAfterPoint(point);
			}
			return null;
		}

		private void Build(int buildedOrder)
		{
			if (_order <= buildedOrder)
			{
				this.onBuild?.Invoke();
			}
		}

		private void New(int seen)
		{
			if (_order > seen)
			{
				this.onNew?.Invoke();
			}
		}
	}
}
