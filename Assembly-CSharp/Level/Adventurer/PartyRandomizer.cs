using System.Collections.Generic;
using System.Linq;
using Characters;
using CutScenes;
using Data;
using UnityEngine;

namespace Level.Adventurer
{
	public class PartyRandomizer : MonoBehaviour
	{
		[SerializeField]
		private Character[] _characters;

		[SerializeField]
		private Character _firstMeetingCharacter;

		[SerializeField]
		private Transform[] _spawnPoints;

		[SerializeField]
		private EnemyWave _enemyWave;

		public List<Character> Spawn()
		{
			Character[] array = Take();
			List<Character> list = new List<Character>();
			for (int i = 0; i < _spawnPoints.Length; i++)
			{
				Character item = Object.Instantiate(array[i], _spawnPoints[i].position, Quaternion.identity, _enemyWave.transform);
				list.Add(item);
			}
			_enemyWave.Initialize();
			return list;
		}

		private Character[] Take()
		{
			if (_spawnPoints.Length > _characters.Length)
			{
				Debug.LogError("Wrong Request, Spawn Point is too big");
			}
			if (!GameData.Progress.cutscene.GetData(CutScenes.Key.rookieHero) && _firstMeetingCharacter != null)
			{
				return new Character[1] { _firstMeetingCharacter };
			}
			_characters.Shuffle();
			return _characters.Take(_spawnPoints.Length).ToArray();
		}
	}
}
