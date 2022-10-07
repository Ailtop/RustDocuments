public class GameModeHardcore : GameModeVanilla
{
	protected override void OnCreated()
	{
		base.OnCreated();
	}

	public override ResearchCostResult GetScrapCostForResearch(ItemDefinition item, ResearchTable.ResearchType researchType)
	{
		switch (item.Blueprint?.workbenchLevelRequired)
		{
		case 1:
		{
			ResearchCostResult result = default(ResearchCostResult);
			result.Scale = 1.2f;
			return result;
		}
		case 2:
		{
			ResearchCostResult result = default(ResearchCostResult);
			result.Scale = 1.4f;
			return result;
		}
		case 3:
		{
			ResearchCostResult result = default(ResearchCostResult);
			result.Scale = 1.6f;
			return result;
		}
		default:
			return default(ResearchCostResult);
		}
	}
}
