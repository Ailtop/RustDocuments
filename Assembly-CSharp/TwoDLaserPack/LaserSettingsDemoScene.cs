using System;
using UnityEngine;
using UnityEngine.UI;

namespace TwoDLaserPack
{
	public class LaserSettingsDemoScene : MonoBehaviour
	{
		public LineBasedLaser LineBasedLaser;

		public DemoFollowScript FollowScript;

		public Toggle toggleisActive;

		public Toggle toggleignoreCollisions;

		public Toggle togglelaserRotationEnabled;

		public Toggle togglelerpLaserRotation;

		public Toggle toggleuseArc;

		public Toggle toggleTargetMouse;

		public Slider slidertexOffsetSpeed;

		public Slider sliderlaserArcMaxYDown;

		public Slider sliderlaserArcMaxYUp;

		public Slider slidermaxLaserRaycastDistance;

		public Slider sliderturningRate;

		public Button buttonSwitch;

		public Text textValue;

		public Material[] LaserMaterials;

		private int selectedMaterialIndex;

		private int maxSelectedIndex;

		private bool targetShouldTrackMouse;

		private void Start()
		{
			if (LineBasedLaser == null)
			{
				Debug.LogError("You need to reference a valid LineBasedLaser on this script.");
			}
			toggleisActive.onValueChanged.AddListener(OnLaserActiveChanged);
			toggleignoreCollisions.onValueChanged.AddListener(OnLaserToggleCollisionsChanged);
			togglelaserRotationEnabled.onValueChanged.AddListener(OnLaserAllowRotationChanged);
			togglelerpLaserRotation.onValueChanged.AddListener(OnLaserLerpRotationChanged);
			toggleuseArc.onValueChanged.AddListener(OnUseArcValueChanged);
			toggleTargetMouse.onValueChanged.AddListener(OnToggleFollowMouse);
			slidertexOffsetSpeed.onValueChanged.AddListener(OnTextureOffsetSpeedChanged);
			sliderlaserArcMaxYDown.onValueChanged.AddListener(OnArcMaxYDownValueChanged);
			sliderlaserArcMaxYUp.onValueChanged.AddListener(OnArcMaxYUpValueChanged);
			slidermaxLaserRaycastDistance.onValueChanged.AddListener(OnLaserRaycastDistanceChanged);
			sliderturningRate.onValueChanged.AddListener(OnLaserTurningRateChanged);
			buttonSwitch.onClick.AddListener(OnButtonClick);
			selectedMaterialIndex = 1;
			maxSelectedIndex = LaserMaterials.Length - 1;
		}

		private void OnToggleFollowMouse(bool followMouse)
		{
			targetShouldTrackMouse = followMouse;
			if (targetShouldTrackMouse)
			{
				FollowScript.enabled = false;
			}
			else
			{
				FollowScript.enabled = true;
			}
		}

		private void OnButtonClick()
		{
			if (selectedMaterialIndex < maxSelectedIndex)
			{
				selectedMaterialIndex++;
				LineBasedLaser.laserLineRenderer.material = LaserMaterials[selectedMaterialIndex];
				LineBasedLaser.laserLineRendererArc.material = LaserMaterials[selectedMaterialIndex];
				LineBasedLaser.hitSparkParticleSystem.GetComponent<Renderer>().material = LaserMaterials[selectedMaterialIndex];
			}
			else
			{
				selectedMaterialIndex = 0;
				LineBasedLaser.laserLineRenderer.material = LaserMaterials[selectedMaterialIndex];
				LineBasedLaser.laserLineRendererArc.material = LaserMaterials[selectedMaterialIndex];
				LineBasedLaser.hitSparkParticleSystem.GetComponent<Renderer>().material = LaserMaterials[selectedMaterialIndex];
			}
		}

		private void OnLaserTurningRateChanged(float turningRate)
		{
			LineBasedLaser.turningRate = turningRate;
			textValue.color = Color.white;
			textValue.text = "Laser turning rate: " + Math.Round(turningRate, 2);
		}

		private void OnLaserRaycastDistanceChanged(float raycastDistance)
		{
			LineBasedLaser.maxLaserRaycastDistance = raycastDistance;
			textValue.color = Color.white;
			textValue.text = "Laser raycast max distance: " + Math.Round(raycastDistance, 2);
		}

		private void OnArcMaxYUpValueChanged(float maxYValueUp)
		{
			LineBasedLaser.laserArcMaxYUp = maxYValueUp;
			textValue.color = Color.white;
			textValue.text = "Laser arc maximum up arc height: " + Math.Round(maxYValueUp, 2);
		}

		private void OnArcMaxYDownValueChanged(float maxYValueDown)
		{
			LineBasedLaser.laserArcMaxYDown = maxYValueDown;
			textValue.color = Color.white;
			textValue.text = "Laser arc maximum down arc height: " + Math.Round(maxYValueDown, 2);
		}

		private void OnTextureOffsetSpeedChanged(float offsetSpeed)
		{
			LineBasedLaser.laserTexOffsetSpeed = offsetSpeed;
			textValue.color = Color.white;
			textValue.text = "Laser texture offset speed: " + Math.Round(offsetSpeed, 2);
		}

		private void OnUseArcValueChanged(bool useArc)
		{
			LineBasedLaser.useArc = useArc;
			sliderlaserArcMaxYDown.interactable = useArc;
			sliderlaserArcMaxYUp.interactable = useArc;
			textValue.color = Color.white;
			textValue.text = "Laser arc enabled: " + useArc;
		}

		private void OnLaserLerpRotationChanged(bool lerpLaserRotation)
		{
			LineBasedLaser.lerpLaserRotation = lerpLaserRotation;
			sliderturningRate.interactable = lerpLaserRotation;
			textValue.color = Color.white;
			textValue.text = "Lerp laser rotation: " + lerpLaserRotation;
		}

		private void OnLaserAllowRotationChanged(bool allowRotation)
		{
			LineBasedLaser.laserRotationEnabled = allowRotation;
			togglelerpLaserRotation.interactable = allowRotation;
			sliderturningRate.interactable = allowRotation;
			textValue.color = Color.white;
			textValue.text = "Laser rotation enabled: " + allowRotation;
		}

		private void OnLaserToggleCollisionsChanged(bool ignoreCollisions)
		{
			LineBasedLaser.ignoreCollisions = ignoreCollisions;
			textValue.color = Color.white;
			textValue.text = "Ignore laser collisions: " + ignoreCollisions;
		}

		private void OnLaserActiveChanged(bool state)
		{
			LineBasedLaser.SetLaserState(state);
			textValue.color = Color.white;
			textValue.text = "Laser active: " + state;
		}

		private void Update()
		{
			if (targetShouldTrackMouse)
			{
				Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 vector2 = new Vector2(vector.x, vector.y);
				LineBasedLaser.targetGo.transform.position = vector2;
			}
		}
	}
}
