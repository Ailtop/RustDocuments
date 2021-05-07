using System.Collections;
using Characters.AI.Pope;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public class MoveHandler : MonoBehaviour
	{
		[SerializeField]
		private Point.Tag _destination;

		[SerializeField]
		[Range(0f, 1f)]
		private float _chance = 1f;

		[SerializeField]
		private Move _move;

		public IEnumerator CMove(AIController controller)
		{
			if (MMMaths.Chance(_chance))
			{
				_move.SetDestination(_destination);
				yield return _move.CRun(controller);
			}
		}
	}
}
