using UnityEngine;

namespace FX
{
	public class GlobalSoundSettings : ScriptableObject
	{
		[SerializeField]
		private SoundInfo _gearDestroyed;

		[SerializeField]
		private SoundInfo _endFreeze;

		private static GlobalSoundSettings _instance;

		public SoundInfo gearDestroying => _gearDestroyed;

		public SoundInfo endFreeze => _endFreeze;

		public static GlobalSoundSettings instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<GlobalSoundSettings>("GlobalSoundSettings");
				}
				return _instance;
			}
		}
	}
}
