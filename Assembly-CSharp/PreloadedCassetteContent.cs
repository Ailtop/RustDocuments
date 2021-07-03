using UnityEngine;

public class PreloadedCassetteContent : ScriptableObject
{
	public enum PreloadType
	{
		Short,
		Medium,
		Long
	}

	public SoundDefinition[] ShortTapeContent;

	public SoundDefinition[] MediumTapeContent;

	public SoundDefinition[] LongTapeContent;

	public SoundDefinition GetSoundContent(int index, PreloadType type)
	{
		switch (type)
		{
		case PreloadType.Short:
			return GetDefinition(index, ShortTapeContent);
		case PreloadType.Medium:
			return GetDefinition(index, MediumTapeContent);
		case PreloadType.Long:
			return GetDefinition(index, LongTapeContent);
		default:
			return null;
		}
	}

	private SoundDefinition GetDefinition(int index, SoundDefinition[] array)
	{
		index = Mathf.Clamp(index, 0, array.Length);
		return array[index];
	}

	public SoundDefinition GetSoundContent(uint id)
	{
		SoundDefinition[] shortTapeContent = ShortTapeContent;
		foreach (SoundDefinition soundDefinition in shortTapeContent)
		{
			if (StringPool.Get(soundDefinition.name) == id)
			{
				return soundDefinition;
			}
		}
		shortTapeContent = MediumTapeContent;
		foreach (SoundDefinition soundDefinition2 in shortTapeContent)
		{
			if (StringPool.Get(soundDefinition2.name) == id)
			{
				return soundDefinition2;
			}
		}
		shortTapeContent = LongTapeContent;
		foreach (SoundDefinition soundDefinition3 in shortTapeContent)
		{
			if (StringPool.Get(soundDefinition3.name) == id)
			{
				return soundDefinition3;
			}
		}
		return null;
	}
}
