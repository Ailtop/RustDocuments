using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public abstract class BaseHandler<T> : IHandler, Pool.IPooled where T : class
{
	protected TokenBucketList<ulong> PlayerBuckets { get; private set; }

	protected virtual double TokenCost => 1.0;

	public IConnection Client { get; private set; }

	public AppRequest Request { get; private set; }

	public T Proto { get; private set; }

	public void Initialize(TokenBucketList<ulong> playerBuckets, IConnection client, AppRequest request, T proto)
	{
		PlayerBuckets = playerBuckets;
		Client = client;
		Request = request;
		Proto = proto;
	}

	public virtual void EnterPool()
	{
		PlayerBuckets = null;
		Client = null;
		if (Request != null)
		{
			Request.Dispose();
			Request = null;
		}
		Proto = null;
	}

	public void LeavePool()
	{
	}

	public virtual ValidationResult Validate()
	{
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
