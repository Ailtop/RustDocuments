using System;
using UnityEngine;

public class CardGameSounds : PrefabAttribute
{
	public enum SoundType
	{
		Chips,
		Draw,
		Play,
		Shuffle,
		Win,
		YourTurn
	}

	public SoundDefinition ChipsSfx;

	public SoundDefinition DrawSfx;

	public SoundDefinition PlaySfx;

	public SoundDefinition ShuffleSfx;

	public SoundDefinition WinSfx;

	public SoundDefinition YourTurnSfx;

	protected override Type GetIndexedType()
	{
		return typeof(CardGameSounds);
	}

	public void PlaySound(SoundType sound, GameObject forGameObject)
	{
		switch (sound)
		{
		case SoundType.Chips:
			ChipsSfx.Play(forGameObject);
			break;
		case SoundType.Draw:
			DrawSfx.Play(forGameObject);
			break;
		case SoundType.Play:
			PlaySfx.Play(forGameObject);
			break;
		case SoundType.Shuffle:
			ShuffleSfx.Play(forGameObject);
			break;
		case SoundType.Win:
			WinSfx.Play(forGameObject);
			break;
		case SoundType.YourTurn:
			YourTurnSfx.Play(forGameObject);
			break;
		default:
			throw new ArgumentOutOfRangeException("sound", sound, null);
		}
	}
}
