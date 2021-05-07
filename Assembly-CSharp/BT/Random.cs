using System;
using UnityEngine;

namespace BT
{
	public class Random : Composite
	{
		private int _index;

		protected virtual Node GetChild(int i)
		{
			if (i >= _child.components.Length || i < 0)
			{
				throw new ArgumentException($"{i} : invalid child index");
			}
			return _child.components[i].node;
		}

		protected override void OnInitialize()
		{
			_index = UnityEngine.Random.Range(0, _child.components.Length);
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			return GetChild(_index).Tick(context);
		}

		protected override void DoReset(NodeState state)
		{
			_index = UnityEngine.Random.Range(0, _child.components.Length);
			base.DoReset(state);
		}
	}
}
