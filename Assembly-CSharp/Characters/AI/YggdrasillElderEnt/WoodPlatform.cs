using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class WoodPlatform : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		[GetComponent]
		private Collider2D _collider;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		private bool _spawned;

		private void Awake()
		{
			_spriteRenderer.sprite = null;
			_collider.enabled = false;
			_animator.Play("Empty");
		}

		public void Spawn()
		{
			_animator.Play("Appearance");
			_collider.enabled = true;
		}

		public void Despawn()
		{
			_animator.Play("Disappearance");
			_collider.enabled = false;
		}
	}
}
