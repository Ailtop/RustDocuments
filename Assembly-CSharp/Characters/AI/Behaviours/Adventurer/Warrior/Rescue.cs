using System.Collections;
using Characters.AI.Adventurer;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class Rescue : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Jump))]
		private Jump _jump;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(SpawnLineText))]
		private SpawnLineText _spawnLineText;

		[SerializeField]
		private Transform _destinationPoint;

		[SerializeField]
		private Commander _commadner;

		[SerializeField]
		private float _minDistance;

		private void Awake()
		{
			if (_commadner == null)
			{
				_commadner = GetComponentInParent<Commander>();
			}
			_spawnLineText.Initialize();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character closestCharacterFromTarget = _commadner.GetClosestCharacterFromTarget(character);
			if (closestCharacterFromTarget == character)
			{
				base.result = Result.Fail;
				yield break;
			}
			_destinationPoint.transform.position = closestCharacterFromTarget.transform.position;
			character.ForceToLookAt(closestCharacterFromTarget.transform.position.x);
			yield return _jump.CRun(controller);
			_spawnLineText.Run(character);
		}

		public bool CanUse(Character character)
		{
			float num = Mathf.Abs(_commadner.GetClosestCharacterFromTarget(character).transform.position.x - character.transform.position.x);
			if (_jump.CanUse())
			{
				return num > _minDistance;
			}
			return false;
		}
	}
}
