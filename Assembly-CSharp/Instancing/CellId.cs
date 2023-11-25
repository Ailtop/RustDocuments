namespace Instancing;

public struct CellId
{
	public int Index;

	public CellId(int index)
	{
		Index = index;
	}

	public override string ToString()
	{
		return Index.ToString();
	}
}
