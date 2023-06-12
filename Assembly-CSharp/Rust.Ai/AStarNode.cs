namespace Rust.AI;

public class AStarNode
{
	public AStarNode Parent;

	public float G;

	public float H;

	public IAIPathNode Node;

	public float F => G + H;

	public AStarNode(float g, float h, AStarNode parent, IAIPathNode node)
	{
		G = g;
		H = h;
		Parent = parent;
		Node = node;
	}

	public void Update(float g, float h, AStarNode parent, IAIPathNode node)
	{
		G = g;
		H = h;
		Parent = parent;
		Node = node;
	}

	public bool Satisfies(IAIPathNode node)
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
