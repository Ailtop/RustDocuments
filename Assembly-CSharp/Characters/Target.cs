using System.Runtime.CompilerServices;
using Level;
using UnityEngine;

namespace Characters
{
	public class Target : MonoBehaviour, ITarget
	{
		[SerializeField]
		[GetComponent]
		private Collider2D _collider;

		[SerializeField]
		private Character _character;

		[SerializeField]
		private DestructibleObject _damageable;

		public Collider2D collider => _collider;

		public Character character
		{
			get
			{
				return _character;
			}
			set
			{
				_character = value;
			}
		}

		public DestructibleObject damageable
		{
			get
			{
				return _damageable;
			}
			set
			{
				_damageable = value;
			}
		}

		[SpecialName]
		Transform ITarget.get_transform()
		{
			return base.transform;
		}
	}
}
