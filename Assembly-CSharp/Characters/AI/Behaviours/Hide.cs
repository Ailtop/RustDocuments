using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Hide : Behaviour
	{
		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Collider2D _collider2D;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _duration;

		public override IEnumerator CRun(AIController controller)
		{
			float seconds = Random.Range(_duration.x, _duration.y);
			_spriteRenderer.enabled = false;
			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
			controller.character.attach.SetActive(false);
			yield return controller.character.chronometer.master.WaitForSeconds(seconds);
			_spriteRenderer.enabled = true;
			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
			controller.character.attach.SetActive(true);
		}
	}
}
