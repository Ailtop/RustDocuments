using UnityEngine;

public class HitNumber : MonoBehaviour
{
	public enum HitType
	{
		Yellow,
		Green,
		Blue,
		Purple,
		Red
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
