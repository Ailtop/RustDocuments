using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Monsters
{
	public class MonsterOnDied : MonoBehaviour
	{
		[SerializeField]
		private Monster _monster;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private void Awake()
		{
			Character owner = _monster.character;
			owner.health.onDied += delegate
			{
				StartCoroutine(_operations.CRun(owner));
				owner.health.Revive();
				_monster.Despawn();
			};
		}
	}
}
