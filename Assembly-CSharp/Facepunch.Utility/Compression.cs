using System;
using Ionic.Zlib;

namespace Facepunch.Utility;

public class Compression
{
	public static byte[] Compress(byte[] data)
	{
		try
		{
			return GZipStream.CompressBuffer(data);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static byte[] Uncompress(byte[] data)
	{
		return GZipStream.UncompressBuffer(data);
	}
}
