using System;
using UnityEngine;

public class PlayerModel : ListComponent<PlayerModel>
{
	public enum MountPoses
	{
		Chair = 0,
		Driving = 1,
		Horseback = 2,
		HeliUnarmed = 3,
		HeliArmed = 4,
		HandMotorBoat = 5,
		MotorBoatPassenger = 6,
		SitGeneric = 7,
		SitRaft = 8,
		StandDrive = 9,
		SitShootingGeneric = 10,
		SitMinicopter_Pilot = 11,
		SitMinicopter_Passenger = 12,
		ArcadeLeft = 13,
		ArcadeRight = 14,
		SitSummer_Ring = 15,
		SitSummer_BoogieBoard = 16,
		SitCarPassenger = 17,
		SitSummer_Chair = 18,
		SitRaft_NoPaddle = 19,
		Sit_SecretLab = 20,
		Sit_Workcart = 21,
		Sit_Cardgame = 22,
		Sit_Crane = 23,
		Sit_Snowmobile_Shooting = 24,
		Sit_RetroSnowmobile_Shooting = 25,
		Driving_Snowmobile = 26,
		ZiplineHold = 27,
		Sit_Locomotive = 28,
		Standing = 128
	}

	public Transform[] Shoulders;

	public Transform[] AdditionalSpineBones;

	protected static int speed = Animator.StringToHash("speed");

	protected static int acceleration = Animator.StringToHash("acceleration");

	protected static int rotationYaw = Animator.StringToHash("rotationYaw");

	protected static int forward = Animator.StringToHash("forward");

	protected static int right = Animator.StringToHash("right");

	protected static int up = Animator.StringToHash("up");

	protected static int ducked = Animator.StringToHash("ducked");

	protected static int grounded = Animator.StringToHash("grounded");

	protected static int crawling = Animator.StringToHash("crawling");

	protected static int waterlevel = Animator.StringToHash("waterlevel");

	protected static int attack = Animator.StringToHash("attack");

	protected static int attack_alt = Animator.StringToHash("attack_alt");

	protected static int deploy = Animator.StringToHash("deploy");

	protected static int reload = Animator.StringToHash("reload");

	protected static int throwWeapon = Animator.StringToHash("throw");

	protected static int holster = Animator.StringToHash("holster");

	protected static int aiming = Animator.StringToHash("aiming");

	protected static int onLadder = Animator.StringToHash("onLadder");

	protected static int posing = Animator.StringToHash("posing");

	protected static int poseType = Animator.StringToHash("poseType");

	protected static int relaxGunPose = Animator.StringToHash("relaxGunPose");

	protected static int vehicle_aim_yaw = Animator.StringToHash("vehicleAimYaw");

	protected static int vehicle_aim_speed = Animator.StringToHash("vehicleAimYawSpeed");

	protected static int onPhone = Animator.StringToHash("onPhone");

	protected static int usePoseTransition = Animator.StringToHash("usePoseTransition");

	protected static int leftFootIK = Animator.StringToHash("leftFootIK");

	protected static int rightFootIK = Animator.StringToHash("rightFootIK");

	protected static int vehicleSteering = Animator.StringToHash("vehicleSteering");

	protected static int sitReaction = Animator.StringToHash("sitReaction");

	protected static int forwardReaction = Animator.StringToHash("forwardReaction");

	protected static int rightReaction = Animator.StringToHash("rightReaction");

	public BoxCollider collision;

	public GameObject censorshipCube;

	public GameObject censorshipCubeBreasts;

	public GameObject jawBone;

	public GameObject neckBone;

	public GameObject headBone;

	public EyeController eyeController;

	public EyeBlink blinkController;

	public Transform[] SpineBones;

	public Transform leftFootBone;

	public Transform rightFootBone;

	public Transform leftHandPropBone;

	public Transform rightHandPropBone;

	public Vector3 rightHandTarget;

	[Header("IK")]
	public Vector3 leftHandTargetPosition;

	public Quaternion leftHandTargetRotation;

	public Vector3 rightHandTargetPosition;

	public Quaternion rightHandTargetRotation;

	public float steeringTargetDegrees;

	public Vector3 rightFootTargetPosition;

	public Quaternion rightFootTargetRotation;

	public Vector3 leftFootTargetPosition;

	public Quaternion leftFootTargetRotation;

	public RuntimeAnimatorController CinematicAnimationController;

	public Avatar DefaultAvatar;

	public Avatar CinematicAvatar;

	public RuntimeAnimatorController DefaultHoldType;

	public RuntimeAnimatorController SleepGesture;

	public RuntimeAnimatorController CrawlToIncapacitatedGesture;

	public RuntimeAnimatorController StandToIncapacitatedGesture;

	[NonSerialized]
	public RuntimeAnimatorController CurrentGesture;

	[Header("Skin")]
	public SkinSetCollection MaleSkin;

	public SkinSetCollection FemaleSkin;

	public SubsurfaceProfile subsurfaceProfile;

	[Header("Parameters")]
	[Range(0f, 1f)]
	public float voiceVolume;

	[Range(0f, 1f)]
	public float skinColor = 1f;

	[Range(0f, 1f)]
	public float skinNumber = 1f;

	[Range(0f, 1f)]
	public float meshNumber;

	[Range(0f, 1f)]
	public float hairNumber;

	[Range(0f, 1f)]
	public int skinType;

	public MovementSounds movementSounds;

	public bool showSash;

	public int tempPoseType;

	public uint underwearSkin;

	public ulong overrideSkinSeed { get; private set; }

	public bool IsFemale => skinType == 1;

	public SkinSetCollection SkinSet
	{
		get
		{
			if (!IsFemale)
			{
				return MaleSkin;
			}
			return FemaleSkin;
		}
	}

	public Quaternion AimAngles { get; set; }

	public Quaternion LookAngles { get; set; }

	private static Vector3 GetFlat(Vector3 dir)
	{
		dir.y = 0f;
		return dir.normalized;
	}

	public static void RebuildAll()
	{
	}
}
