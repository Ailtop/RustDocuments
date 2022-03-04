using UnityEngine;

public class EntityFlag_Animator : EntityFlag_Toggle
{
	public enum AnimatorMode
	{
		Bool = 0,
		Float = 1,
		Trigger = 2,
		Integer = 3
	}

	public Animator TargetAnimator;

	public string ParamName = string.Empty;

	public AnimatorMode AnimationMode;

	public float FloatOnState;

	public float FloatOffState;

	public int IntegerOnState;

	public int IntegerOffState;
}
