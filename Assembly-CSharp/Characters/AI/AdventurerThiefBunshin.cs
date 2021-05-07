using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class AdventurerThiefBunshin : AIController
	{
		[Header("Flashcut")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _flashCut;

		[SerializeField]
		[Subcomponent(typeof(TeleportBehind))]
		private TeleportBehind _teleportBehind;

		[Header("Shuriken")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Jump))]
		private Jump _surikenJump;

		[Header("Despawn Action")]
		[SerializeField]
		private Action _despawnAction;

		protected override void OnEnable()
		{
			Run();
		}

		protected override void OnDisable()
		{
			Hide();
		}

		public void Run()
		{
			Show();
			character.animationController.ForceUpdate();
			StartCoroutine(CProcess());
		}

		private void Show()
		{
			character.gameObject.SetActive(true);
		}

		private void Hide()
		{
			character.gameObject.SetActive(false);
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			character.ForceToLookAt(base.target.transform.position.x);
			yield return Chronometer.global.WaitForSeconds(1f);
			if (MMMaths.RandomBool())
			{
				yield return CastFlashCut();
			}
			else
			{
				yield return CastSuriken();
			}
			character.animationController.ForceUpdate();
			_despawnAction.TryStart();
			while (_despawnAction.running)
			{
				yield return null;
			}
			base.gameObject.SetActive(false);
		}

		private IEnumerator CastFlashCut()
		{
			yield return _teleportBehind.CRun(this);
			if (character.transform.position.x > base.target.transform.position.x)
			{
				character.lookingDirection = Character.LookingDirection.Left;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
			}
			yield return _flashCut.CRun(this);
		}

		private IEnumerator CastSuriken()
		{
			if (character.transform.position.x > base.target.transform.position.x)
			{
				character.lookingDirection = Character.LookingDirection.Left;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
			}
			yield return _surikenJump.CRun(this);
		}
	}
}
