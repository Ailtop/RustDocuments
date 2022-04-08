using UnityEngine;

public class HitNumber : MonoBehaviour
{
	public enum HitType
	{
		Yellow = 0,
		Green = 1,
		Blue = 2,
		Purple = 3,
		Red = 4
	}

	public HitType hitType;

	public int ColorToMultiplier(HitType type)
	{
		return type switch
		{
			HitType.Yellow => 1, 
			HitType.Green => 3, 
			HitType.Blue => 5, 
			HitType.Purple => 10, 
			HitType.Red => 20, 
			_ => 0, 
		};
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(base.transform.position, 0.025f);
	}
}
