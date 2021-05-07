using System;
using BT.SharedValues;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace BT
{
	[Serializable]
	public class ContextSetting
	{
		[SerializeField]
		private Transform _ownerTransform;

		[SerializeField]
		private Character _ownerCharacter;

		[SerializeField]
		private bool _playerIsTarget;

		public void ApplyTo(Context context)
		{
			context.Set(Key.OwnerTransform, new SharedValue<Transform>(_ownerTransform));
			context.Set(Key.OwnerCharacter, new SharedValue<Character>(_ownerCharacter));
			if (_playerIsTarget)
			{
				context.Set(Key.Target, new SharedValue<Character>(Singleton<Service>.Instance.levelManager.player));
			}
		}
	}
}
