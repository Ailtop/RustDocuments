using Apex.AI;

namespace Rust.Ai
{
	public class ValidDistance : OptionScorerBase<BaseEntity>
	{
		public override float Score(IAIContext context, BaseEntity option)
		{
			EntityTargetContext entityTargetContext = context as EntityTargetContext;
			if (entityTargetContext != null)
			{
				if (!((entityTargetContext.Self.Entity.ServerPosition - option.ServerPosition).sqrMagnitude <= entityTargetContext.Self.GetStats.CloseRange * entityTargetContext.Self.GetStats.CloseRange))
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}
	}
}
