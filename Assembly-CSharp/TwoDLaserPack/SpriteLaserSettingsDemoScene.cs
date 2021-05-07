using System;
using UnityEngine;
using UnityEngine.UI;

namespace TwoDLaserPack
{
	public class SpriteLaserSettingsDemoScene : MonoBehaviour
	{
		public SpriteBasedLaser SpriteBasedLaser;

		public Toggle toggleisActive;

		public Toggle toggleignoreCollisions;

		public Toggle togglelaserRotationEnabled;

		public Toggle togglelerpLaserRotation;

		public Toggle toggleuseArc;

		public Toggle toggleOscillateLaser;

		public Slider sliderlaserArcMaxYDown;

		public Slider sliderlaserArcMaxYUp;

		public Slider slidermaxLaserRaycastDistance;

		public Slider sliderturningRate;

		public Slider sliderOscillationThreshold;

		public Slider sliderOscillationSpeed;

		public Button buttonSwitch;

		public Text textValue;

		public Material[] LaserMaterials;

		public GameObject laserStartPieceBlue;

		public GameObject laserStartPieceRed;

		public GameObject laserMidPieceBlue;

		public GameObject laserMidPieceRed;

		public GameObject laserEndPieceBlue;

		public GameObject laserEndPieceRed;

		private int selectedMaterialIndex;

		private int maxSelectedIndex;

		private void Start()
		{
			if (SpriteBasedLaser == null)
			{
				Debug.LogError("You need to reference a valid LineBasedLaser on this script.");
			}
			toggleisActive.onValueChanged.AddListener(OnLaserActiveChanged);
			toggleignoreCollisions.onValueChanged.AddListener(OnLaserToggleCollisionsChanged);
			togglelaserRotationEnabled.onValueChanged.AddListener(OnLaserAllowRotationChanged);
			togglelerpLaserRotation.onValueChanged.AddListener(OnLaserLerpRotationChanged);
			toggleuseArc.onValueChanged.AddListener(OnUseArcValueChanged);
			toggleOscillateLaser.onValueChanged.AddListener(OnOscillateLaserChanged);
			sliderlaserArcMaxYDown.onValueChanged.AddListener(OnArcMaxYDownValueChanged);
			sliderlaserArcMaxYUp.onValueChanged.AddListener(OnArcMaxYUpValueChanged);
			slidermaxLaserRaycastDistance.onValueChanged.AddListener(OnLaserRaycastDistanceChanged);
			sliderturningRate.onValueChanged.AddListener(OnLaserTurningRateChanged);
			sliderOscillationThreshold.onValueChanged.AddListener(OnOscillationThresholdChanged);
			sliderOscillationSpeed.onValueChanged.AddListener(OnOscillationSpeedChanged);
			buttonSwitch.onClick.AddListener(OnButtonClick);
			selectedMaterialIndex = 1;
			maxSelectedIndex = LaserMaterials.Length - 1;
		}

		private void OnOscillationSpeedChanged(float oscillationSpeed)
		{
			SpriteBasedLaser.oscillationSpeed = oscillationSpeed;
		}

		private void OnOscillationThresholdChanged(float oscillationThreshold)
		{
			SpriteBasedLaser.oscillationThreshold = oscillationThreshold;
		}

		private void OnOscillateLaserChanged(bool oscillateLaser)
		{
			SpriteBasedLaser.oscillateLaser = oscillateLaser;
		}

		private void OnButtonClick()
		{
			if (selectedMaterialIndex < maxSelectedIndex)
			{
				selectedMaterialIndex++;
				SpriteBasedLaser.laserLineRendererArc.material = LaserMaterials[selectedMaterialIndex];
				SpriteBasedLaser.hitSparkParticleSystem.GetComponent<Renderer>().material = LaserMaterials[selectedMaterialIndex];
				SpriteBasedLaser.laserStartPiece = laserStartPieceRed;
				SpriteBasedLaser.laserMiddlePiece = laserMidPieceRed;
				SpriteBasedLaser.laserEndPiece = laserEndPieceRed;
			}
			else
			{
				selectedMaterialIndex = 0;
				SpriteBasedLaser.laserLineRendererArc.material = LaserMaterials[selectedMaterialIndex];
				SpriteBasedLaser.hitSparkParticleSystem.GetComponent<Renderer>().material = LaserMaterials[selectedMaterialIndex];
				SpriteBasedLaser.laserStartPiece = laserStartPieceBlue;
				SpriteBasedLaser.laserMiddlePiece = laserMidPieceBlue;
				SpriteBasedLaser.laserEndPiece = laserEndPieceBlue;
			}
			SpriteBasedLaser.DisableLaserGameObjectComponents();
		}

		private void OnLaserTurningRateChanged(float turningRate)
		{
			SpriteBasedLaser.turningRate = turningRate;
			textValue.color = Color.white;
			textValue.text = "Laser turning rate: " + Math.Round(turningRate, 2);
		}

		private void OnLaserRaycastDistanceChanged(float raycastDistance)
		{
			SpriteBasedLaser.maxLaserRaycastDistance = raycastDistance;
			textValue.color = Color.white;
			textValue.text = "Laser raycast max distance: " + Math.Round(raycastDistance, 2);
		}

		private void OnArcMaxYUpValueChanged(float maxYValueUp)
		{
			SpriteBasedLaser.laserArcMaxYUp = maxYValueUp;
			textValue.color = Color.white;
			textValue.text = "Laser arc maximum up arc height: " + Math.Round(maxYValueUp, 2);
		}

		private void OnArcMaxYDownValueChanged(float maxYValueDown)
		{
			SpriteBasedLaser.laserArcMaxYDown = maxYValueDown;
			textValue.color = Color.white;
			textValue.text = "Laser arc maximum down arc height: " + Math.Round(maxYValueDown, 2);
		}

		private void OnUseArcValueChanged(bool useArc)
		{
			SpriteBasedLaser.useArc = useArc;
			sliderlaserArcMaxYDown.interactable = useArc;
			sliderlaserArcMaxYUp.interactable = useArc;
			textValue.color = Color.white;
			textValue.text = "Laser arc enabled: " + useArc;
		}

		private void OnLaserLerpRotationChanged(bool lerpLaserRotation)
		{
			SpriteBasedLaser.lerpLaserRotation = lerpLaserRotation;
			sliderturningRate.interactable = lerpLaserRotation;
			textValue.color = Color.white;
			textValue.text = "Lerp laser rotation: " + lerpLaserRotation;
		}

		private void OnLaserAllowRotationChanged(bool allowRotation)
		{
			SpriteBasedLaser.laserRotationEnabled = allowRotation;
			togglelerpLaserRotation.interactable = allowRotation;
			sliderturningRate.interactable = allowRotation;
			textValue.color = Color.white;
			textValue.text = "Laser rotation enabled: " + allowRotation;
		}

		private void OnLaserToggleCollisionsChanged(bool ignoreCollisions)
		{
			SpriteBasedLaser.ignoreCollisions = ignoreCollisions;
			textValue.color = Color.white;
			textValue.text = "Ignore laser collisions: " + ignoreCollisions;
		}

		private void OnLaserActiveChanged(bool state)
		{
			SpriteBasedLaser.SetLaserState(state);
			textValue.color = Color.white;
			textValue.text = "Laser active: " + state;
		}

		private void Update()
		{
		}
	}
}
