using ProtoBuf;

namespace CompanionServer.Handlers
{
	public class SetSubscription : BaseEntityHandler<AppFlag>
	{
		public override void Execute()
		{
			ISubscribable subscribable;
			if ((subscribable = base.Entity as ISubscribable) != null)
			{
				if (base.Proto.value)
				{
					if (subscribable.AddSubscription(base.UserId))
					{
						SendSuccess();
					}
					else
					{
						SendError("too_many_subscribers");
					}
				}
				else
				{
					subscribable.RemoveSubscription(base.UserId);
				}
				SendSuccess();
			}
			else
			{
				SendError("wrong_type");
			}
		}
	}
}
