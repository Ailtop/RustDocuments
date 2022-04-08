namespace Rust.AI;

public class AStarNode
{
	public AStarNode Parent;

	public float G;

	public float H;

	public BasePathNode Node;

	public float F => G + H;

	public AStarNode(float g, float h, AStarNode parent, BasePathNode node)
	{
		G = g;
		H = h;
		Parent = parent;
		Node = node;
	}

	public void Update(float g, float h, AStarNode parent, BasePathNode node)
	{
		G = g;
		H = h;
		Parent = parent;
		Node = node;
	}

	public bool Satisfies(BasePathNode node)
	{
		return Node == node;
	}

	public static bool operator <(AStarNode lhs, AStarNode rhs)
	{
		return lhs.F < rhs.F;
	}

	public static bool operator >(AStarNode lhs, AStarNode rhs)
	{
		return lhs.F > rhs.F;
	}
}
