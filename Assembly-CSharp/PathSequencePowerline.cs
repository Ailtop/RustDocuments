using System.Collections.Generic;

public class PathSequencePowerline : PathSequence
{
	public enum SequenceRule
	{
		PowerlinePlatform = 0,
		Powerline = 1
	}

	public SequenceRule Rule;

	private const int RegularPowerlineSpacing = 2;

	public override void ApplySequenceReplacement(List<Prefab> sequence, ref Prefab replacement, Prefab[] possibleReplacements)
	{
		if (sequence.Count == 0)
		{
			return;
		}
		bool flag = false;
		int indexCountToRule = GetIndexCountToRule(sequence, SequenceRule.PowerlinePlatform);
		if (Rule == SequenceRule.Powerline)
		{
			flag = indexCountToRule >= 2;
		}
		else if (Rule == SequenceRule.PowerlinePlatform)
		{
			flag = indexCountToRule < 2 && indexCountToRule != sequence.Count;
		}
		if (flag)
		{
			Prefab prefabOfType = GetPrefabOfType(possibleReplacements, (Rule == SequenceRule.PowerlinePlatform) ? SequenceRule.Powerline : SequenceRule.PowerlinePlatform);
			if (prefabOfType != null)
			{
				replacement = prefabOfType;
			}
		}
	}

	private Prefab GetPrefabOfType(Prefab[] options, SequenceRule ruleToFind)
	{
		for (int i = 0; i < options.Length; i++)
		{
			PathSequencePowerline pathSequencePowerline = options[i].Attribute.Find<PathSequence>(options[i].ID) as PathSequencePowerline;
			if (pathSequencePowerline == null || pathSequencePowerline.Rule == ruleToFind)
			{
				return options[i];
			}
		}
		return null;
	}

	private int GetIndexCountToRule(List<Prefab> sequence, SequenceRule rule)
	{
		int num = 0;
		for (int num2 = sequence.Count - 1; num2 >= 0; num2--)
		{
			PathSequencePowerline pathSequencePowerline = sequence[num2].Attribute.Find<PathSequence>(sequence[num2].ID) as PathSequencePowerline;
			if (pathSequencePowerline != null)
			{
				if (pathSequencePowerline.Rule == rule)
				{
					break;
				}
				num++;
			}
		}
		return num;
	}
}
