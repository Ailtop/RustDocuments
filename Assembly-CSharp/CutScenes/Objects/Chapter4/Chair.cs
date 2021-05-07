using Characters.Operations;
using Level.Chapter4;
using UnityEngine;

namespace CutScenes.Objects.Chapter4
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Chair : MonoBehaviour
	{
		[SerializeField]
		private OperationInfo Operations;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _renderer;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private Scenario _scenario;

		private void Awake()
		{
		}

		public void Hide()
		{
			_animator.speed = 0.4f;
			_animator.Play("down");
		}
	}
}
