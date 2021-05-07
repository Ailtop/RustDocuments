using System.Collections.Generic;
using UnityEngine;

namespace Characters.Utils
{
	public class HitHistoryManager
	{
		private readonly List<Target> _targets;

		private readonly List<float> _times;

		private readonly List<int> _hits;

		public int Count => _targets.Count;

		public HitHistoryManager(int capacity)
		{
			_targets = new List<Target>(capacity);
			_times = new List<float>(capacity);
			_hits = new List<int>(capacity);
		}

		public int AddOrUpdate(Target target)
		{
			int num = _targets.IndexOf(target);
			if (num == -1)
			{
				_targets.Add(target);
				_times.Add(Time.time);
				_hits.Add(1);
				return _targets.Count - 1;
			}
			_times[num] = Time.time;
			_hits[num]++;
			return num;
		}

		public void Clear()
		{
			_targets.Clear();
			_times.Clear();
			_hits.Clear();
		}

		public void ClearHits()
		{
			for (int i = 0; i < _hits.Count; i++)
			{
				_hits[i] = 0;
			}
		}

		public bool CanAttack(Target target, int maxHit, int maxHitsPerUnit, float interval)
		{
			if (_targets.Count >= maxHit)
			{
				return false;
			}
			int num = _targets.IndexOf(target);
			if (num == -1)
			{
				return true;
			}
			if (Time.time - _times[num] > interval)
			{
				return _hits[num] < maxHitsPerUnit;
			}
			return false;
		}
	}
}
