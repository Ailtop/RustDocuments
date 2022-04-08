namespace Rust.Ai;

public static class Sense
{
	private static BaseEntity[] query = new BaseEntity[512];

	public static void Stimulate(Sensation sensation)
	{
		int inSphere = BaseEntity.Query.Server.GetInSphere(sensation.Position, sensation.Radius, query, IsAbleToBeStimulated);
		float num = sensation.Radius * sensation.Radius;
		for (int i = 0; i < inSphere; i++)
		{
			if ((query[i].transform.position - sensation.Position).sqrMagnitude <= num)
			{
				query[i].OnSensation(sensation);
			}
		}
	}

	private static bool IsAbleToBeStimulated(BaseEntity ent)
	{
		if (ent is BasePlayer)
		{
			return true;
		}
		if (ent is BaseNpc)
		{
			return true;
		}
		return false;
	}
}
