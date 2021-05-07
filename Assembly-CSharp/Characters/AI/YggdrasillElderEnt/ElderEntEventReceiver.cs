using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class ElderEntEventReceiver : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[Header("Platform")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onPlatform_On;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onPlatform_Off;

		[Header("1 Phase")]
		[Space]
		[Header("Appearance")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAppearance_Appearance;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAppearance_Impact_Left;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAppearance_Impact_Right;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAppearance_Roar;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onAppearance_Sign;

		[Header("Fist Slam")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Recovery;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Recovery_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Slam;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onFistSlam_Outro;

		[Header("Sweeping")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSweeping_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSweeping_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSweeping_Sweeping;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSweeping_Sweeping_End;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSweeping_Outro;

		[Header("Energy Bomb")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onLaser_Intro_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onLaser_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onLaser_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onLaser_Laser;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onLaser_Laser_End;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onEnergyBomb_Fire;

		[Header("Groggy")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onGroggy_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onGroggy_Groggy;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onGroggy_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onGroggy_Recovery;

		[Header("Dead")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDead_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDead_DarkQuartz_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDead_DarkQuartz_Spark;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDead_DarkQuartz_Explosion;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDead_Normalize;

		[Header("Awakening")]
		[Header("2 Phase")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Awakening_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Awakening_Awakening;

		[Header("EnergyCorps")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_EnergyCorps_Intro_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_EnergyCorps_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_EnergyCorps_Emerge;

		[Header("Groggy")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Groggy_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Groggy_Groggy;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Groggy_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_Groggy_Recovery;

		[Header("Fist Power Slam")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Sign2;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Slam;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Impact;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Recovery_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Recovery;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_FistPowerSlam_Outro;

		[Header("Both Fist Power Slam")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_BothFistPowerSlam_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_BothFistPowerSlam_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_BothFistPowerSlam_Slam;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_BothFistPowerSlam_Impact;

		[Header("Sweeping Combo")]
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_Intro;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_Ready;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_Sign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_Sweeping;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_End;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onP2_SweepingCombo_Outro;

		private void Awake()
		{
			_onAppearance_Appearance.Initialize();
			_onAppearance_Impact_Left.Initialize();
			_onAppearance_Impact_Right.Initialize();
			_onAppearance_Roar.Initialize();
			_onAppearance_Sign.Initialize();
			_onDead_DarkQuartz_Explosion.Initialize();
			_onDead_DarkQuartz_Intro.Initialize();
			_onDead_DarkQuartz_Spark.Initialize();
			_onDead_Intro.Initialize();
			_onDead_Normalize.Initialize();
			_onEnergyBomb_Fire.Initialize();
			_onFistSlam_Impact.Initialize();
			_onFistSlam_Intro.Initialize();
			_onFistSlam_Outro.Initialize();
			_onFistSlam_Recovery.Initialize();
			_onFistSlam_Recovery_Sign.Initialize();
			_onFistSlam_Sign.Initialize();
			_onGroggy_Groggy.Initialize();
			_onGroggy_Impact.Initialize();
			_onGroggy_Intro.Initialize();
			_onGroggy_Recovery.Initialize();
			_onLaser_Impact.Initialize();
			_onLaser_Intro_Impact.Initialize();
			_onLaser_Laser.Initialize();
			_onLaser_Laser_End.Initialize();
			_onLaser_Sign.Initialize();
			_onSweeping_Intro.Initialize();
			_onSweeping_Outro.Initialize();
			_onSweeping_Sign.Initialize();
			_onSweeping_Sweeping.Initialize();
			_onSweeping_Sweeping_End.Initialize();
			_onP2_Awakening_Intro.Initialize();
			_onP2_Awakening_Awakening.Initialize();
			_onP2_EnergyCorps_Emerge.Initialize();
			_onP2_EnergyCorps_Intro_Impact.Initialize();
			_onP2_EnergyCorps_Sign.Initialize();
			_onP2_FistPowerSlam_Impact.Initialize();
			_onP2_FistPowerSlam_Intro.Initialize();
			_onP2_FistPowerSlam_Outro.Initialize();
			_onP2_FistPowerSlam_Recovery.Initialize();
			_onP2_FistPowerSlam_Recovery_Sign.Initialize();
			_onP2_FistPowerSlam_Sign.Initialize();
			_onP2_Groggy_Groggy.Initialize();
			_onP2_Groggy_Impact.Initialize();
			_onP2_Groggy_Intro.Initialize();
			_onP2_Groggy_Recovery.Initialize();
			_onP2_SweepingCombo_End.Initialize();
			_onP2_SweepingCombo_Intro.Initialize();
			_onP2_SweepingCombo_Outro.Initialize();
			_onP2_SweepingCombo_Ready.Initialize();
			_onP2_SweepingCombo_Sign.Initialize();
			_onP2_SweepingCombo_Sweeping.Initialize();
			_onP2_BothFistPowerSlam_Intro.Initialize();
			_onP2_BothFistPowerSlam_Sign.Initialize();
			_onP2_BothFistPowerSlam_Impact.Initialize();
		}

		private void RunOperation(OperationInfos operationInfos)
		{
			operationInfos.gameObject.SetActive(true);
			operationInfos.Run(_owner);
		}

		public void Platform_On()
		{
			RunOperation(_onPlatform_On);
		}

		public void Platform_Off()
		{
			RunOperation(_onPlatform_Off);
		}

		public void Appearance_Appearance()
		{
			RunOperation(_onAppearance_Appearance);
		}

		public void Appearance_Impact_Left()
		{
			RunOperation(_onAppearance_Impact_Left);
		}

		public void Appearance_Impact_Right()
		{
			RunOperation(_onAppearance_Impact_Right);
		}

		public void Appearance_Roar()
		{
			RunOperation(_onAppearance_Roar);
		}

		public void Appearance_Sign()
		{
			RunOperation(_onAppearance_Sign);
		}

		public void FistSlam_Intro()
		{
			RunOperation(_onFistSlam_Intro);
		}

		public void FistSlam_Impact()
		{
			RunOperation(_onFistSlam_Impact);
		}

		public void FistSlam_Recovery()
		{
			RunOperation(_onFistSlam_Recovery);
		}

		public void FistSlam_Recovery_Sign()
		{
			RunOperation(_onFistSlam_Recovery_Sign);
		}

		public void FistSlam_Sign()
		{
			RunOperation(_onFistSlam_Sign);
		}

		public void FistSlam_Slam()
		{
			RunOperation(_onFistSlam_Slam);
		}

		public void FistSlam_Outro()
		{
			RunOperation(_onFistSlam_Outro);
		}

		public void Sweeping_Intro()
		{
			RunOperation(_onSweeping_Intro);
		}

		public void Sweeping_Sign()
		{
			RunOperation(_onSweeping_Sign);
		}

		public void Sweeping_Sweeping()
		{
			RunOperation(_onSweeping_Sweeping);
		}

		public void Sweeping_Sweeping_End()
		{
			RunOperation(_onSweeping_Sweeping_End);
		}

		public void Sweeping_Outro()
		{
			RunOperation(_onSweeping_Outro);
		}

		public void Laser_Intro_Impact()
		{
			RunOperation(_onLaser_Intro_Impact);
		}

		public void Laser_Sign()
		{
			RunOperation(_onLaser_Sign);
		}

		public void Laser_Impact()
		{
			RunOperation(_onLaser_Impact);
		}

		public void Laser_Laser()
		{
			RunOperation(_onLaser_Laser);
		}

		public void Laser_Laser_End()
		{
			RunOperation(_onLaser_Laser_End);
		}

		public void EnergyBomb_Fire()
		{
			RunOperation(_onEnergyBomb_Fire);
		}

		public void Groggy_Intro()
		{
			RunOperation(_onGroggy_Intro);
		}

		public void Groggy_Groggy()
		{
			RunOperation(_onGroggy_Groggy);
		}

		public void Groggy_Impact()
		{
			RunOperation(_onGroggy_Impact);
		}

		public void Groggy_Recovery()
		{
			RunOperation(_onGroggy_Recovery);
		}

		public void Dead_Intro()
		{
			RunOperation(_onDead_Intro);
		}

		public void Dead_DarkQuartz_Intro()
		{
			RunOperation(_onDead_DarkQuartz_Intro);
		}

		public void Dead_DarkQuartz_Spark()
		{
			RunOperation(_onDead_DarkQuartz_Spark);
		}

		public void Dead_DarkQuartz_Explosion()
		{
			RunOperation(_onDead_DarkQuartz_Explosion);
		}

		public void Dead_Normalize()
		{
			RunOperation(_onDead_Normalize);
		}

		public void P2_Awakening_Intro()
		{
			RunOperation(_onP2_Awakening_Intro);
		}

		public void P2_Awakening_Awakening()
		{
			RunOperation(_onP2_Awakening_Awakening);
		}

		public void P2_EnergyCorps_Intro_Impact()
		{
			RunOperation(_onP2_EnergyCorps_Intro_Impact);
		}

		public void P2_EnergyCorps_Sign()
		{
			RunOperation(_onP2_EnergyCorps_Sign);
		}

		public void P2_EnergyCorps_Emerge()
		{
			RunOperation(_onP2_EnergyCorps_Emerge);
		}

		public void P2_Groggy_Intro()
		{
			RunOperation(_onP2_Groggy_Intro);
		}

		public void P2_Groggy_Groggy()
		{
			RunOperation(_onP2_Groggy_Groggy);
		}

		public void P2_Groggy_Impact()
		{
			RunOperation(_onP2_Groggy_Impact);
		}

		public void P2_Groggy_Recovery()
		{
			RunOperation(_onP2_Groggy_Recovery);
		}

		public void P2_FistPowerSlam_Intro()
		{
			RunOperation(_onP2_FistPowerSlam_Intro);
		}

		public void P2_FistPowerSlam_Sign()
		{
			RunOperation(_onP2_FistPowerSlam_Sign);
		}

		public void P2_FistPowerSlam_Sign2()
		{
			RunOperation(_onP2_FistPowerSlam_Sign2);
		}

		public void P2_FistPowerSlam_Slam()
		{
			RunOperation(_onP2_FistPowerSlam_Slam);
		}

		public void P2_FistPowerSlam_Impact()
		{
			RunOperation(_onP2_FistPowerSlam_Impact);
		}

		public void P2_FistPowerSlam_Recovery_Sign()
		{
			RunOperation(_onP2_FistPowerSlam_Recovery_Sign);
		}

		public void P2_FistPowerSlam_Recovery()
		{
			RunOperation(_onP2_FistPowerSlam_Recovery);
		}

		public void P2_FistPowerSlam_Outro()
		{
			RunOperation(_onP2_FistPowerSlam_Outro);
		}

		public void P2_BothFistPowerSlam_Intro()
		{
			RunOperation(_onP2_BothFistPowerSlam_Intro);
		}

		public void P2_BothFistPowerSlam_Sign()
		{
			RunOperation(_onP2_BothFistPowerSlam_Sign);
		}

		public void P2_BothFistPowerSlam_Slam()
		{
			RunOperation(_onP2_BothFistPowerSlam_Slam);
		}

		public void P2_BothFistPowerSlam_Impact()
		{
			RunOperation(_onP2_BothFistPowerSlam_Impact);
		}

		public void P2_SweepingCombo_Intro()
		{
			RunOperation(_onP2_SweepingCombo_Intro);
		}

		public void P2_SweepingCombo_Ready()
		{
			RunOperation(_onP2_SweepingCombo_Ready);
		}

		public void P2_SweepingCombo_Sign()
		{
			RunOperation(_onP2_SweepingCombo_Sign);
		}

		public void P2_SweepingCombo_Sweeping()
		{
			RunOperation(_onP2_SweepingCombo_Sweeping);
		}

		public void P2_SweepingCombo_Sweeping_End()
		{
			RunOperation(_onP2_SweepingCombo_End);
		}

		public void P2_SweepingCombo_Outro()
		{
			RunOperation(_onP2_SweepingCombo_Outro);
		}
	}
}
