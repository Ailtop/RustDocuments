using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public class RespawnAtBagHandler : BaseNexusRequestHandler<SleepingBagRespawnRequest>
{
	protected override void Handle()
	{
		BasePlayer basePlayer = BasePlayer.FindByID(base.Request.userId) ?? BasePlayer.FindSleeping(base.Request.userId);
		if (basePlayer != null)
		{
			if (basePlayer.IsConnected)
			{
				basePlayer.Kick("You're apparently respawning from a another zone - contact developers!");
			}
			basePlayer.Kill();
		}
		BasePlayer basePlayer2 = SingletonComponent<ServerMgr>.Instance.SpawnNewPlayer(null);
		basePlayer2.userID = base.Request.userId;
		basePlayer2.UserIDString = base.Request.userId.ToString();
		basePlayer2.displayName = basePlayer2.UserIDString;
		basePlayer2.SetPlayerFlag(BasePlayer.PlayerFlags.LoadingAfterTransfer, b: true);
		if (!SleepingBag.TrySpawnPlayer(basePlayer2, base.Request.sleepingBagId, out var errorMessage))
		{
			basePlayer2.Kill();
			SendError(errorMessage);
		}
		if (basePlayer2.isMounted)
		{
			basePlayer2.DisableTransferProtection();
		}
		else if (!basePlayer2.isMounted)
		{
			basePlayer2.SetPlayerFlag(BasePlayer.PlayerFlags.LoadingAfterTransfer, b: false);
		}
		basePlayer2.LoadSecondaryData(base.Request.secondaryData);
		basePlayer2.LoadClanInfo();
	}
}
