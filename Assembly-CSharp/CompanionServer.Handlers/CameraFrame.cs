using CCTVRender;
using ProtoBuf;

namespace CompanionServer.Handlers
{
	public class CameraFrame : BaseHandler<AppCameraFrameRequest>
	{
		protected override int TokenCost => 2;

		public override void Execute()
		{
			if (!Settings.Enabled)
			{
				SendError("disabled");
				return;
			}
			if (!ComputerStation.IsValidIdentifier(base.Proto.identifier))
			{
				SendError("invalid");
				return;
			}
			IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(base.Proto.identifier);
			CCTV_RC camera;
			if (remoteControllable == null || (object)(camera = remoteControllable as CCTV_RC) == null)
			{
				SendError("not_found");
			}
			else if (!Manager.TryRequest(base.Request.seq, base.Client, camera, base.Proto.frame))
			{
				SendError("unavailable");
			}
		}
	}
}
