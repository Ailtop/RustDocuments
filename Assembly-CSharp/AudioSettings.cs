using ConVar;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
	public AudioMixer mixer;

	private void Update()
	{
		if (!(mixer == null))
		{
			mixer.SetFloat("MasterVol", LinearToDecibel(Audio.master));
			float value;
			mixer.GetFloat("MusicVol", out value);
			if (!LevelManager.isLoaded || !MainCamera.isValid)
			{
				mixer.SetFloat("MusicVol", Mathf.Lerp(value, LinearToDecibel(Audio.musicvolumemenu), UnityEngine.Time.deltaTime));
			}
			else
			{
				mixer.SetFloat("MusicVol", Mathf.Lerp(value, LinearToDecibel(Audio.musicvolume), UnityEngine.Time.deltaTime));
			}
			mixer.SetFloat("WorldVol", LinearToDecibel(Audio.game));
			mixer.SetFloat("VoiceVol", LinearToDecibel(Audio.voices));
			mixer.SetFloat("InstrumentVol", LinearToDecibel(Audio.instruments));
			float value2 = LinearToDecibel(Audio.voiceProps) - 28.7f;
			mixer.SetFloat("VoicePropsVol", value2);
		}
	}

	private float LinearToDecibel(float linear)
	{
		if (linear > 0f)
		{
			return 20f * Mathf.Log10(linear);
		}
		return -144f;
	}
}
