using UnityEngine;

namespace Tutorial
{
	public class Message : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		public void Activate()
		{
			_spriteRenderer.enabled = true;
		}

		public void Deactivate()
		{
			_spriteRenderer.enabled = false;
		}
	}
}
