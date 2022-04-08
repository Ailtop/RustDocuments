using System;
using System.Collections.Generic;
using UnityEngine;

public static class ImageProcessing
{
	private static byte[] signaturePNG = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	private static byte[] signatureIHDR = new byte[8] { 0, 0, 0, 13, 73, 72, 68, 82 };

	public static void GaussianBlur2D(float[] data, int len1, int len2, int iterations = 1)
	{
		float[] a = data;
		float[] b = new float[len1 * len2];
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < len1; j++)
			{
				int num = Mathf.Max(0, j - 1);
				int num2 = Mathf.Min(len1 - 1, j + 1);
				for (int k = 0; k < len2; k++)
				{
					int num3 = Mathf.Max(0, k - 1);
					int num4 = Mathf.Min(len2 - 1, k + 1);
					float num5 = a[j * len2 + k] * 4f + a[j * len2 + num3] + a[j * len2 + num4] + a[num * len2 + k] + a[num2 * len2 + k];
					b[j * len2 + k] = num5 * 0.125f;
				}
			}
			GenericsUtil.Swap(ref a, ref b);
		}
		if (a != data)
		{
			Buffer.BlockCopy(a, 0, data, 0, data.Length * 4);
		}
	}

	public static void GaussianBlur2D(float[] data, int len1, int len2, int len3, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2 * len3];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					for (int k = 0; k < len3; k++)
					{
						float num5 = src[(x * len2 + j) * len3 + k] * 4f + src[(x * len2 + num3) * len3 + k] + src[(x * len2 + num4) * len3 + k] + src[(num * len2 + j) * len3 + k] + src[(num2 * len2 + j) * len3 + k];
						dst[(x * len2 + j) * len3 + k] = num5 * 0.125f;
					}
				}
			});
			GenericsUtil.Swap(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Average2D(float[] data, int len1, int len2, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					float num5 = src[x * len2 + j] + src[x * len2 + num3] + src[x * len2 + num4] + src[num * len2 + j] + src[num2 * len2 + j];
					dst[x * len2 + j] = num5 * 0.2f;
				}
			});
			GenericsUtil.Swap(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Average2D(float[] data, int len1, int len2, int len3, int iterations = 1)
	{
		float[] src = data;
		float[] dst = new float[len1 * len2 * len3];
		for (int i = 0; i < iterations; i++)
		{
			Parallel.For(0, len1, delegate(int x)
			{
				int num = Mathf.Max(0, x - 1);
				int num2 = Mathf.Min(len1 - 1, x + 1);
				for (int j = 0; j < len2; j++)
				{
					int num3 = Mathf.Max(0, j - 1);
					int num4 = Mathf.Min(len2 - 1, j + 1);
					for (int k = 0; k < len3; k++)
					{
						float num5 = src[(x * len2 + j) * len3 + k] + src[(x * len2 + num3) * len3 + k] + src[(x * len2 + num4) * len3 + k] + src[(num * len2 + j) * len3 + k] + src[(num2 * len2 + j) * len3 + k];
						dst[(x * len2 + j) * len3 + k] = num5 * 0.2f;
					}
				}
			});
			GenericsUtil.Swap(ref src, ref dst);
		}
		if (src != data)
		{
			Buffer.BlockCopy(src, 0, data, 0, data.Length * 4);
		}
	}

	public static void Upsample2D(float[] src, int srclen1, int srclen2, float[] dst, int dstlen1, int dstlen2)
	{
		if (2 * srclen1 != dstlen1 || 2 * srclen2 != dstlen2)
		{
			return;
		}
		Parallel.For(0, srclen1, delegate(int x)
		{
			int num = Mathf.Max(0, x - 1);
			int num2 = Mathf.Min(srclen1 - 1, x + 1);
			for (int i = 0; i < srclen2; i++)
			{
				int num3 = Mathf.Max(0, i - 1);
				int num4 = Mathf.Min(srclen2 - 1, i + 1);
				float num5 = src[x * srclen2 + i] * 6f;
				float num6 = num5 + src[num * srclen2 + i] + src[x * srclen2 + num3];
				dst[2 * x * dstlen2 + 2 * i] = num6 * 0.125f;
				float num7 = num5 + src[num2 * srclen2 + i] + src[x * srclen2 + num3];
				dst[(2 * x + 1) * dstlen2 + 2 * i] = num7 * 0.125f;
				float num8 = num5 + src[num * srclen2 + i] + src[x * srclen2 + num4];
				dst[2 * x * dstlen2 + (2 * i + 1)] = num8 * 0.125f;
				float num9 = num5 + src[num2 * srclen2 + i] + src[x * srclen2 + num4];
				dst[(2 * x + 1) * dstlen2 + (2 * i + 1)] = num9 * 0.125f;
			}
		});
	}

	public static void Upsample2D(float[] src, int srclen1, int srclen2, int srclen3, float[] dst, int dstlen1, int dstlen2, int dstlen3)
	{
		if (2 * srclen1 != dstlen1 || 2 * srclen2 != dstlen2 || srclen3 != dstlen3)
		{
			return;
		}
		Parallel.For(0, srclen1, delegate(int x)
		{
			int num = Mathf.Max(0, x - 1);
			int num2 = Mathf.Min(srclen1 - 1, x + 1);
			for (int i = 0; i < srclen2; i++)
			{
				int num3 = Mathf.Max(0, i - 1);
				int num4 = Mathf.Min(srclen2 - 1, i + 1);
				for (int j = 0; j < srclen3; j++)
				{
					float num5 = src[(x * srclen2 + i) * srclen3 + j] * 6f;
					float num6 = num5 + src[(num * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num3) * srclen3 + j];
					dst[(2 * x * dstlen2 + 2 * i) * dstlen3 + j] = num6 * 0.125f;
					float num7 = num5 + src[(num2 * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num3) * srclen3 + j];
					dst[((2 * x + 1) * dstlen2 + 2 * i) * dstlen3 + j] = num7 * 0.125f;
					float num8 = num5 + src[(num * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num4) * srclen3 + j];
					dst[(2 * x * dstlen2 + (2 * i + 1)) * dstlen3 + j] = num8 * 0.125f;
					float num9 = num5 + src[(num2 * srclen2 + i) * srclen3 + j] + src[(x * srclen2 + num4) * srclen3 + j];
					dst[((2 * x + 1) * dstlen2 + (2 * i + 1)) * dstlen3 + j] = num9 * 0.125f;
				}
			}
		});
	}

	public static void Dilate2D(int[] src, int len1, int len2, int srcmask, int radius, Action<int, int> action)
	{
		Parallel.For(0, len1, delegate(int x)
		{
			MaxQueue maxQueue2 = new MaxQueue(radius * 2 + 1);
			for (int k = 0; k < radius; k++)
			{
				maxQueue2.Push(src[x * len2 + k] & srcmask);
			}
			for (int l = 0; l < len2; l++)
			{
				if (l > radius)
				{
					maxQueue2.Pop();
				}
				if (l < len2 - radius)
				{
					maxQueue2.Push(src[x * len2 + l + radius] & srcmask);
				}
				if (maxQueue2.Max != 0)
				{
					action(x, l);
				}
			}
		});
		Parallel.For(0, len2, delegate(int y)
		{
			MaxQueue maxQueue = new MaxQueue(radius * 2 + 1);
			for (int i = 0; i < radius; i++)
			{
				maxQueue.Push(src[i * len2 + y] & srcmask);
			}
			for (int j = 0; j < len1; j++)
			{
				if (j > radius)
				{
					maxQueue.Pop();
				}
				if (j < len1 - radius)
				{
					maxQueue.Push(src[(j + radius) * len2 + y] & srcmask);
				}
				if (maxQueue.Max != 0)
				{
					action(j, y);
				}
			}
		});
	}

	public static void FloodFill2D(int x, int y, int[] data, int len1, int len2, int mask_any, int mask_not, Func<int, int> action)
	{
		Stack<KeyValuePair<int, int>> stack = new Stack<KeyValuePair<int, int>>();
		stack.Push(new KeyValuePair<int, int>(x, y));
		while (stack.Count > 0)
		{
			KeyValuePair<int, int> keyValuePair = stack.Pop();
			x = keyValuePair.Key;
			y = keyValuePair.Value;
			int num;
			for (num = y; num >= 0; num--)
			{
				int num2 = data[x * len2 + num];
				if ((num2 & mask_any) == 0 || (num2 & mask_not) != 0)
				{
					break;
				}
			}
			num++;
			bool flag;
			bool flag2 = (flag = false);
			for (; num < len2; num++)
			{
				int num3 = data[x * len2 + num];
				if ((num3 & mask_any) == 0 || (num3 & mask_not) != 0)
				{
					break;
				}
				data[x * len2 + num] = action(num3);
				if (x > 0)
				{
					int num4 = data[(x - 1) * len2 + num];
					bool flag3 = (num4 & mask_any) != 0 && (num4 & mask_not) == 0;
					if (!flag2 && flag3)
					{
						stack.Push(new KeyValuePair<int, int>(x - 1, num));
						flag2 = true;
					}
					else if (flag2 && !flag3)
					{
						flag2 = false;
					}
				}
				if (x < len1 - 1)
				{
					int num5 = data[(x + 1) * len2 + num];
					bool flag4 = (num5 & mask_any) != 0 && (num5 & mask_not) == 0;
					if (!flag && flag4)
					{
						stack.Push(new KeyValuePair<int, int>(x + 1, num));
						flag = true;
					}
					else if (flag && !flag4)
					{
						flag = false;
					}
				}
			}
		}
	}

	public static bool IsValidPNG(byte[] data, int maxWidth, int maxHeight)
	{
		if (data == null || data.Length < 29)
		{
			return false;
		}
		if (data.Length > 29 + maxWidth * maxHeight * 4)
		{
			return false;
		}
		for (int i = 0; i < signaturePNG.Length; i++)
		{
			if (data[i] != signaturePNG[i])
			{
				return false;
			}
		}
		for (int j = 0; j < signatureIHDR.Length; j++)
		{
			if (data[8 + j] != signatureIHDR[j])
			{
				return false;
			}
		}
		Union32 union = default(Union32);
		union.b4 = data[16];
		union.b3 = data[17];
		union.b2 = data[18];
		union.b1 = data[19];
		if (union.i < 1 || union.i > maxWidth)
		{
			return false;
		}
		Union32 union2 = default(Union32);
		union2.b4 = data[20];
		union2.b3 = data[21];
		union2.b2 = data[22];
		union2.b1 = data[23];
		if (union2.i < 1 || union2.i > maxHeight)
		{
			return false;
		}
		byte b = data[24];
		if (b != 8 && b != 16)
		{
			return false;
		}
		byte b2 = data[25];
		if (b2 != 2 && b2 != 6)
		{
			return false;
		}
		if (data[26] != 0)
		{
			return false;
		}
		if (data[27] != 0)
		{
			return false;
		}
		if (data[28] != 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidJPG(byte[] data, int maxWidth, int maxHeight)
	{
		if (data.Length < 30)
		{
			return false;
		}
		if (data.Length > 30 + maxWidth * maxHeight)
		{
			return false;
		}
		try
		{
			if (data[0] != byte.MaxValue || data[1] != 216)
			{
				return false;
			}
			if (data[2] != byte.MaxValue || data[3] != 224)
			{
				return false;
			}
			if (data[6] != 74 || data[7] != 70 || data[8] != 73 || data[9] != 70 || data[10] != 0)
			{
				return false;
			}
			if (data[13] != 0)
			{
				return false;
			}
			if (data[14] != data[16] || data[15] != data[17])
			{
				return false;
			}
			int num = 4;
			int num2 = (data[num] << 8) | data[num + 1];
			while (num < data.Length)
			{
				num += num2;
				if (num >= data.Length)
				{
					return false;
				}
				if (data[num] != byte.MaxValue)
				{
					return false;
				}
				if (data[num + 1] == 192 || data[num + 1] == 193 || data[num + 1] == 194)
				{
					int num3 = (data[num + 5] << 8) | data[num + 6];
					return ((data[num + 7] << 8) | data[num + 8]) <= maxWidth && num3 <= maxHeight;
				}
				num += 2;
				num2 = (data[num] << 8) | data[num + 1];
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsClear(Color32[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i].a > 5)
			{
				return false;
			}
		}
		return true;
	}
}
