using System.Collections.Generic;
using System.Linq;
using Characters.Operations.Fx;
using FX;
using UnityEditor;
using UnityEngine;

namespace Level.Chapter4
{
	public class PlatformContainer : MonoBehaviour
	{
		private class Assets
		{
			internal static EffectInfo appearance = new EffectInfo(Resource.instance.enemyAppearanceEffect);
		}

		[SerializeField]
		private Platform _center;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawnEffect;

		[SerializeField]
		private Transform _container;

		[SerializeReference]
		public List<INode> multipleNodes = new List<INode>
		{
			new Node(),
			new DerivedNode()
		};

		private Platform[] _platforms;

		public Platform center => _center;

		private void Awake()
		{
			_platforms = new Platform[_container.childCount - 1];
			int num = 0;
			foreach (Transform item in _container)
			{
				if (!(_center.transform == item))
				{
					Platform component = item.GetComponent<Platform>();
					if (!(component == null))
					{
						_platforms[num++] = component;
					}
				}
			}
		}

		public void RandomTakeTo(Platform[] platforms)
		{
			_platforms.Shuffle();
			Platform[] array = _platforms.Take(platforms.Length).ToArray();
			for (int i = 0; i < platforms.Length; i++)
			{
				platforms[i] = array[i];
			}
		}

		public bool CanPurify()
		{
			Platform[] platforms = _platforms;
			for (int i = 0; i < platforms.Length; i++)
			{
				if (!platforms[i].tentacleAlives)
				{
					return true;
				}
			}
			return false;
		}

		public void NoTentacleTakeTo(Platform[] results)
		{
			_platforms.Shuffle();
			for (int i = 0; i < results.Length; i++)
			{
				results[i] = null;
			}
			int num = 0;
			int num2 = 0;
			int num3 = results.Length;
			int num4 = _platforms.Length;
			while (num < num3 && num2 < num4)
			{
				if (_platforms[num2].tentacleAlives)
				{
					num2++;
				}
				else
				{
					results[num++] = _platforms[num2++];
				}
			}
		}

		public void Appear()
		{
			foreach (Transform item in _container)
			{
				Assets.appearance.Spawn(item.position);
				item.gameObject.SetActive(true);
			}
		}
	}
}
