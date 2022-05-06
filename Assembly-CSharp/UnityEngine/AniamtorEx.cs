namespace UnityEngine;

public static class AniamtorEx
{
	public static void SetFloatFixed(this Animator animator, int id, float value, float dampTime, float deltaTime)
	{
		if (value == 0f)
		{
			float @float = animator.GetFloat(id);
			if (@float == 0f)
			{
				return;
			}
			if (@float < float.Epsilon)
			{
				animator.SetFloat(id, 0f);
				return;
			}
		}
		animator.SetFloat(id, value, dampTime, deltaTime);
	}

	public static void SetBoolChecked(this Animator animator, int id, bool value)
	{
		if (animator.GetBool(id) != value)
		{
			animator.SetBool(id, value);
		}
	}
}
