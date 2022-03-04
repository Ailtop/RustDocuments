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
		switch (type)
		{
		case HitType.Yellow:
			return 1;
		case HitType.Green:
			return 3;
		case HitType.Blue:
			return 5;
		case HitType.Purple:
			return 10;
		case HitType.Red:
			return 20;
		default:
			return 0;
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(base.transform.position, 0.025f);
	}
}
