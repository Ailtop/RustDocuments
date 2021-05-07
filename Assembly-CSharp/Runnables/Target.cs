using System;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Runnables
{
	[Serializable]
	public class Target
	{
		public enum Type
		{
			Player,
			Character
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private Character _character;

		public Type type => _type;

		public Character character
		{
			get
			{
				if (Singleton<Service>.Instance.levelManager == null)
				{
					return null;
				}
				if (type == Type.Player)
				{
					return Singleton<Service>.Instance.levelManager.player;
				}
				return _character;
			}
		}
	}
}
