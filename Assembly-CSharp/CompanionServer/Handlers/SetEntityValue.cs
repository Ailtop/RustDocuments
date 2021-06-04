using ProtoBuf;

namespace CompanionServer.Handlers
{
	public class SetEntityValue : BaseEntityHandler<AppSetEntityValue>
	{
		public override void Execute()
		{
			SmartSwitch smartSwitch;
			if ((object)(smartSwitch = base.Entity as SmartSwitch) != null)
			{
				smartSwitch.Value = base.Proto.value;
				SendSuccess();
			}
			else
			{
				SendError("wrong_type");
			}
		}
	}
}
