using System;
using UnityEngine;

public class CardGameSounds : PrefabAttribute
{
	public enum SoundType
	{
		Chips = 0,
		Draw = 1,
		Play = 2,
		Shuffle = 3,
		Win = 4,
		YourTurn = 5,
		Check = 6
	}

	public SoundDefinition ChipsSfx;

	public SoundDefinition DrawSfx;

	public SoundDefinition PlaySfx;

	public SoundDefinition ShuffleSfx;

	public SoundDefinition WinSfx;

	public SoundDefinition YourTurnSfx;

	public SoundDefinition CheckSfx;

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
		case SoundType.Check:
			CheckSfx.Play(forGameObject);
			break;
		default:
			throw new ArgumentOutOfRangeException("sound", sound, null);
		}
	}
}
