using Network;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public static class NetworkWriteEx
{
	public static void WriteObject<T>(this NetWrite write, T obj)
	{
		if (typeof(T) == typeof(Vector3))
		{
			Vector3 obj2 = GenericsUtil.Cast<T, Vector3>(obj);
			write.Vector3(in obj2);
		}
		else if (typeof(T) == typeof(Ray))
		{
			Ray obj3 = GenericsUtil.Cast<T, Ray>(obj);
			write.Ray(in obj3);
		}
		else if (typeof(T) == typeof(float))
		{
			write.Float(GenericsUtil.Cast<T, float>(obj));
		}
		else if (typeof(T) == typeof(short))
		{
			write.Int16(GenericsUtil.Cast<T, short>(obj));
		}
		else if (typeof(T) == typeof(ushort))
		{
			write.UInt16(GenericsUtil.Cast<T, ushort>(obj));
		}
		else if (typeof(T) == typeof(int))
		{
			write.Int32(GenericsUtil.Cast<T, int>(obj));
		}
		else if (typeof(T) == typeof(uint))
		{
			write.UInt32(GenericsUtil.Cast<T, uint>(obj));
		}
		else if (typeof(T) == typeof(byte[]))
		{
			write.Bytes(GenericsUtil.Cast<T, byte[]>(obj));
		}
		else if (typeof(T) == typeof(long))
		{
			write.Int64(GenericsUtil.Cast<T, long>(obj));
		}
		else if (typeof(T) == typeof(ulong))
		{
			write.UInt64(GenericsUtil.Cast<T, ulong>(obj));
		}
		else if (typeof(T) == typeof(string))
		{
			write.String(GenericsUtil.Cast<T, string>(obj));
		}
		else if (typeof(T) == typeof(sbyte))
		{
			write.Int8(GenericsUtil.Cast<T, sbyte>(obj));
		}
		else if (typeof(T) == typeof(byte))
		{
			write.UInt8(GenericsUtil.Cast<T, byte>(obj));
		}
		else if (typeof(T) == typeof(bool))
		{
			write.Bool(GenericsUtil.Cast<T, bool>(obj));
		}
		else if (typeof(T) == typeof(Color))
		{
			Color obj4 = GenericsUtil.Cast<T, Color>(obj);
			write.Color(in obj4);
		}
		else if (typeof(T) == typeof(Color32))
		{
			Color32 obj5 = GenericsUtil.Cast<T, Color32>(obj);
			write.Color32(in obj5);
		}
		else if (typeof(T) == typeof(NetworkableId))
		{
			write.EntityID(GenericsUtil.Cast<T, NetworkableId>(obj));
		}
		else if (typeof(T) == typeof(ItemContainerId))
		{
			write.ItemContainerID(GenericsUtil.Cast<T, ItemContainerId>(obj));
		}
		else if (typeof(T) == typeof(ItemId))
		{
			write.ItemID(GenericsUtil.Cast<T, ItemId>(obj));
		}
		else if (obj is IProto proto)
		{
			proto.WriteToStream(write);
		}
		else
		{
			T val = obj;
			Debug.LogError("NetworkData.Write - no handler to write " + val?.ToString() + " -> " + obj.GetType());
		}
	}
}
