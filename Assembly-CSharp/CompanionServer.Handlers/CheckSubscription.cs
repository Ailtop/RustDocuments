using ProtoBuf;

namespace CompanionServer.Handlers;

public class CheckSubscription : BaseEntityHandler<AppEmpty>
{
	public override void Execute()
	{
		if (base.Entity is ISubscribable subscribable)
		{
			bool value = subscribable.HasSubscription(base.UserId);
			SendFlag(value);
		}
		else
		{
			SendError("wrong_type");
		}
	}
}
