using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.AI.Behaviours.Attacks;
using Level;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class Choice : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		private Collider2D _range;

		private SacrificeCharacter _target;

		private List<SacrificeCharacter> _targets;

		private EnemyWaveContainer _container;

		private void Awake()
		{
			_container = Object.FindObjectOfType<EnemyWaveContainer>();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _attack.CRun(controller);
			base.result = Result.Success;
		}

		public void RunSacrifice()
		{
			List<Character> allEnemies = Map.Instance.waveContainer.GetAllEnemies();
			if (allEnemies == null || allEnemies.Count <= 0)
			{
				return;
			}
			_targets = allEnemies.Select((Character character) => character.GetComponent<SacrificeCharacter>()).ToList();
			foreach (SacrificeCharacter target in _targets)
			{
				if (target != null)
				{
					target.Run(true);
				}
			}
		}

		public bool CanUse()
		{
			List<Character> allEnemies = Map.Instance.waveContainer.GetAllEnemies();
			if (allEnemies == null)
			{
				return false;
			}
			if (allEnemies.Count <= 0)
			{
				return false;
			}
			return allEnemies.Any((Character character) => character.GetComponent<SacrificeCharacter>() != null);
		}
	}
}
