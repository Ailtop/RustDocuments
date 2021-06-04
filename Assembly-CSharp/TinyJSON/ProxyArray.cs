using System.Collections;
using System.Collections.Generic;

namespace TinyJSON
{
	public sealed class ProxyArray : Variant, IEnumerable<Variant>, IEnumerable
	{
		private readonly List<Variant> list;

		public override Variant this[int index]
		{
			get
			{
				return list[index];
			}
			set
			{
				list[index] = value;
			}
		}

		public int Count => list.Count;

		public ProxyArray()
		{
			list = new List<Variant>();
		}

		IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public void Add(Variant item)
		{
			list.Add(item);
		}

		internal bool CanBeMultiRankArray(int[] rankLengths)
		{
			return CanBeMultiRankArray(0, rankLengths);
		}

		private bool CanBeMultiRankArray(int rank, int[] rankLengths)
		{
			int num = (rankLengths[rank] = list.Count);
			if (rank == rankLengths.Length - 1)
			{
				return true;
			}
			ProxyArray proxyArray = list[0] as ProxyArray;
			if (proxyArray == null)
			{
				return false;
			}
			int count = proxyArray.Count;
			for (int i = 1; i < num; i++)
			{
				ProxyArray proxyArray2 = list[i] as ProxyArray;
				if (proxyArray2 == null)
				{
					return false;
				}
				if (proxyArray2.Count != count)
				{
					return false;
				}
				if (!proxyArray2.CanBeMultiRankArray(rank + 1, rankLengths))
				{
					return false;
				}
			}
			return true;
		}
	}
}
