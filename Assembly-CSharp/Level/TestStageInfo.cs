using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Level
{
	public class TestStageInfo : IStageInfo
	{
		[SerializeField]
		private ParallaxBackground _background;

		private PathNode _path;

		[TupleElementNames(new string[] { "node1", "node2" })]
		public override ValueTuple<PathNode, PathNode> nextMapTypes
		{
			[return: TupleElementNames(new string[] { "node1", "node2" })]
			get
			{
				return new ValueTuple<PathNode, PathNode>(_path, PathNode.none);
			}
		}

		public override ParallaxBackground background => _background;

		public override void Reset()
		{
		}

		public override void Initialize()
		{
			_path = new PathNode(Resource.MapReference.FromPath("Level/Test/mapToTest"), MapReward.Type.Head, Gate.Type.Grave);
		}

		public override bool Next()
		{
			return true;
		}

		public override void UpdateReferences()
		{
		}
	}
}
