using System;
using UnityEngine;

namespace Characters
{
	[Serializable]
	public class ActionInformation
	{
		[SerializeField]
		private AnimationClip _characterClip;

		[SerializeField]
		private AnimationClip _weaponClip;

		[SerializeField]
		private bool _force;

		[SerializeField]
		private bool _blockMovement;

		[SerializeField]
		private bool _blockLook;

		public AnimationClip characterClip => _characterClip;

		public AnimationClip weaponClip => _weaponClip;

		public bool force => _force;

		public bool blockMovement => _blockMovement;

		public bool blockLook => _blockLook;
	}
}
