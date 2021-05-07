using Level;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerDieHeadParts : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private DroppedParts _parts;

		public static PlayerDieHeadParts instance { get; private set; }

		public DroppedParts parts => _parts;

		public Sprite sprite
		{
			get
			{
				return _spriteRenderer.sprite;
			}
			set
			{
				_spriteRenderer.sprite = value;
			}
		}

		private void Awake()
		{
			instance = this;
		}
	}
}
