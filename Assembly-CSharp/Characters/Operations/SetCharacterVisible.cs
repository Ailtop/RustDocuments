using UnityEngine;

namespace Characters.Operations
{
	public class SetCharacterVisible : CharacterOperation
	{
		[SerializeField]
		private bool _visible;

		[SerializeField]
		private Renderer _render;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private GameObject[] _extras;

		public override void Run(Character owner)
		{
			if (_collider != null)
			{
				_collider.enabled = _visible;
			}
			if (_render != null)
			{
				_render.enabled = _visible;
			}
			if (_extras != null)
			{
				GameObject[] extras = _extras;
				for (int i = 0; i < extras.Length; i++)
				{
					extras[i].SetActive(_visible);
				}
			}
		}
	}
}
