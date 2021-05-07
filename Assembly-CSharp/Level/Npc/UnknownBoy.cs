using System.Collections;
using Characters;
using Characters.Gear.Quintessences;
using FX;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class UnknownBoy : InteractiveObject
	{
		private enum QuestState
		{
			Wait,
			Accepted,
			Cleared,
			GaveReward
		}

		[SerializeField]
		private Transform _body;

		[GetComponent]
		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private EnemyWave _startWave;

		[SerializeField]
		private Transform _rewardConversationPoint;

		[SerializeField]
		private Transform _dropPosition;

		[SerializeField]
		private GameObject _lineText;

		[SerializeField]
		private RarityPossibilities _essencePossibilities;

		[SerializeField]
		private EffectInfo _outEffectInfo;

		[SerializeField]
		private EffectInfo _inEffectInfo;

		private QuestState _questState;

		private NpcConversation _npcConversation;

		private Resource.Request<Quintessence> _quintessenceInfo;

		private string displayName => Lingua.GetLocalizedString("npc/essence/unknownboy/name");

		private string[] questScripts => Lingua.GetLocalizedStringArray("npc/essence/unknownboy/quest/0");

		private string[] rewardScripts => Lingua.GetLocalizedStringArray("npc/essence/unknownboy/reward/0");

		private string[] chatScripts => Lingua.GetLocalizedStringArrays("npc/essence/unknownboy/chat").Random();

		protected override void Awake()
		{
			base.Awake();
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_npcConversation.name = displayName;
			_npcConversation.skippable = true;
			_npcConversation.portrait = null;
			_questState = QuestState.Wait;
		}

		private void Start()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			_quintessenceInfo = Singleton<Service>.Instance.gearManager.GetQuintessenceToTake(_essencePossibilities.Evaluate()).LoadAsync();
		}

		public override void InteractWith(Character character)
		{
			if (_questState == QuestState.Cleared)
			{
				StartCoroutine(CRewardConversation());
			}
			else if (_questState == QuestState.Wait)
			{
				StartCoroutine(CAcceptQuest());
			}
			else if (_questState == QuestState.GaveReward)
			{
				StartCoroutine(CChat());
			}
		}

		private IEnumerator CAcceptQuest()
		{
			LetterBox.instance.Appear();
			yield return _npcConversation.CConversation(questScripts);
			LetterBox.instance.Disappear();
			_questState = QuestState.Accepted;
			_startWave.Spawn();
			StartCoroutine(CCheckWaveAllCleared());
		}

		private void OnDisable()
		{
			_npcConversation.portrait = null;
		}

		private IEnumerator CCheckWaveAllCleared()
		{
			EnemyWave[] waves = Map.Instance.waveContainer.enemyWaves;
			Transform player = Singleton<Service>.Instance.levelManager.player.transform;
			_collider.enabled = false;
			while (true)
			{
				if (base.transform.position.x < player.position.x)
				{
					_body.localScale = new Vector2(-1f, 1f);
				}
				else
				{
					_body.localScale = new Vector2(1f, 1f);
				}
				bool flag = true;
				EnemyWave[] array = waves;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].state != Wave.State.Cleared)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				yield return null;
			}
			_questState = QuestState.Cleared;
			_inEffectInfo.Spawn(base.transform.position);
			base.transform.position = _rewardConversationPoint.transform.position;
			_outEffectInfo.Spawn(base.transform.position);
			_body.localScale = Vector3.one;
			_collider.enabled = true;
			_spriteRenderer.sortingLayerName = "Enemy";
			_lineText.SetActive(false);
		}

		private IEnumerator CChat()
		{
			LetterBox.instance.Appear();
			yield return _npcConversation.CConversation(chatScripts.Random());
			LetterBox.instance.Disappear();
		}

		private IEnumerator CRewardConversation()
		{
			LetterBox.instance.Appear();
			yield return _npcConversation.CConversation(rewardScripts.Random());
			LetterBox.instance.Disappear();
			while (!_quintessenceInfo.isDone)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.DropQuintessence(_quintessenceInfo.asset, _dropPosition.position);
			_questState = QuestState.GaveReward;
		}
	}
}
