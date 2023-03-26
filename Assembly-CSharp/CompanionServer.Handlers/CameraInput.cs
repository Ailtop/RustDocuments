using CompanionServer.Cameras;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class CameraInput : BaseHandler<AppCameraInput>
{
	protected override double TokenCost => 0.01;

	public override void Execute()
	{
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return;
		}
		if (base.Client.CurrentCamera == null || !base.Client.IsControllingCamera)
		{
			SendError("no_camera");
			return;
		}
		InputState inputState = base.Client.InputState;
		if (inputState == null)
		{
			inputState = new InputState();
			base.Client.InputState = inputState;
		}
		InputMessage obj = Pool.Get<InputMessage>();
		obj.buttons = base.Proto.buttons;
		obj.mouseDelta = Sanitize(base.Proto.mouseDelta);
		obj.aimAngles = Vector3.zero;
		inputState.Flip(obj);
		Pool.Free(ref obj);
		base.Client.CurrentCamera.UserInput(inputState, new CameraViewerId(base.Client.ControllingSteamId, base.Client.ConnectionId));
		SendSuccess();
	}

	private static Vector3 Sanitize(Vector3 value)
	{
		return new Vector3(Sanitize(value.x), Sanitize(value.y), Sanitize(value.z));
	}

	private static float Sanitize(float value)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			return 0f;
		}
		return Mathf.Clamp(value, -100f, 100f);
	}
}
