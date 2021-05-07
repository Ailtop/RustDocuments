using System;

namespace Level
{
	[Serializable]
	public class SerializablePathNode : PathNode
	{
		[Serializable]
		internal class Reorderable : ReorderableArray<SerializablePathNode>
		{
		}
	}
}
