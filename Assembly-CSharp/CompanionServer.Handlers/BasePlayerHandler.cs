namespace CompanionServer.Handlers;

public abstract class BasePlayerHandler<T> : BaseHandler<T> where T : class
{
	protected ulong UserId { get; private set; }

	protected BasePlayer Player { get; private set; }

	public override void EnterPool()
	{
		UserId = 0uL;
		Player = null;
	}

	public override ValidationResult Validate()
	{
		ValidationResult validationResult = base.Validate();
		if (validationResult != 0)
		{
			return validationResult;
		}
		bool locked;
		int orGenerateAppToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(base.Request.playerId, out locked);
		if (base.Request.playerId == 0L || base.Request.playerToken != orGenerateAppToken)
		{
			return ValidationResult.NotFound;
		}
		if (locked)
		{
			return ValidationResult.Banned;
		}
		if ((ServerUsers.Get(base.Request.playerId)?.group ?? ServerUsers.UserGroup.None) == ServerUsers.UserGroup.Banned)
		{
			return ValidationResult.Banned;
		}
		TokenBucket tokenBucket = base.PlayerBuckets?.Get(base.Request.playerId);
		if (tokenBucket == null || !tokenBucket.TryTake(TokenCost))
		{
			if (tokenBucket == null || !tokenBucket.IsNaughty)
			{
				return ValidationResult.RateLimit;
			}
			return ValidationResult.Rejected;
		}
		UserId = base.Request.playerId;
		Player = BasePlayer.FindByID(UserId) ?? BasePlayer.FindSleeping(UserId);
		base.Client.Subscribe(new PlayerTarget(UserId));
		return ValidationResult.Success;
	}
}
