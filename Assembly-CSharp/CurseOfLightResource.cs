using UnityEngine;

[CreateAssetMenu]
public class CurseOfLightResource : ScriptableObject
{
	private static CurseOfLightResource _instance;

	[SerializeField]
	private AudioClip _sfx;

	[SerializeField]
	private RuntimeAnimatorController _effect;

	[SerializeField]
	private Sprite _icon;

	public AudioClip sfx => _sfx;

	public RuntimeAnimatorController effect => _effect;

	public Sprite icon => _icon;

	public static CurseOfLightResource instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<CurseOfLightResource>("CurseOfLightResource");
			}
			return _instance;
		}
	}
}
