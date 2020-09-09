using Network;
using Network.Visibility;
using UnityEngine;

public static class EffectNetwork
{
	public static void Send(Effect effect)
	{
		if (Net.sv != null && Net.sv.IsConnected())
		{
			using (TimeWarning.New("EffectNetwork.Send"))
			{
				Group group = null;
				if (!string.IsNullOrEmpty(effect.pooledString))
				{
					effect.pooledstringid = StringPool.Get(effect.pooledString);
				}
				if (effect.pooledstringid == 0)
				{
					Debug.Log("String ID is 0 - unknown effect " + effect.pooledString);
				}
				else if (effect.broadcast)
				{
					if (Net.sv.write.Start())
					{
						Net.sv.write.PacketID(Message.Type.Effect);
						effect.WriteToStream(Net.sv.write);
						Net.sv.write.Send(new SendInfo(BaseNetworkable.GlobalNetworkGroup.subscribers));
					}
				}
				else
				{
					if (effect.entity == 0)
					{
						group = Net.sv.visibility.GetGroup(effect.worldPos);
						goto IL_0113;
					}
					BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(effect.entity) as BaseEntity;
					if (BaseEntityEx.IsValid(baseEntity))
					{
						group = baseEntity.net.group;
						goto IL_0113;
					}
				}
				goto end_IL_0021;
				IL_0113:
				if (group != null)
				{
					Net.sv.write.Start();
					Net.sv.write.PacketID(Message.Type.Effect);
					effect.WriteToStream(Net.sv.write);
					Net.sv.write.Send(new SendInfo(group.subscribers));
				}
				end_IL_0021:;
			}
		}
	}

	public static void Send(Effect effect, Connection target)
	{
		effect.pooledstringid = StringPool.Get(effect.pooledString);
		if (effect.pooledstringid == 0)
		{
			Debug.LogWarning("EffectNetwork.Send - unpooled effect name: " + effect.pooledString);
			return;
		}
		Net.sv.write.Start();
		Net.sv.write.PacketID(Message.Type.Effect);
		effect.WriteToStream(Net.sv.write);
		Net.sv.write.Send(new SendInfo(target));
	}
}
