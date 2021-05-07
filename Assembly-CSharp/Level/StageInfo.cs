using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Data;
using Level.Npc;
using UnityEngine;

namespace Level
{
	[CreateAssetMenu]
	public class StageInfo : IStageInfo
	{
		[Serializable]
		public class ExtraMapInfo : SerializablePathNode
		{
			[Serializable]
			internal new class Reorderable : ReorderableArray<ExtraMapInfo>
			{
			}

			[Range(0f, 100f)]
			public float possibility = 100f;

			[MinMaxSlider(0f, 100f)]
			public Vector2Int positionRange;
		}

		[SerializeField]
		private SerializablePathNode _entry;

		[SerializeField]
		private SerializablePathNode _terminal;

		[SerializeField]
		private Gate.Type _lastGate;

		[SerializeField]
		private ExtraMapInfo _castleNpc;

		[SerializeField]
		private NpcType _npcType;

		[SerializeField]
		private ParallaxBackground _background;

		[SerializeField]
		[Tooltip("일반 전투 맵 개수")]
		private Vector2Int _normalMaps;

		[SerializeField]
		[Tooltip("헤드 보상 맵 개수")]
		private Vector2Int _headRewards;

		[SerializeField]
		[Tooltip("아이템 보상 맵 개수")]
		private Vector2Int _itemRewards;

		[Header("Special Maps")]
		[SerializeField]
		[Tooltip("해당 스테이지에 스페셜 맵이 n개 나올 비중. 예를 들어 [0, 30, 70]이면 30% 확률로 1개 등장, 70% 확률로 2개 등장")]
		private float[] _specialMapWeights;

		[Header("Extra Maps")]
		[SerializeField]
		private ExtraMapInfo.Reorderable _extraMaps;

		private int _current = -1;

		[TupleElementNames(new string[] { "type1", "type2" })]
		private ValueTuple<PathNode, PathNode>[] _path;

		private ILookup<Map.Type, Resource.MapReference> _maps;

		private EnumArray<Map.Type, List<Resource.MapReference>> _remainMaps = new EnumArray<Map.Type, List<Resource.MapReference>>();

		[TupleElementNames(new string[] { "node1", "node2" })]
		public override ValueTuple<PathNode, PathNode> nextMapTypes
		{
			[return: TupleElementNames(new string[] { "node1", "node2" })]
			get
			{
				if (_current + 1 >= _path.Length)
				{
					return new ValueTuple<PathNode, PathNode>(PathNode.none, PathNode.none);
				}
				return _path[_current + 1];
			}
		}

		public override ParallaxBackground background => _background;

		public override void Initialize()
		{
			_maps = maps.ToLookup((Resource.MapReference m) => m.type);
		}

		private void GeneratePath()
		{
			int num = UnityEngine.Random.Range(_normalMaps.x, _normalMaps.y + 1);
			int num2 = UnityEngine.Random.Range(_headRewards.x, _headRewards.y + 1);
			int num3 = UnityEngine.Random.Range(_itemRewards.x, _itemRewards.y + 1);
			if (num2 > num)
			{
				throw new ArgumentOutOfRangeException("headRewards", "headRewards must be less than normalMaps");
			}
			if (num3 > num)
			{
				throw new ArgumentOutOfRangeException("itemRewards", "itemRewards must be less than normalMaps");
			}
			ValueTuple<PathNode, PathNode>[] array = new ValueTuple<PathNode, PathNode>[num];
			for (int i = 0; i < array.Length; i++)
			{
				List<Resource.MapReference> list = _remainMaps[Map.Type.Normal];
				int index = list.RandomIndex();
				Resource.MapReference reference = list[index];
				list.RemoveAt(index);
				index = list.RandomIndex();
				Resource.MapReference reference2 = list[index];
				list.RemoveAt(index);
				array[i] = new ValueTuple<PathNode, PathNode>(new PathNode(reference, MapReward.Type.Gold, Gate.Type.Normal), new PathNode(reference2, MapReward.Type.Gold, Gate.Type.Normal));
			}
			if (num2 > 0)
			{
				int[] array2 = MMMaths.MultipleRandomWithoutDuplactes(num2, 0, num);
				foreach (int num4 in array2)
				{
					array[num4].Item1.reward = MapReward.Type.Head;
					array[num4].Item1.gate = Gate.Type.Grave;
				}
			}
			if (num3 > 0)
			{
				int[] array2 = MMMaths.MultipleRandomWithoutDuplactes(num3, 0, num);
				foreach (int num5 in array2)
				{
					array[num5].Item2.reward = MapReward.Type.Item;
					array[num5].Item2.gate = Gate.Type.Chest;
				}
			}
			if (_remainMaps[Map.Type.Special] != null)
			{
				List<Resource.MapReference> list2 = _remainMaps[Map.Type.Special].Where((Resource.MapReference m) => !SpecialMap.GetEncoutered(m.specialMapType)).ToList();
				int[] array3 = MMMaths.MultipleRandomWithoutDuplactes(Math.Min(GetSpecialMapCount(), list2.Count), 0, num);
				for (int k = 0; k < array3.Length; k++)
				{
					int index2 = list2.RandomIndex();
					Resource.MapReference reference3 = list2[index2];
					list2.RemoveAt(index2);
					int num6 = array3[k];
					array[num6].Item1.reference = reference3;
					array[num6].Item2.reference = reference3;
				}
			}
			for (int l = 0; l < array.Length; l++)
			{
				if (MMMaths.RandomBool())
				{
					PathNode item = array[l].Item1;
					array[l].Item1 = array[l].Item2;
					array[l].Item2 = item;
				}
			}
			List<ValueTuple<PathNode, PathNode>> list3 = new List<ValueTuple<PathNode, PathNode>>(num + 2);
			list3.AddRange(array);
			if (!_entry.reference.IsNullOrEmpty())
			{
				list3.Insert(0, new ValueTuple<PathNode, PathNode>(_entry, PathNode.none));
			}
			if (!_terminal.reference.IsNullOrEmpty())
			{
				list3.Add(new ValueTuple<PathNode, PathNode>(_terminal, PathNode.none));
			}
			List<ExtraMapInfo> list4 = new List<ExtraMapInfo>();
			ExtraMapInfo[] values = _extraMaps.values;
			foreach (ExtraMapInfo extraMapInfo in values)
			{
				if (MMMaths.Chance(extraMapInfo.possibility / 100f))
				{
					list4.Add(extraMapInfo);
				}
			}
			if (!_castleNpc.reference.IsNullOrEmpty() && !GameData.Progress.GetRescued(_npcType))
			{
				list4.Add(_castleNpc);
			}
			foreach (ExtraMapInfo item2 in list4)
			{
				int min = Mathf.RoundToInt((float)(num * item2.positionRange.x) * 0.01f);
				int max = Mathf.RoundToInt((float)(num * item2.positionRange.y) * 0.01f);
				int index3 = UnityEngine.Random.Range(min, max) + 1;
				list3.Insert(index3, new ValueTuple<PathNode, PathNode>(item2, item2));
			}
			PathNode pathNode = new PathNode(null, MapReward.Type.None, _lastGate);
			list3.Add(new ValueTuple<PathNode, PathNode>(pathNode, pathNode));
			_path = list3.ToArray();
		}

		private int GetSpecialMapCount()
		{
			float num = UnityEngine.Random.Range(0f, _specialMapWeights.Sum());
			for (int i = 0; i < _specialMapWeights.Length; i++)
			{
				num -= _specialMapWeights[i];
				if (num <= 0f)
				{
					return i;
				}
			}
			return 0;
		}

		public override void Reset()
		{
			foreach (IGrouping<Map.Type, Resource.MapReference> map in _maps)
			{
				_remainMaps[map.Key] = map.ToList();
			}
			GeneratePath();
			_current = -1;
		}

		public override bool Next()
		{
			_current++;
			return _current <= _path.Length;
		}

		public override void UpdateReferences()
		{
		}
	}
}
