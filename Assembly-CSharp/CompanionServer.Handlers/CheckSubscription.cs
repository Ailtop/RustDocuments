using ProtoBuf;

namespace CompanionServer.Handlers
{
	public class CheckSubscription : BaseEntityHandler<AppEmpty>
	{
		public override void Execute()
		{
			ISubscribable subscribable;
			if ((subscribable = (base.Entity as ISubscribable)) != null)
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
}
