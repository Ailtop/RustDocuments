using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class SpiritBottle : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(true, typeof(SimpleAction))]
		private SimpleAction _onDied;

		private void Start()
		{
			_character.health.onDied += OnDied;
		}

		private void OnDied()
		{
			_character.collider.enabled = false;
			_character.gameObject.SetActive(true);
			_spriteRenderer.enabled = true;
			_onDied?.TryStart();
			_character.health.onDied -= OnDied;
			StartCoroutine(_003COnDied_003Eg__CDestroy_007C4_0());
		}
	}
}
