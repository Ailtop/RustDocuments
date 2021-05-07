using System.Collections;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Mercenarys
{
	public class Mercenary : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private Character _owner;

		[SerializeField]
		private float _term = 1.5f;

		[SerializeField]
		private bool _follow = true;

		public bool follow
		{
			get
			{
				return _follow;
			}
			set
			{
				_follow = value;
			}
		}

		private void Start()
		{
			_owner = Singleton<Service>.Instance.levelManager.player;
			StartCoroutine(HorizontalFollow());
		}

		private IEnumerator HorizontalFollow()
		{
			while (true)
			{
				yield return null;
				if (_follow)
				{
					float num = _owner.transform.position.x - _character.transform.position.x;
					if (!(Mathf.Abs(num) < _term))
					{
						Vector2 move = ((num > 0f) ? Vector2.right : Vector2.left);
						_character.movement.move = move;
					}
				}
			}
		}
	}
}
