using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public class FerryRetireHandler : BaseNexusRequestHandler<FerryRetireRequest>
{
	protected override void Handle()
	{
		NexusFerry nexusFerry = NexusFerry.Get(base.Request.entityId, base.Request.timestamp);
		if (nexusFerry != null)
		{
			nexusFerry.Retire();
		}
		SendSuccess();
	}
}
