using System.Collections.Generic;
using Characters.Controllers;
using Singletons;
using UnityEngine;
using UserInput;

namespace UI
{
	public class NavigationManager : PersistentSingleton<NavigationManager>
	{
		private const float _navigatingInterval = 0.3f;

		private float _remainNavigatingInterval;

		private readonly Stack<INavigatable> _navigatables = new Stack<INavigatable>();

		public void Push(INavigatable navigatable)
		{
			_navigatables.Push(navigatable);
			if (_navigatables.Count == 1)
			{
				PlayerInput.blocked.Attach(this);
			}
		}

		public void Pop()
		{
			_navigatables.Pop();
			if (_navigatables.Count == 0)
			{
				PlayerInput.blocked.Detach(this);
			}
		}

		private void Update()
		{
			if (_navigatables.Count == 0)
			{
				return;
			}
			INavigatable navigatable = _navigatables.Peek();
			if (KeyMapper.Map.Submit.IsPressed)
			{
				navigatable.Submit();
				return;
			}
			if (KeyMapper.Map.Cancel.IsPressed)
			{
				navigatable.Cancel();
				return;
			}
			_remainNavigatingInterval -= Time.deltaTime;
			if (!(_remainNavigatingInterval > 0f))
			{
				if (KeyMapper.Map.Up.IsPressed)
				{
					_remainNavigatingInterval = 0.3f;
					navigatable.Up();
				}
				else if (KeyMapper.Map.Down.IsPressed)
				{
					_remainNavigatingInterval = 0.3f;
					navigatable.Down();
				}
			}
		}
	}
}
