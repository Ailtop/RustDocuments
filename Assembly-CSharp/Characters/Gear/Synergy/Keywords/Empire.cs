using System.Collections;
using Characters.Actions;
using Characters.Operations;
using Level;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Empire : Keyword
	{
		[Space]
		[SerializeField]
		[Header("디스크립션을 위한 필드")]
		private float[] _bonusStat = new float[4] { 10f, 20f, 30f, 40f };

		[SerializeField]
		private CharacterOperation[] _summonOperations;

		private Map _lastSummonedMap;

		private CharacterOperation _currentLevelOpeartion;

		public override Key key => Key.Empire;

		protected override IList valuesByLevel => _bonusStat;

		protected override void Initialize()
		{
			CharacterOperation[] summonOperations = _summonOperations;
			foreach (CharacterOperation characterOperation in summonOperations)
			{
				if (characterOperation != null)
				{
					characterOperation.Initialize();
				}
			}
		}

		protected override void OnAttach()
		{
			base.character.onStartAction += OnStartAction;
		}

		protected override void OnDetach()
		{
			base.character.onStartAction -= OnStartAction;
		}

		protected override void UpdateBonus()
		{
			_currentLevelOpeartion = _summonOperations[base.level];
		}

		private void OnStartAction(Action action)
		{
			if (action.type == Action.Type.Skill && (!(_lastSummonedMap != null) || !(_lastSummonedMap == Map.Instance)))
			{
				_lastSummonedMap = Map.Instance;
				_currentLevelOpeartion.Run(base.character);
			}
		}
	}
}
