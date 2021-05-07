using UnityEngine;

namespace Characters
{
	public class CharacterHealthBarAttacher : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private CharacterHealthBar _healthBar;

		[SerializeField]
		private Transform _parent;

		[SerializeField]
		private Vector2 _offset;

		private void Start()
		{
			if (_character != null)
			{
				_healthBar = Object.Instantiate(_healthBar, _parent ?? base.transform);
				_healthBar.transform.position = base.transform.position + MMMaths.Vector2ToVector3(_offset);
				_healthBar.Initialize(_character);
				_healthBar.SetWidth(_character.collider.size.x * 32f);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				Gizmos.DrawIcon(base.transform.position + MMMaths.Vector2ToVector3(_offset), "healthbar");
			}
		}
	}
}
