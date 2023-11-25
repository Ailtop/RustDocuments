using System;
using Network;

public class NetworkCryptographyServer : NetworkCryptography
{
	protected override void EncryptionHandler(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		if (connection.encryptionLevel > 1)
		{
			EACServer.Encrypt(connection, src, ref dst);
		}
		else
		{
			Craptography.XOR(2511u, src, ref dst);
		}
	}

	protected override void DecryptionHandler(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		if (connection.encryptionLevel > 1)
		{
			EACServer.Decrypt(connection, src, ref dst);
		}
		else
		{
			Craptography.XOR(2511u, src, ref dst);
		}
	}
}
