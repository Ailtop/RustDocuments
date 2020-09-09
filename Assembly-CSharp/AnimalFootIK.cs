using UnityEngine;

public class AnimalFootIK : MonoBehaviour
{
	public Transform[] Feet;

	public Animator animator;

	public float maxWeightDistance = 0.1f;

	public float minWeightDistance = 0.025f;

	public float actualFootOffset = 0.01f;

	public bool GroundSample(Vector3 origin, out RaycastHit hit)
	{
		if (Physics.Raycast(origin + Vector3.up * 0.5f, Vector3.down, out hit, 1f, 455155969))
		{
			return true;
		}
		return false;
	}

	public void Start()
	{
	}

	public AvatarIKGoal GoalFromIndex(int index)
	{
		switch (index)
		{
		case 0:
			return AvatarIKGoal.LeftHand;
		case 1:
			return AvatarIKGoal.RightHand;
		case 2:
			return AvatarIKGoal.LeftFoot;
		case 3:
			return AvatarIKGoal.RightFoot;
		default:
			return AvatarIKGoal.LeftHand;
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		Debug.Log("animal ik!");
		for (int i = 0; i < 4; i++)
		{
			Transform transform = Feet[i];
			AvatarIKGoal goal = GoalFromIndex(i);
			Vector3 up = Vector3.up;
			Vector3 position = transform.transform.position;
			float iKPositionWeight = animator.GetIKPositionWeight(goal);
			RaycastHit hit;
			if (GroundSample(transform.transform.position - Vector3.down * actualFootOffset, out hit))
			{
				Vector3 normal = hit.normal;
				position = hit.point;
				float value = Vector3.Distance(transform.transform.position - Vector3.down * actualFootOffset, position);
				iKPositionWeight = 1f - Mathf.InverseLerp(minWeightDistance, maxWeightDistance, value);
				animator.SetIKPosition(goal, position + Vector3.up * actualFootOffset);
			}
			else
			{
				iKPositionWeight = 0f;
			}
			animator.SetIKPositionWeight(goal, iKPositionWeight);
		}
	}
}
