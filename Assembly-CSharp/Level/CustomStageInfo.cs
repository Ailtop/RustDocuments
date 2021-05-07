using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Level
{
	[CreateAssetMenu]
	public class CustomStageInfo : IStageInfo
	{
		[SerializeField]
		private ParallaxBackground _background;

		[SerializeField]
		private Gate.Type _lastGate;

		[SerializeField]
		private SerializablePathNode.Reorderable _maps;

		private int _current;

		[TupleElementNames(new string[] { "node1", "node2" })]
		public override ValueTuple<PathNode, PathNode> nextMapTypes
		{
			[return: TupleElementNames(new string[] { "node1", "node2" })]
			get
			{
				if (_current + 1 >= _maps.values.Length)
				{
					return new ValueTuple<PathNode, PathNode>(new PathNode(null, MapReward.Type.None, _lastGate), PathNode.none);
				}
				return new ValueTuple<PathNode, PathNode>(_maps.values[_current + 1], PathNode.none);
			}
		}

		public override ParallaxBackground background => _background;

		public override void Reset()
		{
			_current = -1;
		}

		public override void Initialize()
		{
		}

		public override bool Next()
		{
			_current++;
			return _current < _maps.values.Length;
		}

		public override void UpdateReferences()
		{
		}
	}
}
