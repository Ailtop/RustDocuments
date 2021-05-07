using UnityEngine;

public class AnimatorRandomizer : MonoBehaviour
{
	[SerializeField]
	[GetComponent]
	private Animator _animator;

	private void OnEnable()
	{
		_animator.Play(0, 0, Random.value);
	}
}
