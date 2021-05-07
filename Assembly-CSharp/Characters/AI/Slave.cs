using UnityEngine;

namespace Characters.AI
{
	public class Slave : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		public Master master { get; private set; }

		public void Initialize(Master master)
		{
			this.master = master;
			_character.health.onDied += delegate
			{
				master.RemoveSlave(this);
			};
		}
	}
}
