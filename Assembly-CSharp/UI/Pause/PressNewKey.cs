using System;
using InControl;
using UnityEngine;
using UserInput;

namespace UI.Pause
{
	public class PressNewKey : MonoBehaviour
	{
		private static readonly KeyBindingSource escapeBinding = new KeyBindingSource(InControl.Key.Escape);

		private static readonly KeyBindingSource leftShiftBinding = new KeyBindingSource(InControl.Key.LeftShift);

		private static readonly KeyBindingSource rightShiftBinding = new KeyBindingSource(InControl.Key.RightShift);

		private static readonly KeyBindingSource shiftBinding = new KeyBindingSource(InControl.Key.Shift);

		private static readonly KeyBindingSource leftControlBinding = new KeyBindingSource(InControl.Key.LeftControl);

		private static readonly KeyBindingSource rightControlBinding = new KeyBindingSource(InControl.Key.RightControl);

		private static readonly KeyBindingSource controlBinding = new KeyBindingSource(InControl.Key.Control);

		private static readonly KeyBindingSource leftAltBinding = new KeyBindingSource(InControl.Key.LeftAlt);

		private static readonly KeyBindingSource rightAltBinding = new KeyBindingSource(InControl.Key.RightAlt);

		private static readonly KeyBindingSource altBinding = new KeyBindingSource(InControl.Key.Alt);

		private static readonly KeyBindingSource leftCommandBinding = new KeyBindingSource(InControl.Key.LeftCommand);

		private static readonly KeyBindingSource rightCommandBinding = new KeyBindingSource(InControl.Key.RightCommand);

		private static readonly KeyBindingSource commandBinding = new KeyBindingSource(InControl.Key.Command);

		private PlayerAction _currentAction;

		private BindingSource _oldBinding;

		private void Awake()
		{
			BindingListenOptions listenOptions = KeyMapper.Map.ListenOptions;
			listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, (Action<PlayerAction, BindingSource>)delegate(PlayerAction action, BindingSource addedBinding)
			{
				if (addedBinding == leftShiftBinding || addedBinding == rightShiftBinding)
				{
					action.ReplaceBinding(addedBinding, shiftBinding);
				}
				if (addedBinding == leftControlBinding || addedBinding == rightControlBinding)
				{
					action.ReplaceBinding(addedBinding, controlBinding);
				}
				if (addedBinding == leftAltBinding || addedBinding == rightAltBinding)
				{
					action.ReplaceBinding(addedBinding, altBinding);
				}
				if (addedBinding == leftCommandBinding || addedBinding == rightCommandBinding)
				{
					action.ReplaceBinding(addedBinding, commandBinding);
				}
			});
			BindingListenOptions listenOptions2 = KeyMapper.Map.ListenOptions;
			listenOptions2.OnBindingFound = (Func<PlayerAction, BindingSource, bool>)Delegate.Combine(listenOptions2.OnBindingFound, (Func<PlayerAction, BindingSource, bool>)delegate(PlayerAction action, BindingSource foundBinding)
			{
				DeviceBindingSource deviceBindingSource;
				if ((object)(deviceBindingSource = foundBinding as DeviceBindingSource) != null && (deviceBindingSource.Control == InputControlType.Back || deviceBindingSource.Control == InputControlType.Options))
				{
					action.StopListeningForBinding();
					base.gameObject.SetActive(false);
					return false;
				}
				if (foundBinding == escapeBinding)
				{
					action.StopListeningForBinding();
					base.gameObject.SetActive(false);
					return false;
				}
				if (foundBinding == leftShiftBinding || foundBinding == rightShiftBinding)
				{
					foundBinding = shiftBinding;
				}
				if (foundBinding == leftControlBinding || foundBinding == rightControlBinding)
				{
					foundBinding = controlBinding;
				}
				if (foundBinding == leftAltBinding || foundBinding == rightAltBinding)
				{
					foundBinding = altBinding;
				}
				if (foundBinding == leftCommandBinding || foundBinding == rightCommandBinding)
				{
					foundBinding = commandBinding;
				}
				Sprite sprite;
				if (!Resource.instance.TryGetKeyIcon(foundBinding, out sprite))
				{
					return false;
				}
				if (KeyMapper.Map.gameActions.Contains(action))
				{
					for (int i = 0; i < KeyMapper.Map.gameActions.Count; i++)
					{
						PlayerAction playerAction = KeyMapper.Map.gameActions[i];
						if (action != playerAction)
						{
							foreach (BindingSource binding in playerAction.Bindings)
							{
								if (binding == foundBinding)
								{
									BindingSource bindingSource = null;
									BindingSource oldBinding = _oldBinding;
									if ((object)oldBinding != null)
									{
										KeyBindingSource keyBindingSource;
										if ((object)(keyBindingSource = oldBinding as KeyBindingSource) == null)
										{
											MouseBindingSource mouseBindingSource;
											if ((object)(mouseBindingSource = oldBinding as MouseBindingSource) == null)
											{
												DeviceBindingSource deviceBindingSource2;
												if ((object)(deviceBindingSource2 = oldBinding as DeviceBindingSource) != null)
												{
													bindingSource = new DeviceBindingSource(deviceBindingSource2.Control);
												}
											}
											else
											{
												bindingSource = new MouseBindingSource(mouseBindingSource.Control);
											}
										}
										else
										{
											bindingSource = new KeyBindingSource(keyBindingSource.Control);
										}
									}
									if (bindingSource != null)
									{
										playerAction.ReplaceBinding(binding, bindingSource);
									}
									break;
								}
							}
						}
					}
				}
				base.gameObject.SetActive(false);
				return true;
			});
		}

		public void ListenForBinding(PlayerAction action, BindingSource binding)
		{
			base.gameObject.SetActive(true);
			_currentAction = action;
			_oldBinding = binding;
			KeyMapper.Map.SetListenOptions();
			_currentAction.ListenForBindingReplacing(_oldBinding);
		}

		private void Update()
		{
			if (!_currentAction.IsListeningForBinding)
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
