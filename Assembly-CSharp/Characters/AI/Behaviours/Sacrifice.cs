using System.Collections;
using Characters.Actions;
using Level;
using Level.Traps;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class Sacrifice : Behaviour
	{
		[SerializeField]
		private Action _action;

		[SerializeField]
		private TentacleAI _tentaclePrefab;

		[SerializeField]
		private Sprite _corpseImage;

		private TentacleAI _instantiatedTentacle;

		private void Awake()
		{
			Preload();
			base.result = Result.Done;
		}

		private void Preload()
		{
			_instantiatedTentacle = Object.Instantiate(_tentaclePrefab, base.transform);
			_instantiatedTentacle.Hide();
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			controller.StopAllBehaviour();
			base.result = Result.Doing;
			while (!controller.dead)
			{
				if (controller.stuned)
				{
					yield return null;
					continue;
				}
				if (_action.TryStart())
				{
					while (_action.running && base.result.Equals(Result.Doing))
					{
						yield return null;
					}
				}
				if (!controller.stuned)
				{
					break;
				}
				yield return null;
			}
			base.result = Result.Done;
			SummonTentacle(character);
		}

		private void SummonTentacle(Character owner)
		{
			_instantiatedTentacle.Appear(owner.transform, _corpseImage, owner.lookingDirection == Character.LookingDirection.Left);
			_instantiatedTentacle.character.lookingDirection = owner.lookingDirection;
			Map.Instance.waveContainer.summonWave.Attach(_instantiatedTentacle.character);
			CharacterDieEffect component = owner.GetComponent<CharacterDieEffect>();
			if (component != null)
			{
				component.Detach();
			}
			owner.health.Kill();
			owner.gameObject.SetActive(false);
			owner.collider.enabled = false;
		}
	}
}
