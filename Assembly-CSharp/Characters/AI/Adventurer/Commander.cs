using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Level.Adventurer;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class Commander : MonoBehaviour
	{
		[SerializeField]
		private PartyRandomizer _partyRandomizer;

		private List<Combat> _alives;

		private Combat _main;

		private IList<Combat> _subs;

		public List<Combat> alives => _alives;

		private void Awake()
		{
			List<Character> list = _partyRandomizer.Spawn();
			_alives = new List<Combat>(list.Count);
			foreach (Character item in list)
			{
				AdventurerController componentInChildren = item.GetComponentInChildren<AdventurerController>();
				Combat combat = new Combat(this, componentInChildren);
				alives.Add(combat);
				item.health.onDied += delegate
				{
					alives.Remove(combat);
				};
			}
		}

		public void StartIntro()
		{
			foreach (Combat alife in alives)
			{
				alife.SetTargetToPlayer();
			}
			StartCoroutine(CRun());
		}

		public AdventurerController GetRandomOne(AIController except)
		{
			if (alives == null)
			{
				return null;
			}
			if (alives.Count <= 0)
			{
				return null;
			}
			IEnumerable<Combat> enumerable = alives.Where((Combat alive) => alive.who != except);
			if (enumerable.Count() <= 0)
			{
				return null;
			}
			return enumerable.Random().who;
		}

		public Character GetLowestHealthCharacter(Character except)
		{
			if (alives.Count <= 0)
			{
				return null;
			}
			IEnumerable<Combat> source = alives.Where((Combat alive) => alive.who.character != except);
			if (source.Count() <= 0)
			{
				return null;
			}
			List<Character> list = source.Select((Combat alive) => alive.who.character).ToList();
			Character character = list[0];
			if (character == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] == null) && character.health.currentHealth > list[i].health.currentHealth)
				{
					character = list[i];
				}
			}
			return character;
		}

		public Character GetClosestCharacterFromTarget(Character except)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			int index = 0;
			float num = 2.14748365E+09f;
			for (int i = 0; i < alives.Count; i++)
			{
				if (!(except == alives[i].who.character))
				{
					Character character = alives[i].who.character;
					float num2 = Mathf.Abs(player.transform.position.x - character.transform.position.x);
					if (num2 < num)
					{
						num = num2;
						index = i;
					}
				}
			}
			return alives[index].who.character;
		}

		private IEnumerator CRun()
		{
			for (int i = 0; i < alives.Count - 1; i++)
			{
				StartCoroutine(alives[i].who.CRunIntro());
			}
			yield return alives[alives.Count - 1].who.CRunIntro();
			_main = alives.Random();
			_subs = alives.Where((Combat alive) => alive != _main).ToList();
			StartCoroutine(_main.CProcess(Strategist.instance.GetMainStrategy(alives.Count)));
			List<Strategy> subStrategys = Strategist.instance.GetSubStrategys(alives.Count);
			for (int j = 0; j < _subs.Count; j++)
			{
				StartCoroutine(_subs[j].CProcess(subStrategys[j]));
			}
		}

		private bool SatisfyRoleReplacementCondition()
		{
			foreach (Combat alife in alives)
			{
				if (!alife.terminated)
				{
					return false;
				}
			}
			return true;
		}

		private void TryExpireAll()
		{
			if (!_main.who.dead)
			{
				return;
			}
			foreach (Combat alife in alives)
			{
				alife.expired = true;
			}
		}
	}
}
