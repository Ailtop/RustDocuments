using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public class FerryUpdateScheduleHandler : BaseNexusRequestHandler<FerryUpdateScheduleRequest>
{
	protected override void Handle()
	{
		NexusFerry nexusFerry = NexusFerry.Get(base.Request.entityId, base.Request.timestamp);
		if (nexusFerry != null)
		{
			nexusFerry.UpdateSchedule(base.Request.schedule);
		}
		SendSuccess();
	}
}
