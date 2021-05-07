using UnityEngine;

namespace Characters
{
	[ExecuteAlways]
	public class LookLeft : MonoBehaviour
	{
		private Character _character;

		private void Awake()
		{
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				base.transform.localScale = Vector3.one;
				_character = GetComponent<Character>();
				if (_character == null)
				{
					base.transform.localScale = new Vector3(-1f, 1f, 0f);
				}
				else
				{
					_character.lookingDirection = Character.LookingDirection.Left;
				}
				Object.Destroy(this);
			}
		}
	}
}
