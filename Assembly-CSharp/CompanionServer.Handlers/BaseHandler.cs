using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public abstract class BaseHandler<T> : IHandler, Pool.IPooled where T : class
{
	private TokenBucketList<ulong> _playerBuckets;

	protected virtual int TokenCost => 1;

	public IConnection Client { get; private set; }

	public AppRequest Request { get; private set; }

	public T Proto { get; private set; }

	protected ulong UserId { get; private set; }

	protected BasePlayer Player { get; private set; }

	public void Initialize(TokenBucketList<ulong> playerBuckets, IConnection client, AppRequest request, T proto)
	{
		_playerBuckets = playerBuckets;
		Client = client;
		Request = request;
		Proto = proto;
	}

	public virtual void EnterPool()
	{
		_playerBuckets = null;
		Client = null;
		if (Request != null)
		{
			Request.Dispose();
			Request = null;
		}
		Proto = null;
		UserId = 0uL;
		Player = null;
	}

	public void LeavePool()
	{
	}

	public virtual ValidationResult Validate()
	{
		bool locked;
		int orGenerateAppToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(Request.playerId, out locked);
		if (Request.playerId == 0L || Request.playerToken != orGenerateAppToken)
		{
			return ValidationResult.NotFound;
		}
		if (locked)
		{
			return ValidationResult.Banned;
		}
		if ((ServerUsers.Get(Request.playerId)?.group ?? ServerUsers.UserGroup.None) == ServerUsers.UserGroup.Banned)
		{
			return ValidationResult.Banned;
		}
		TokenBucket tokenBucket = _playerBuckets?.Get(Request.playerId);
		if (tokenBucket == null || !tokenBucket.TryTake(TokenCost))
		{
			if (tokenBucket == null || !tokenBucket.IsNaughty)
			{
				return ValidationResult.RateLimit;
			}
			return ValidationResult.Rejected;
		}
		UserId = Request.playerId;
		Player = BasePlayer.FindByID(UserId) ?? BasePlayer.FindSleeping(UserId);
		Client.Subscribe(new PlayerTarget(UserId));
		return ValidationResult.Success;
	}

	public abstract void Execute();

	protected void SendSuccess()
	{
		AppSuccess success = Pool.Get<AppSuccess>();
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.success = success;
		Send(appResponse);
	}

	public void SendError(string code)
	{
		AppError appError = Pool.Get<AppError>();
		appError.error = code;
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.error = appError;
		Send(appResponse);
	}

	public void SendFlag(bool value)
	{
		AppFlag appFlag = Pool.Get<AppFlag>();
		appFlag.value = value;
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.flag = appFlag;
		Send(appResponse);
	}

	protected void Send(AppResponse response)
	{
		response.seq = Request.seq;
		Client.Send(response);
	}
}
