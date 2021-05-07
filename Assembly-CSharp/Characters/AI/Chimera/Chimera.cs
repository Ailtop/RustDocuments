using System.Collections;
using Characters.AI.Behaviours.Chimera;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class Chimera : AIController
	{
		[Header("Intro")]
		[SerializeField]
		[Subcomponent(typeof(Intro))]
		private Intro _intro;

		[Header("Bite")]
		[SerializeField]
		[Subcomponent(typeof(Bite))]
		private Bite _bite;

		[Header("Stomp")]
		[SerializeField]
		[Subcomponent(typeof(Stomp))]
		private Stomp _stomp;

		[Header("VenomFall")]
		[SerializeField]
		[Subcomponent(typeof(VenomFall))]
		private VenomFall _venomFall;

		[Header("VenomBall")]
		[SerializeField]
		[Subcomponent(typeof(VenomBall))]
		private VenomBall _venomBall;

		[Header("VenomCannon")]
		[SerializeField]
		[Subcomponent(typeof(VenomCannon))]
		private VenomCannon _venomCannon;

		[Header("SubjectDrop")]
		[SerializeField]
		[Subcomponent(typeof(SubjectDrop))]
		private SubjectDrop _subjectDrop;

		[Header("WreckDrop")]
		[SerializeField]
		[Subcomponent(typeof(WreckDrop))]
		private WreckDrop _wreckDrop;

		[Header("WreckDestroy")]
		[SerializeField]
		[Subcomponent(typeof(WreckDestroy))]
		private WreckDestroy _wreckDestroy;

		[Header("VenomBreath")]
		[SerializeField]
		[Subcomponent(typeof(VenomBreath))]
		private VenomBreath _venomBreath;

		[Header("Dead")]
		[SerializeField]
		[Subcomponent(typeof(ChimeraDie))]
		private ChimeraDie _chimeraDie;

		[Header("SkippableIdle")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _idleSkipChance = 0.3f;

		[Header("Tools")]
		[SerializeField]
		private ChimeraEventReceiver _chimeraEventListener;

		[SerializeField]
		private ChimeraAnimation _chimeraAnimation;

		[SerializeField]
		private ChimeraCombat _chimeraCombat;

		[SerializeField]
		private Chapter3Script _script;

		[SerializeField]
		private Character _darkAlchemist;

		private void Awake()
		{
			base.OnEnable();
			RegisterBehaviourtoEvent();
			StartCoroutine(CProcess());
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.Open(BossHealthbarController.Type.Chpater3_Phase1, character);
		}

		private void OnDestroy()
		{
			if (!Service.quitting && !(Singleton<Service>.Instance.levelManager.player == null))
			{
				Singleton<Service>.Instance.levelManager.player.invulnerable.Detach(this);
			}
		}

		protected override IEnumerator CProcess()
		{
			character.health.onDiedTryCatch += delegate
			{
				Singleton<Service>.Instance.levelManager.player.invulnerable.Attach(this);
				_chimeraDie.KillAllEnemyInBounds(this);
				_wreckDestroy.CRun(this);
				StopAllCoroutinesWithBehaviour();
				StartCoroutine(Die());
			};
			yield return Sleep();
		}

		public IEnumerator Combat()
		{
			yield return Intro();
			while (!character.health.dead)
			{
				yield return _chimeraCombat.Combat();
			}
		}

		public bool CanUseWreckDrop()
		{
			return _wreckDrop.CanUse(character);
		}

		public bool CanUseSubjectDrop()
		{
			return _subjectDrop.canUse;
		}

		public bool CanUseStomp()
		{
			return _stomp.CanUse(this);
		}

		public bool CanUseVenomFall()
		{
			return _venomFall.CanUse();
		}

		public bool CanUseBite()
		{
			return _bite.CanUse(this);
		}

		public void SetAnimationSpeed(float speed)
		{
			_chimeraAnimation.speed = speed;
		}

		public IEnumerator RunPattern(Pattern pattern)
		{
			if (!character.health.dead)
			{
				switch (pattern)
				{
				case Pattern.Bite:
					yield return CastBite();
					break;
				case Pattern.Stomp:
					yield return CastStomp();
					break;
				case Pattern.VenomFall:
					yield return CastVenomFall();
					break;
				case Pattern.VenomBall:
					yield return CastVenomBall();
					break;
				case Pattern.VenomCannon:
					yield return CastVenomCannon();
					break;
				case Pattern.SubjectDrop:
					yield return CastSubjectDrop();
					break;
				case Pattern.WreckDrop:
					yield return CastWreckDrop();
					break;
				case Pattern.WreckDestroy:
					yield return CastWreckDestroy();
					break;
				case Pattern.VenomBreath:
					yield return CastVenomBreath();
					break;
				case Pattern.Idle:
					yield return Idle();
					break;
				case Pattern.SkippableIdle:
					yield return SkippableIdle();
					break;
				}
			}
		}

		private IEnumerator Sleep()
		{
			yield return _chimeraAnimation.PlaySleepAnimation();
		}

		private IEnumerator Idle()
		{
			yield return _chimeraAnimation.PlayIdleAnimation();
		}

		private IEnumerator SkippableIdle()
		{
			if (!MMMaths.Chance(_idleSkipChance))
			{
				yield return _chimeraAnimation.PlayIdleAnimation();
			}
		}

		private IEnumerator Intro()
		{
			character.invulnerable.Attach(this);
			yield return _chimeraAnimation.PlayIntroAnimation();
			character.invulnerable.Detach(this);
		}

		private IEnumerator CastBite()
		{
			yield return _chimeraAnimation.PlayBiteAnimation();
		}

		private IEnumerator CastStomp()
		{
			yield return _chimeraAnimation.PlayStompAnimation();
		}

		private IEnumerator CastVenomFall()
		{
			yield return _chimeraAnimation.PlayVenomFallAnimation();
		}

		private IEnumerator CastVenomBall()
		{
			_script.ShowCeil();
			yield return _chimeraAnimation.PlayVenomBallAnimation();
		}

		private IEnumerator CastVenomCannon()
		{
			yield return _chimeraAnimation.PlayVenomCannonAnimation();
		}

		private IEnumerator CastSubjectDrop()
		{
			yield return _chimeraAnimation.PlaySubjectDropAnimation();
		}

		private IEnumerator CastVenomBreath()
		{
			_script.HideCeil();
			yield return _chimeraAnimation.PlayVenomBreathAnimation();
		}

		private IEnumerator CastWreckDrop()
		{
			yield return _chimeraAnimation.PlayWreckDropAnimation();
		}

		private IEnumerator CastWreckDestroy()
		{
			yield return _chimeraAnimation.PlayWreckDestroyAnimation();
		}

		private IEnumerator Die()
		{
			SetAnimationSpeed(1f);
			yield return _chimeraAnimation.PlayDieAnimation();
			Singleton<Service>.Instance.levelManager.player.invulnerable.Detach(this);
		}

		private void RegisterBehaviourtoEvent()
		{
			_chimeraEventListener.onIntro_Ready += delegate
			{
				_intro.Ready(character);
			};
			_chimeraEventListener.onIntro_Landing += delegate
			{
				_intro.Landing(character, _script);
			};
			_chimeraEventListener.onIntro_FallingRocks += delegate
			{
				_intro.FallingRocks(character);
			};
			_chimeraEventListener.onIntro_Explosion += delegate
			{
				_intro.Explosion(character);
				if (_darkAlchemist != null)
				{
					_darkAlchemist.health.Kill();
				}
			};
			_chimeraEventListener.onIntro_CameraZoomOut += delegate
			{
				_intro.CameraZoomOut();
			};
			_chimeraEventListener.onIntro_Roar_Ready += delegate
			{
				_intro.RoarReady(character);
			};
			_chimeraEventListener.onIntro_Roar += delegate
			{
				_intro.Roar(character);
			};
			_chimeraEventListener.onIntro_Roar += delegate
			{
				_intro.LetterBoxOff();
			};
			_chimeraEventListener.onIntro_Roar += delegate
			{
				_intro.HealthBarOn(character);
			};
			_chimeraEventListener.onBite_Ready += delegate
			{
				_bite.Ready(character);
			};
			_chimeraEventListener.onBite_Attack += delegate
			{
				StartCoroutine(_bite.CRun(this));
			};
			_chimeraEventListener.onBite_End += delegate
			{
				_bite.End(character);
			};
			_chimeraEventListener.onBite_Hit += delegate
			{
				_bite.Hit(character);
			};
			_chimeraEventListener.onStomp_Ready += delegate
			{
				_stomp.Ready(character);
			};
			_chimeraEventListener.onStomp_Attack += delegate
			{
				StartCoroutine(_stomp.CRun(this));
			};
			_chimeraEventListener.onStomp_End += delegate
			{
				_stomp.End(character);
			};
			_chimeraEventListener.onStomp_Hit += delegate
			{
				_stomp.Hit(character);
			};
			_chimeraEventListener.onSubjectDrop_Roar_Ready += delegate
			{
				_subjectDrop.Ready(character);
			};
			_chimeraEventListener.onSubjectDrop_Roar += delegate
			{
				_subjectDrop.Roar(character);
			};
			_chimeraEventListener.onSubjectDrop_Fire += delegate
			{
				_script.HideCeil();
				StartCoroutine(_subjectDrop.CRun(this));
			};
			_chimeraEventListener.onSubjectDrop_Roar_End += delegate
			{
				_subjectDrop.End(character);
			};
			_chimeraEventListener.onVenomBall_Ready += delegate
			{
				_venomBall.Ready(character);
			};
			_chimeraEventListener.onVenomBall_Fire += delegate
			{
				StartCoroutine(_venomBall.CRun(this));
			};
			_chimeraEventListener.onVenomCannon_Ready += delegate
			{
				_venomCannon.Ready(character);
			};
			_chimeraEventListener.onVenomCannon_Fire += delegate
			{
				StartCoroutine(_venomCannon.CRun(this));
			};
			_chimeraEventListener.onVenomFall_Roar_Ready += delegate
			{
				_venomFall.Ready(character);
			};
			_chimeraEventListener.onVenomFall_Roar += delegate
			{
				_venomFall.Roar(character);
			};
			_chimeraEventListener.onVenomFall_Fire += delegate
			{
				StartCoroutine(_venomFall.CRun(this));
			};
			_chimeraEventListener.onWreckDrop_Out_Ready += delegate
			{
				_wreckDrop.OutReady(character);
			};
			_chimeraEventListener.onWreckDrop_Out += delegate
			{
				_wreckDrop.OutJump(character);
			};
			_chimeraEventListener.onWreckDrop_In_Sign += delegate
			{
				_wreckDrop.InSign(character);
			};
			_chimeraEventListener.onWreckDrop_In_Ready += delegate
			{
				_wreckDrop.InReady(character);
			};
			_chimeraEventListener.onWreckDrop_Fire += delegate
			{
				_script.HideCeil();
				StartCoroutine(_wreckDrop.CRun(this));
			};
			_chimeraEventListener.oWreckDrop_In += delegate
			{
				_wreckDrop.InLanding(character);
			};
			_chimeraEventListener.onVenomBreath_Ready += delegate
			{
				_venomBreath.Ready(character);
			};
			_chimeraEventListener.onVenomBreath_Fire += delegate
			{
				StartCoroutine(_venomBreath.CRun(this));
			};
			_chimeraEventListener.onVenomBreath_End += delegate
			{
				_venomBreath.End(character);
			};
			_chimeraEventListener.onBigStomp_Ready += delegate
			{
				_wreckDestroy.Ready(character);
			};
			_chimeraEventListener.onBigStomp_Attack += delegate
			{
				_wreckDestroy.Attack(character);
			};
			_chimeraEventListener.onBigStomp_End += delegate
			{
				_wreckDestroy.End(character);
			};
			_chimeraEventListener.onBigStomp_Hit += delegate
			{
				StartCoroutine(_wreckDestroy.CRun(this));
			};
			_chimeraEventListener.onDead_Pause += delegate
			{
				_chimeraDie.Pause(character);
			};
			_chimeraEventListener.onDead_Ready += delegate
			{
				_chimeraDie.Ready(character);
			};
			_chimeraEventListener.onDead_Start += delegate
			{
				_chimeraDie.Down(character);
			};
			_chimeraEventListener.onDead_BreakTerrain += delegate
			{
				_chimeraDie.BreakTerrain(character);
			};
			_chimeraEventListener.onDead_Struggle1 += delegate
			{
				_chimeraDie.Struggle1(character);
			};
			_chimeraEventListener.onDead_Struggle2 += delegate
			{
				_chimeraDie.Struggle2(character);
			};
			_chimeraEventListener.onDead_Fall += delegate
			{
				_chimeraDie.Fall(character);
			};
			_chimeraEventListener.onDead_Water += delegate
			{
				_chimeraDie.Water(character, _script);
			};
		}
	}
}
