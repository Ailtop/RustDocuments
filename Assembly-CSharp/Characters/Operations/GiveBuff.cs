using System.Collections.Generic;
using Level;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations
{
	public class GiveBuff : CharacterOperation
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(AttachAbility))]
		private AttachAbility _attachAbility;

		[SerializeField]
		private EnemyWaveContainer _enemyWaveContainer;

		private List<Character> _buffTargets;

		private static readonly NonAllocOverlapper _enemyOverlapper;

		private const int _targetCount = 1;

		static GiveBuff()
		{
			_enemyOverlapper = new NonAllocOverlapper(15);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Start()
		{
			if (_enemyWaveContainer == null)
			{
				_enemyWaveContainer = GetComponentInParent<EnemyWaveContainer>();
			}
		}

		private List<Character> FindRandomEnemy(Character except)
		{
			List<Character> allEnemies = _enemyWaveContainer.GetAllEnemies();
			List<Character> list = new List<Character>();
			foreach (Character item in allEnemies)
			{
				if (item.gameObject.activeSelf && item != except)
				{
					list.Add(item);
				}
			}
			if (list.Count <= 0)
			{
				return list;
			}
			int count = Mathf.Min(list.Count, 1);
			list.PseudoShuffle();
			return list.GetRange(0, count);
		}

		public override void Run(Character owner)
		{
			_buffTargets = FindRandomEnemy(owner);
			foreach (Character buffTarget in _buffTargets)
			{
				_attachAbility.Run(buffTarget);
			}
		}

		public override void Stop()
		{
			if (!(_attachAbility == null))
			{
				base.Stop();
				_attachAbility.Stop();
			}
		}
	}
}
