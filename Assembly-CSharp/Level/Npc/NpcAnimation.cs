using UnityEngine;

namespace Level.Npc
{
	public class NpcAnimation : MonoBehaviour
	{
		public enum Animation
		{
			Idle,
			Cage,
			Castle
		}

		private static readonly int _idleHash = Animator.StringToHash("Idle");

		private static readonly int _cageHash = Animator.StringToHash("Idle_Cage");

		private static readonly int _castleHash = Animator.StringToHash("Idle_Castle");

		private static readonly EnumArray<Animation, int> _hashes = new EnumArray<Animation, int>(_idleHash, _cageHash, _castleHash);

		[SerializeField]
		private Animation _animation;

		private Animator _animator;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			Play(_animation);
		}

		public void Play(Animation animation)
		{
			_animator.Play(_hashes[animation]);
		}
	}
}
