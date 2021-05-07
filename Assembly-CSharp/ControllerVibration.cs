using Data;
using InControl;
using UnityEngine;
using UserInput;

public class ControllerVibration : MonoBehaviour
{
	public readonly MaxOnlyTimedFloats vibration = new MaxOnlyTimedFloats();

	private void OnEnable()
	{
		InputManager.OnActiveDeviceChanged += OnActiveDeviceChanged;
	}

	private void OnDisable()
	{
		InputManager.OnActiveDeviceChanged -= OnActiveDeviceChanged;
	}

	private void Update()
	{
		vibration.Update();
		float intensity = vibration.value * Chronometer.global.timeScale * 10f * GameData.Settings.vibrationIntensity;
		KeyMapper.Map.ActiveDevice.Vibrate(intensity);
	}

	private void OnActiveDeviceChanged(InputDevice obj)
	{
		Stop();
	}

	public void Stop()
	{
		vibration.Clear();
		InputManager.ActiveDevice.StopVibration();
	}
}
