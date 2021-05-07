using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Level
{
	[CreateAssetMenu]
	public class SequentialStageInfo : IStageInfo
	{
		[SerializeField]
		private ParallaxBackground _background;

		private PathNode[] _path;

		private int _current;

		[TupleElementNames(new string[] { "node1", "node2" })]
		public override ValueTuple<PathNode, PathNode> nextMapTypes
		{
			[return: TupleElementNames(new string[] { "node1", "node2" })]
			get
			{
				return new ValueTuple<PathNode, PathNode>(_path[_current + 1], PathNode.none);
			}
		}

		public override ParallaxBackground background => _background;

		public override void Reset()
		{
			_current = -1;
		}

		public override void Initialize()
		{
			_path = new PathNode[maps.Length + 1];
			for (int i = 0; i < maps.Length; i++)
			{
				_path[i] = new PathNode(maps[i], MapReward.Type.None, Gate.Type.Normal);
			}
			_path[maps.Length] = PathNode.none;
		}

		public override bool Next()
		{
			_current++;
			return _current < _path.Length;
		}

		public override void UpdateReferences()
		{
		}
	}
}
