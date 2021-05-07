using System;
using Services;
using Singletons;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnEnterMap : Trigger
	{
		public override void Attach(Character character)
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += base.Invoke;
		}

		public override void Detach()
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= base.Invoke;
		}
	}
}
