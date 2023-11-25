using System;
using System.Threading.Tasks;

namespace CompanionServer.Handlers;

public abstract class BaseClanHandler<T> : BasePlayerHandler<T> where T : class
{
	protected IClanBackend ClanBackend { get; private set; }

	protected async ValueTask<IClan> GetClan()
	{
		if (ClanBackend == null)
		{
			return null;
		}
		ClanValueResult<IClan> clanValueResult = ((!(base.Player != null) || base.Player.clanId == 0L) ? (await ClanBackend.GetByMember(base.UserId)) : (await ClanBackend.Get(base.Player.clanId)));
		ClanValueResult<IClan> clanValueResult2 = clanValueResult;
		if (clanValueResult2.Result != ClanResult.NoClan && clanValueResult2.Result != ClanResult.NotFound)
		{
			IClan value = clanValueResult2.Value;
			base.Client.Subscribe(new ClanTarget(value.ClanId));
			return value;
		}
		return null;
	}

	public override void EnterPool()
	{
		base.EnterPool();
		ClanBackend = null;
	}

	public override ValidationResult Validate()
	{
		ValidationResult num = base.Validate();
		if (num == ValidationResult.Success && ClanManager.ServerInstance != null)
		{
			ClanBackend = ClanManager.ServerInstance.Backend;
		}
		return num;
	}

	protected void SendError(ClanResult result)
	{
		SendError(GetErrorString(result));
	}

	private static string GetErrorString(ClanResult result)
	{
		return result switch
		{
			ClanResult.Success => throw new ArgumentException("ClanResult.Success is not an error"), 
			ClanResult.Timeout => "clan_timeout", 
			ClanResult.NoClan => "clan_no_clan", 
			ClanResult.NotFound => "clan_not_found", 
			ClanResult.NoPermission => "clan_no_permission", 
			ClanResult.InvalidText => "clan_invalid_text", 
			ClanResult.InvalidLogo => "clan_invalid_logo", 
			ClanResult.InvalidColor => "clan_invalid_color", 
			ClanResult.DuplicateName => "clan_duplicate_name", 
			ClanResult.RoleNotEmpty => "clan_role_not_empty", 
			ClanResult.CannotSwapLeader => "clan_cannot_swap_leader", 
			ClanResult.CannotDeleteLeader => "clan_cannot_delete_leader", 
			ClanResult.CannotKickLeader => "clan_cannot_kick_leader", 
			ClanResult.CannotDemoteLeader => "clan_cannot_demote_leader", 
			ClanResult.AlreadyInAClan => "clan_already_in_clan", 
			_ => "clan_fail", 
		};
	}
}
