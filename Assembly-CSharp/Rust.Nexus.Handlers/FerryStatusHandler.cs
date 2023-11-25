using Facepunch;
using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public class FerryStatusHandler : BaseNexusRequestHandler<FerryStatusRequest>
{
	protected override void Handle()
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		ferryStatusResponse.statuses = Pool.GetList<FerryStatus>();
		foreach (NexusFerry item in NexusFerry.All)
		{
			if (!(item == null))
			{
				FerryStatus status = item.GetStatus();
				ferryStatusResponse.statuses.Add(status);
			}
		}
		Response response = Pool.Get<Response>();
		response.ferryStatus = ferryStatusResponse;
		SendSuccess(response);
	}
}
