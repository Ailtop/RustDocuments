using ProtoBuf.Nexus;
using UnityEngine;

namespace Rust.Nexus.Handlers;

public class ClanChatBatchHandler : BaseNexusRequestHandler<ClanChatBatchRequest>
{
	protected override void Handle()
	{
		if (!(ClanManager.ServerInstance.Backend is NexusClanBackend nexusClanBackend))
		{
			Debug.LogError("Received a clan chat batch but this server isn't using the nexus clan backend!");
		}
		else
		{
			nexusClanBackend.HandleClanChatBatch(base.Request);
		}
	}
}
