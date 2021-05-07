using Characters.Actions;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Weapons
{
	public class Skul : MonoBehaviour
	{
		[SerializeField]
		private Action _spawn;

		[SerializeField]
		private Action _downButNotOut;

		[SerializeField]
		private Action _getSkul;

		[SerializeField]
		private Action _getScroll;

		public Action spawn => _spawn;

		public Action downButNotOut => _downButNotOut;

		public Action getSkul => _getSkul;

		public Action getScroll => _getScroll;

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.type == Chapter.Type.Castle)
			{
				_spawn.TryStart();
			}
		}
	}
}
