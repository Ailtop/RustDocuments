using UnityEngine;

public class rotateCamera : MonoBehaviour
{
	public float turnSpeed = 50f;

	public int count;

	public int maxCount;

	public bool left;

	private void Update()
	{
		if (left)
		{
			if (count >= maxCount)
			{
				base.transform.Rotate(Vector3.up, (0f - turnSpeed) * Time.deltaTime);
				count = 0;
				left = false;
			}
			else
			{
				base.transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
				count++;
			}
		}
		else if (count >= maxCount)
		{
			base.transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
			count = 0;
			left = true;
		}
		else
		{
			base.transform.Rotate(Vector3.up, (0f - turnSpeed) * Time.deltaTime);
			count++;
		}
	}
}
