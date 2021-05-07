using System.Collections;
using Characters;
using Characters.Abilities.CharacterStat;
using FX;
using Services;
using Singletons;
using UI;
using UnityEditor;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class FogWolf : FieldNpc
	{
		[Header("Effects")]
		[Tooltip("버프 부여 후 이 시간 동안 대화창이 잠시 사라집니다.")]
		[SerializeField]
		private float _effectShowingDuration;

		[SerializeField]
		private EffectInfo _givingBuffEffect;

		[SerializeField]
		private SoundInfo _givingBuffSound;

		[SerializeField]
		private EffectInfo _takingBuffEffect;

		[SerializeField]
		private SoundInfo _takingBuffSound;

		[Header("Buffs")]
		[SerializeField]
		[Subcomponent(typeof(StatBonusComponent))]
		private StatBonusComponent _buff1;

		[SerializeField]
		[Subcomponent(typeof(StatBonusComponent))]
		private StatBonusComponent _buff2;

		[SerializeField]
		[Subcomponent(typeof(StatBonusComponent))]
		private StatBonusComponent _buff3;

		[SerializeField]
		[Subcomponent(typeof(StatBonusComponent))]
		private StatBonusComponent _buff4;

		[SerializeField]
		[Subcomponent(typeof(StatBonusComponent))]
		private StatBonusComponent _buff5;

		[SerializeField]
		private string[] _floatingKeyArray = new string[5];

		private StatBonusComponent[] buffs;

		protected override NpcType _type => NpcType.FogWolf;

		protected override void Awake()
		{
			base.Awake();
			buffs = new StatBonusComponent[5] { _buff1, _buff2, _buff3, _buff4, _buff5 };
		}

		protected override void Interact(Character character)
		{
			base.Interact(character);
			switch (_phase)
			{
			case Phase.Initial:
			case Phase.Greeted:
				StartCoroutine(CGiveBuff(character));
				break;
			case Phase.Gave:
				StartCoroutine(CChat());
				break;
			}
		}

		private IEnumerator CGiveBuff(Character character)
		{
			yield return LetterBox.instance.CAppear();
			yield return CGreeting();
			int buffIndex = buffs.RandomIndex();
			character.ability.Add(buffs[buffIndex].ability);
			Vector2 vector = new Vector2(character.collider.bounds.center.x, character.collider.bounds.max.y + 0.5f);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(Lingua.GetLocalizedString(_floatingKeyArray[buffIndex]), vector);
			_phase = Phase.Gave;
			_npcConversation.skippable = true;
			_givingBuffEffect.Spawn(base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_givingBuffSound, base.transform.position);
			bool flag2 = (_npcConversation.visible = false);
			yield return flag2;
			yield return new WaitForSeconds(_effectShowingDuration);
			flag2 = (_npcConversation.visible = true);
			yield return flag2;
			_takingBuffEffect.Spawn(character.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_takingBuffSound, character.transform.position);
			yield return _npcConversation.CConversation(base._confirmed[buffIndex]);
			LetterBox.instance.Disappear();
		}
	}
}
