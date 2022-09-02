using System.IO;
using Network;

public class NetworkCryptographyServer : NetworkCryptography
{
	protected override void EncryptionHandler(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
	{
		if (connection.encryptionLevel > 1)
		{
			EACServer.Encrypt(connection, src, srcOffset, dst, dstOffset);
		}
		else
		{
			Craptography.XOR(2356u, src, srcOffset, dst, dstOffset);
		}
	}

	protected override void DecryptionHandler(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
	{
		if (connection.encryptionLevel > 1)
		{
			EACServer.Decrypt(connection, src, srcOffset, dst, dstOffset);
		}
		else
		{
			Craptography.XOR(2356u, src, srcOffset, dst, dstOffset);
		}
	}
}
