public class GrowableGene
{
	public GrowableGenetics.GeneType Type
	{
		get;
		private set;
	}

	public GrowableGenetics.GeneType PreviousType
	{
		get;
		private set;
	}

	public void Set(GrowableGenetics.GeneType geneType, bool firstSet = false)
	{
		if (firstSet)
		{
			SetPrevious(geneType);
		}
		else
		{
			SetPrevious(Type);
		}
		Type = geneType;
	}

	public void SetPrevious(GrowableGenetics.GeneType type)
	{
		PreviousType = type;
	}

	public string GetDisplayCharacter()
	{
		return GetDisplayCharacter(Type);
	}

	public static string GetDisplayCharacter(GrowableGenetics.GeneType type)
	{
		switch (type)
		{
		case GrowableGenetics.GeneType.Empty:
			return "X";
		case GrowableGenetics.GeneType.GrowthSpeed:
			return "G";
		case GrowableGenetics.GeneType.Hardiness:
			return "H";
		case GrowableGenetics.GeneType.WaterRequirement:
			return "W";
		case GrowableGenetics.GeneType.Yield:
			return "Y";
		default:
			return "U";
		}
	}

	public string GetColourCodedDisplayCharacter()
	{
		return GetColourCodedDisplayCharacter(Type);
	}

	public static string GetColourCodedDisplayCharacter(GrowableGenetics.GeneType type)
	{
		return "<color=" + (IsPositive(type) ? "#60891B>" : "#AA4734>") + GetDisplayCharacter(type) + "</color>";
	}

	public static bool IsPositive(GrowableGenetics.GeneType type)
	{
		switch (type)
		{
		case GrowableGenetics.GeneType.Empty:
			return false;
		case GrowableGenetics.GeneType.GrowthSpeed:
			return true;
		case GrowableGenetics.GeneType.Hardiness:
			return true;
		case GrowableGenetics.GeneType.WaterRequirement:
			return false;
		case GrowableGenetics.GeneType.Yield:
			return true;
		default:
			return false;
		}
	}

	public bool IsPositive()
	{
		return IsPositive(Type);
	}
}
