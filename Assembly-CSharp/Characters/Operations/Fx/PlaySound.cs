using System.Collections.Generic;
using FX;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public sealed class PlaySound : CharacterOperation
	{
		[SerializeField]
		private SoundInfo _audioClipInfo;

		[SerializeField]
		private Transform _position;

		[SerializeField]
		private bool _trackChildren;

		private readonly List<ReusableAudioSource> _children = new List<ReusableAudioSource>();

		public override void Run(Character owner)
		{
			_003C_003Ec__DisplayClass4_0 _003C_003Ec__DisplayClass4_ = new _003C_003Ec__DisplayClass4_0();
			_003C_003Ec__DisplayClass4_._003C_003E4__this = this;
			Vector3 position = ((_position == null) ? base.transform.position : _position.position);
			_003C_003Ec__DisplayClass4_.reusableAudioSource = PersistentSingleton<SoundManager>.Instance.PlaySound(_audioClipInfo, position);
			if (!(_003C_003Ec__DisplayClass4_.reusableAudioSource == null) && _trackChildren)
			{
				_children.Add(_003C_003Ec__DisplayClass4_.reusableAudioSource);
				_003C_003Ec__DisplayClass4_.reusableAudioSource.reusable.onDespawn += _003C_003Ec__DisplayClass4_._003CRun_003Eg__RemoveFromList_007C0;
			}
		}

		public override void Stop()
		{
			if (_trackChildren)
			{
				for (int num = _children.Count - 1; num >= 0; num--)
				{
					_children[num].reusable.Despawn();
				}
			}
		}
	}
}
