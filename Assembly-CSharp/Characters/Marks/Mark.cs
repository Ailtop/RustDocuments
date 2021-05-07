using System.Collections.Generic;
using FX;
using UnityEngine;

namespace Characters.Marks
{
	public class Mark : MonoBehaviour
	{
		private static readonly EffectInfo stackImageBase = new EffectInfo(Resource.instance.emptyEffect)
		{
			loop = true,
			flipXByOwnerDirection = false
		};

		private readonly List<MarkInfo> _markInfo = new List<MarkInfo>();

		private readonly List<float> _stacks = new List<float>();

		private readonly List<ReusableChronoSpriteEffect> _stackImages = new List<ReusableChronoSpriteEffect>();

		public Character owner { get; private set; }

		public static Mark AddComponent(Character owner)
		{
			Mark mark = owner.gameObject.AddComponent<Mark>();
			mark.owner = owner;
			mark.owner.health.onDied += mark.ClearAllStack;
			return mark;
		}

		public void AddStack(MarkInfo mark, float count = 1f)
		{
			if (owner.type == Character.Type.Trap)
			{
				return;
			}
			int num = _markInfo.IndexOf(mark);
			int num2 = (int)count;
			float num3 = count - (float)num2;
			if (num == -1)
			{
				_markInfo.Add(mark);
				_stacks.Add(0f);
				stackImageBase.attachInfo = mark.attachInfo;
				ReusableChronoSpriteEffect item = stackImageBase.Spawn(base.transform.position, owner);
				_stackImages.Add(item);
				num = _markInfo.Count - 1;
			}
			float num4 = _stacks[num];
			if ((int)num4 == (int)(num4 + num3))
			{
				_stacks[num] += num3;
			}
			else
			{
				_stacks[num] += num3 - 1f;
				num2++;
			}
			for (int i = 0; i < num2; i++)
			{
				if ((float)mark.maxStack <= _stacks[num])
				{
					_stacks[num] = mark.maxStack;
					break;
				}
				num4 = (_stacks[num] += 1f);
				mark.onStack?.Invoke(this, num4);
			}
			num = _markInfo.IndexOf(mark);
			if (num >= 0)
			{
				UpdateStackImage(_stackImages[num], mark, _stacks[num]);
			}
		}

		public float GetStack(MarkInfo mark)
		{
			int num = _markInfo.IndexOf(mark);
			if (num == -1)
			{
				return 0f;
			}
			return _stacks[num];
		}

		public float TakeAllStack(MarkInfo mark)
		{
			int num = _markInfo.IndexOf(mark);
			if (num == -1)
			{
				return 0f;
			}
			float result = _stacks[num];
			ClearStack(num);
			return result;
		}

		public float TakeStack(MarkInfo mark, float count)
		{
			int num = _markInfo.IndexOf(mark);
			if (num == -1)
			{
				return 0f;
			}
			float result;
			if (_stacks[num] > count)
			{
				result = count;
				_stacks[num] -= count;
			}
			else
			{
				result = _stacks[num];
				_stacks[num] = 0f;
			}
			UpdateStackImage(_stackImages[num], mark, _stacks[num]);
			return result;
		}

		public void ClearAllStack()
		{
			_markInfo.Clear();
			_stacks.Clear();
			foreach (ReusableChronoSpriteEffect stackImage in _stackImages)
			{
				stackImage.reusable.Despawn();
			}
			_stackImages.Clear();
		}

		public void ClearStack(MarkInfo mark)
		{
			ClearStack(_markInfo.IndexOf(mark));
		}

		private void ClearStack(int index)
		{
			_markInfo.RemoveAt(index);
			_stacks.RemoveAt(index);
			_stackImages[index].reusable.Despawn();
			_stackImages.RemoveAt(index);
		}

		private void UpdateStackImage(ReusableChronoSpriteEffect effect, MarkInfo mark, float stacks)
		{
			if (!(stacks < 1f))
			{
				int num = Mathf.Clamp((int)stacks - 1, 0, mark.stackImages.Length - 1);
				effect.renderer.sprite = mark.stackImages[num];
			}
		}
	}
}
