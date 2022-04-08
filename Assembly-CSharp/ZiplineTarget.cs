using UnityEngine;

public class ZiplineTarget : MonoBehaviour
{
	public Transform Target;

	public bool IsChainPoint;

	public float MonumentConnectionDotMin = 0.2f;

	public float MonumentConnectionDotMax = 1f;

	public bool IsValidPosition(Vector3 position)
	{
		float num = Mathf.Clamp(Vector3.Dot((position - Target.position.WithY(position.y)).normalized, Target.forward), 0f, 1f);
		if (num >= MonumentConnectionDotMin)
		{
			return num <= MonumentConnectionDotMax;
		}
		return false;
	}

	public bool IsValidChainPoint(Vector3 origin, Vector3 targetPos)
	{
		float num = Mathf.Clamp(Vector3.Dot((origin - Target.position.WithY(origin.y)).normalized, Target.forward), -1f, 1f);
		float num2 = Mathf.Clamp(Vector3.Dot((targetPos - Target.position.WithY(targetPos.y)).normalized, Target.forward), -1f, 1f);
		if ((num > 0f && num2 > 0f) || (num < 0f && num2 < 0f))
		{
			return false;
		}
		num = Mathf.Abs(num);
		num2 = Mathf.Abs(num2);
		if (num >= MonumentConnectionDotMin && num <= MonumentConnectionDotMax && num2 >= MonumentConnectionDotMin)
		{
			return num2 <= MonumentConnectionDotMax;
		}
		return false;
	}
}
