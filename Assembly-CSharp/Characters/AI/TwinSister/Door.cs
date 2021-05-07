using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class Door : MonoBehaviour
	{
		[SerializeField]
		private Animator _animator;

		public void Open()
		{
			_animator.Play("open");
		}

		public void Close()
		{
			_animator.Play("close");
		}
	}
}
