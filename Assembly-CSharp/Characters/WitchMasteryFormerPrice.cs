using System;
using UnityEngine;

namespace Characters
{
	public static class WitchMasteryFormerPrice
	{
		private static readonly int[][] prices = new int[4][]
		{
			new int[10] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },
			new int[11]
			{
				10, 30, 60, 90, 120, 150, 180, 210, 240, 270,
				300
			},
			new int[2] { 500, 1000 },
			new int[2] { 2000, 4000 }
		};

		public static int GeRefundAmount(int stage, int level)
		{
			int num = 0;
			try
			{
				int[] array = prices[stage];
				for (int i = 0; i < level; i++)
				{
					num += array[i];
				}
				return num;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				return num;
			}
		}
	}
}
