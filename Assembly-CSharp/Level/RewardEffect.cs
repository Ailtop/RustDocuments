using System;
using FX;
using UnityEngine;

namespace Level
{
	public class RewardEffect : MonoBehaviour
	{
		[Serializable]
		private class Effect
		{
			[SerializeField]
			internal RuntimeAnimatorController animator;

			[SerializeField]
			internal SoundInfo soundInfo;
		}

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private Effect _unique;

		[SerializeField]
		private Effect _legendary;

		public void Play(Rarity rarity)
		{
		}
	}
}
