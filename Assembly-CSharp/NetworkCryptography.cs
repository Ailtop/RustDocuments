using System;
using Network;

public abstract class NetworkCryptography : INetworkCryptocraphy
{
	private byte[] buffer = new byte[8388608];

	public unsafe ArraySegment<byte> EncryptCopy(Connection connection, ArraySegment<byte> data)
	{
		ArraySegment<byte> src = new ArraySegment<byte>(data.Array, data.Offset, data.Count);
		ArraySegment<byte> dst = new ArraySegment<byte>(buffer, data.Offset, buffer.Length - data.Offset);
		if (data.Offset > 0)
		{
			fixed (byte* destination = dst.Array)
			{
				fixed (byte* source = data.Array)
				{
					Buffer.MemoryCopy(source, destination, dst.Array.Length, data.Offset);
				}
			}
		}
		EncryptionHandler(connection, src, ref dst);
		return dst;
	}

	public unsafe ArraySegment<byte> DecryptCopy(Connection connection, ArraySegment<byte> data)
	{
		ArraySegment<byte> src = new ArraySegment<byte>(data.Array, data.Offset, data.Count);
		ArraySegment<byte> dst = new ArraySegment<byte>(buffer, data.Offset, buffer.Length - data.Offset);
		if (data.Offset > 0)
		{
			fixed (byte* destination = dst.Array)
			{
				fixed (byte* source = data.Array)
				{
					Buffer.MemoryCopy(source, destination, dst.Array.Length, data.Offset);
				}
			}
		}
		DecryptionHandler(connection, src, ref dst);
		return dst;
	}

	public void Encrypt(Connection connection, ref ArraySegment<byte> data)
	{
		ArraySegment<byte> src = new ArraySegment<byte>(data.Array, data.Offset, data.Count);
		ArraySegment<byte> dst = new ArraySegment<byte>(data.Array, data.Offset, data.Array.Length - data.Offset);
		EncryptionHandler(connection, src, ref dst);
		data = dst;
	}

	public void Decrypt(Connection connection, ref ArraySegment<byte> data)
	{
		ArraySegment<byte> src = new ArraySegment<byte>(data.Array, data.Offset, data.Count);
		ArraySegment<byte> dst = new ArraySegment<byte>(data.Array, data.Offset, data.Array.Length - data.Offset);
		DecryptionHandler(connection, src, ref dst);
		data = dst;
	}

	public bool IsEnabledIncoming(Connection connection)
	{
		if (connection != null && connection.encryptionLevel != 0)
		{
			return connection.decryptIncoming;
		}
		return false;
	}

	public bool IsEnabledOutgoing(Connection connection)
	{
		if (connection != null && connection.encryptionLevel != 0)
		{
			return connection.encryptOutgoing;
		}
		return false;
	}

	protected abstract void EncryptionHandler(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst);

	protected abstract void DecryptionHandler(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst);
}
