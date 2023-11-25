using System;
using Facepunch;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using ProtoBuf.Nexus;
using UnityEngine;

namespace Rust.Nexus.Handlers;

public abstract class BaseNexusRequestHandler<T> : INexusRequestHandler, Pool.IPooled where T : class
{
	private bool _fireAndForget;

	protected NexusZoneDetails FromZone { get; private set; }

	protected Uuid RequestId { get; private set; }

	protected T Request { get; private set; }

	public Response Response { get; private set; }

	public void Initialize(NexusZoneDetails fromZone, Uuid id, bool fireAndForget, T request)
	{
		FromZone = fromZone;
		RequestId = id;
		Request = request;
		_fireAndForget = fireAndForget;
	}

	public void EnterPool()
	{
		FromZone = null;
		RequestId = Uuid.Empty;
		Request = null;
		Response = null;
		_fireAndForget = false;
		Reset();
	}

	public void LeavePool()
	{
	}

	public void Execute()
	{
		try
		{
			Handle();
			if (Response == null)
			{
				SendSuccess();
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			if (Response != null)
			{
				Debug.LogError("Nexus RPC handler threw an exception but already sent a response!");
			}
			else
			{
				SendError(ex.Message);
			}
		}
	}

	protected abstract void Handle();

	protected virtual void Reset()
	{
	}

	protected void SendSuccess()
	{
		SendResult(success: true);
	}

	protected void SendSuccess(Response response)
	{
		SendResult(success: true, response);
	}

	protected void SendResult(bool success)
	{
		SendResult(success, NewResponse());
	}

	protected void SendResult(bool success, Response response)
	{
		if (_fireAndForget)
		{
			response.Dispose();
			return;
		}
		if (Response != null)
		{
			response.Dispose();
			throw new InvalidOperationException("Already sent a response for this nexus RPC invocation!");
		}
		response.id = RequestId;
		response.status = Pool.Get<Status>();
		response.status.success = success;
		Response = response;
	}

	protected void SendError(string message)
	{
		if (!_fireAndForget)
		{
			if (Response != null)
			{
				throw new InvalidOperationException("Already sent a response for this nexus RPC invocation!");
			}
			Response response = Pool.Get<Response>();
			response.id = RequestId;
			response.status = Pool.Get<Status>();
			response.status.success = false;
			response.status.errorMessage = message;
			Response = response;
		}
	}

	protected static Response NewResponse()
	{
		return Pool.Get<Response>();
	}
}
