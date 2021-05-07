using System;
using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Characters.Operations.Fx;
using FX;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class SleepySkeleton : FieldNpc
	{
		[SerializeField]
		private Transform _dropPosition;

		[SerializeField]
		private EffectInfo _dropEffect;

		[SerializeField]
		private SoundInfo _dropSound;

		[SerializeField]
		private Characters.Operations.Fx.Vignette _vignette;

		[SerializeField]
		private ShaderEffect _shaderEffect;

		private Resource.Request<Weapon> _weaponToDrop;

		protected override NpcType _type => NpcType.SleepySkeleton;

		private int _healthPercentToTake => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.sleepySekeletonHealthPercentCost;

		private RarityPossibilities _headPossibilities => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.sleepySekeletonHeadPossibilities;

		private void Start()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			_weaponToDrop = Singleton<Service>.Instance.gearManager.GetWeaponToTake(_headPossibilities.Evaluate()).LoadAsync();
		}

		protected override void Interact(Character character)
		{
			base.Interact(character);
			switch (_phase)
			{
			case Phase.Initial:
			case Phase.Greeted:
				StartCoroutine(CGreetingAndConfirm(character));
				break;
			case Phase.Gave:
				StartCoroutine(CChat());
				break;
			}
		}

		private IEnumerator CGreetingAndConfirm(Character character, object confirmArg = null)
		{
			yield return LetterBox.instance.CAppear();
			string[] scripts = ((_phase == Phase.Initial) ? base._greeting : base._regreeting);
			_phase = Phase.Greeted;
			_npcConversation.skippable = true;
			int lastIndex = scripts.Length - 1;
			for (int i = 0; i < lastIndex; i++)
			{
				yield return _npcConversation.CConversation(scripts[i]);
			}
			_npcConversation.skippable = true;
			_npcConversation.body = ((confirmArg == null) ? scripts[lastIndex] : string.Format(scripts[lastIndex], confirmArg));
			yield return _npcConversation.CType();
			yield return new WaitForSecondsRealtime(0.3f);
			_npcConversation.OpenConfirmSelector(delegate
			{
				OnConfirmed(character);
			}, base.Close);
		}

		private void OnConfirmed(Character character)
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass15_.character = character;
			StartCoroutine(_003C_003Ec__DisplayClass15_._003COnConfirmed_003Eg__CDropHead_007C0());
		}

		private IEnumerator CDropWeapon()
		{
			while (!_weaponToDrop.isDone)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.DropWeapon(_weaponToDrop.asset, _dropPosition.position);
			_dropEffect.Spawn(_dropPosition.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dropSound, base.transform.position);
		}

		private void GiveDamage(Character character)
		{
			double num = character.health.maximumHealth * (double)_healthPercentToTake * 0.01;
			if (Math.Floor(character.health.currentHealth) <= num)
			{
				num = character.health.currentHealth - 1.0;
			}
			_vignette.Run(character);
			_shaderEffect.Run(character);
			character.health.TakeHealth(num);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(num, character.transform.position);
		}
	}
}
