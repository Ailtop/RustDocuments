using System;

public class BoundsCheck : PrefabAttribute
{
	public enum BlockType
	{
		Tree = 0
	}

	public BlockType IsType;

	protected override Type GetIndexedType()
	{
		return typeof(BoundsCheck);
	}
}
