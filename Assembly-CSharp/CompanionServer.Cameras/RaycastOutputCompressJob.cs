using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace CompanionServer.Cameras;

public struct RaycastOutputCompressJob : IJob
{
	[Unity.Collections.ReadOnly]
	public NativeArray<int> rayOutputs;

	[WriteOnly]
	public NativeArray<int> dataLength;

	[WriteOnly]
	public NativeArray<byte> data;

	public void Execute()
	{
		int num = rayOutputs.Length * 4;
		if (data.Length < num)
		{
			throw new InvalidOperationException("Not enough data buffer available to compress rays");
		}
		NativeArray<int> nativeArray = new NativeArray<int>(64, Allocator.Temp);
		int value = 0;
		for (int i = 0; i < rayOutputs.Length; i++)
		{
			int num2 = rayOutputs[i];
			ushort num3 = RayDistance(num2);
			byte b = RayAlignment(num2);
			byte b2 = RayMaterial(num2);
			int num4 = ((int)num3 / 128 * 3 + (int)b / 16 * 5 + b2 * 7) & 0x3F;
			int num5 = nativeArray[num4];
			if (num5 == num2)
			{
				data[value++] = (byte)(0u | (uint)num4);
				continue;
			}
			int num6 = num3 - RayDistance(num5);
			int num7 = b - RayAlignment(num5);
			if (b2 == RayMaterial(num5) && num6 >= -15 && num6 <= 16 && num7 >= -3 && num7 <= 4)
			{
				data[value++] = (byte)(0x40u | (uint)num4);
				data[value++] = (byte)((num6 + 15 << 3) | (num7 + 3));
			}
			else if (b2 == RayMaterial(num5) && num7 == 0 && num6 >= -127 && num6 <= 128)
			{
				data[value++] = (byte)(0x80u | (uint)num4);
				data[value++] = (byte)(num6 + 127);
			}
			else if (b2 < 63)
			{
				nativeArray[num4] = num2;
				data[value++] = (byte)(0xC0u | b2);
				data[value++] = (byte)(num3 >> 2);
				data[value++] = (byte)(((num3 & 3) << 6) | b);
			}
			else
			{
				nativeArray[num4] = num2;
				data[value++] = byte.MaxValue;
				data[value++] = (byte)(num3 >> 2);
				data[value++] = (byte)(((num3 & 3) << 6) | b);
				data[value++] = b2;
			}
		}
		nativeArray.Dispose();
		dataLength[0] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ushort RayDistance(int ray)
	{
		return (ushort)(ray >> 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte RayAlignment(int ray)
	{
		return (byte)(ray >> 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte RayMaterial(int ray)
	{
		return (byte)ray;
	}
}
