using Facepunch;
using System.Collections.Generic;

public class BaseResourceExtractor : BaseCombatEntity
{
	public bool canExtractLiquid;

	public bool canExtractSolid = true;

	public override void ServerInit()
	{
		base.ServerInit();
		if (!base.isClient)
		{
			List<SurveyCrater> obj = Pool.GetList<SurveyCrater>();
			Vis.Entities(base.transform.position, 3f, obj, 1);
			foreach (SurveyCrater item in obj)
			{
				if (item.isServer)
				{
					item.Kill();
				}
			}
			Pool.FreeList(ref obj);
		}
	}
}
