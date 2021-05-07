using System;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class ChimeraEventReceiver : MonoBehaviour
	{
		public event Action onIntro_Ready;

		public event Action onIntro_Landing;

		public event Action onIntro_FallingRocks;

		public event Action onIntro_Explosion;

		public event Action onIntro_CameraZoomOut;

		public event Action onIntro_Roar_Ready;

		public event Action onIntro_Roar;

		public event Action onIntro_LetterBoxOff;

		public event Action onIntro_HealthBarOn;

		public event Action onStomp_Ready;

		public event Action onStomp_Attack;

		public event Action onStomp_Hit;

		public event Action onStomp_End;

		public event Action onBite_Ready;

		public event Action onBite_Attack;

		public event Action onBite_Hit;

		public event Action onBite_End;

		public event Action onVenomBall_Ready;

		public event Action onVenomBall_Fire;

		public event Action onVenomCannon_Ready;

		public event Action onVenomCannon_Fire;

		public event Action onVenomBreath_Ready;

		public event Action onVenomBreath_Fire;

		public event Action onVenomBreath_End;

		public event Action onVenomFall_Roar_Ready;

		public event Action onVenomFall_Roar;

		public event Action onVenomFall_Fire;

		public event Action onVenomFall_Roar_End;

		public event Action onSubjectDrop_Roar_Ready;

		public event Action onSubjectDrop_Roar;

		public event Action onSubjectDrop_Roar_End;

		public event Action onSubjectDrop_Fire;

		public event Action onWreckDrop_Out_Ready;

		public event Action onWreckDrop_Out;

		public event Action onWreckDrop_In_Sign;

		public event Action onWreckDrop_In_Ready;

		public event Action onWreckDrop_Fire;

		public event Action oWreckDrop_In;

		public event Action onBigStomp_Ready;

		public event Action onBigStomp_Attack;

		public event Action onBigStomp_Hit;

		public event Action onBigStomp_End;

		public event Action onDead_Pause;

		public event Action onDead_Ready;

		public event Action onDead_Start;

		public event Action onDead_BreakTerrain;

		public event Action onDead_Struggle1;

		public event Action onDead_Struggle2;

		public event Action onDead_Fall;

		public event Action onDead_Water;

		public void Intro_Ready()
		{
			this.onIntro_Ready?.Invoke();
		}

		public void Intro_Landing()
		{
			this.onIntro_Landing?.Invoke();
		}

		public void Intro_FallingRocks()
		{
			this.onIntro_FallingRocks?.Invoke();
		}

		public void Intro_Explosion()
		{
			this.onIntro_Explosion?.Invoke();
		}

		public void Intro_CameraZoomOut()
		{
			this.onIntro_CameraZoomOut?.Invoke();
		}

		public void Intro_Roar_Ready()
		{
			this.onIntro_Roar_Ready?.Invoke();
		}

		public void Intro_Roar()
		{
			this.onIntro_Roar?.Invoke();
		}

		public void Intro_LetterBoxOff()
		{
			this.onIntro_LetterBoxOff?.Invoke();
		}

		public void Intro_HealthBarOn()
		{
			this.onIntro_HealthBarOn?.Invoke();
		}

		public void Stomp_Ready()
		{
			this.onStomp_Ready?.Invoke();
		}

		public void Stomp_Attack()
		{
			this.onStomp_Attack?.Invoke();
		}

		public void Stomp_Hit()
		{
			this.onStomp_Hit?.Invoke();
		}

		public void Stomp_End()
		{
			this.onStomp_End?.Invoke();
		}

		public void Bite_Ready()
		{
			this.onBite_Ready?.Invoke();
		}

		public void Bite_Attack()
		{
			this.onBite_Attack?.Invoke();
		}

		public void Bite_Hit()
		{
			this.onBite_Hit?.Invoke();
		}

		public void Bite_End()
		{
			this.onBite_End?.Invoke();
		}

		public void VenomBall_Ready()
		{
			this.onVenomBall_Ready?.Invoke();
		}

		public void VenomBall_Fire()
		{
			this.onVenomBall_Fire?.Invoke();
		}

		public void VenomCannon_Ready()
		{
			this.onVenomCannon_Ready?.Invoke();
		}

		public void VenomCannon_Fire()
		{
			this.onVenomCannon_Fire?.Invoke();
		}

		public void VenomBreath_Ready()
		{
			this.onVenomBreath_Ready?.Invoke();
		}

		public void VenomBreath_Fire()
		{
			this.onVenomBreath_Fire?.Invoke();
		}

		public void VenomBreath_End()
		{
			this.onVenomBreath_End?.Invoke();
		}

		public void VenomFall_Roar_Ready()
		{
			this.onVenomFall_Roar_Ready?.Invoke();
		}

		public void VenomFall_Roar()
		{
			this.onVenomFall_Roar?.Invoke();
		}

		public void VenomFall_Fire()
		{
			this.onVenomFall_Fire?.Invoke();
		}

		public void VenomFall_Roar_End()
		{
			this.onVenomFall_Roar_End?.Invoke();
		}

		public void SubjectDrop_Roar_Ready()
		{
			this.onSubjectDrop_Roar_Ready?.Invoke();
		}

		public void SubjectDrop_Roar()
		{
			this.onSubjectDrop_Roar?.Invoke();
		}

		public void SubjectDrop_Roar_End()
		{
			this.onSubjectDrop_Roar_End?.Invoke();
		}

		public void SubjectDrop_Fire()
		{
			this.onSubjectDrop_Fire?.Invoke();
		}

		public void WreckDrop_Out_Ready()
		{
			this.onWreckDrop_Out_Ready?.Invoke();
		}

		public void WreckDrop_Out()
		{
			this.onWreckDrop_Out?.Invoke();
		}

		public void WreckDrop_In_Sign()
		{
			this.onWreckDrop_In_Sign?.Invoke();
		}

		public void WreckDrop_In_Ready()
		{
			this.onWreckDrop_In_Ready?.Invoke();
		}

		public void WreckDrop_Fire()
		{
			this.onWreckDrop_Fire?.Invoke();
		}

		public void WreckDrop_In()
		{
			this.oWreckDrop_In?.Invoke();
		}

		public void BigStomp_Ready()
		{
			this.onBigStomp_Ready?.Invoke();
		}

		public void BigStomp_Attack()
		{
			this.onBigStomp_Attack?.Invoke();
		}

		public void BigStomp_Hit()
		{
			this.onBigStomp_Hit?.Invoke();
		}

		public void BigStomp_End()
		{
			this.onBigStomp_End?.Invoke();
		}

		public void Dead_Pause()
		{
			this.onDead_Pause?.Invoke();
		}

		public void Dead_Ready()
		{
			this.onDead_Ready?.Invoke();
		}

		public void Dead_Start()
		{
			this.onDead_Start?.Invoke();
		}

		public void Dead_BreakTerrain()
		{
			this.onDead_BreakTerrain?.Invoke();
		}

		public void Dead_Struggle1()
		{
			this.onDead_Struggle1?.Invoke();
		}

		public void Dead_Struggle2()
		{
			this.onDead_Struggle2?.Invoke();
		}

		public void Dead_Fall()
		{
			this.onDead_Fall?.Invoke();
		}

		public void Dead_Water()
		{
			this.onDead_Water?.Invoke();
		}
	}
}
