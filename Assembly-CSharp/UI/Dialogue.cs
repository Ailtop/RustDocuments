using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public abstract class Dialogue : MonoBehaviour
	{
		public static readonly List<Dialogue> opened = new List<Dialogue>();

		[SerializeField]
		protected Selectable _defaultFocus;

		public static bool anyDialogueOpened => opened.Count > 0;

		public abstract bool closeWithPauseKey { get; }

		public bool focused => Focused(this);

		public static Dialogue GetCurrent()
		{
			if (opened.Count <= 0)
			{
				return null;
			}
			return opened[opened.Count - 1];
		}

		private static bool Focused(Dialogue dialogue)
		{
			if (opened.Count == 0)
			{
				return false;
			}
			return opened[opened.Count - 1] == dialogue;
		}

		public void Toggle()
		{
			if (base.gameObject.activeSelf)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		public void Open()
		{
			if (!base.gameObject.activeSelf)
			{
				opened.Add(this);
				base.gameObject.SetActive(true);
			}
		}

		protected virtual void OnEnable()
		{
			Focus();
		}

		public void Close()
		{
			if (opened.Count >= 2 && focused)
			{
				opened[opened.Count - 2].Focus();
			}
			base.gameObject.SetActive(false);
		}

		protected virtual void OnDisable()
		{
			opened.Remove(this);
		}

		public void Focus()
		{
			if (!(_defaultFocus == null))
			{
				Focus(_defaultFocus);
			}
		}

		public void Focus(Selectable focus)
		{
			StartCoroutine(CFocus(focus));
		}

		private IEnumerator CFocus(Selectable focus)
		{
			EventSystem.current.SetSelectedGameObject(null);
			yield return null;
			EventSystem.current.SetSelectedGameObject(focus.gameObject);
			focus.Select();
			typeof(Selectable).GetMethod("DoStateTransition", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(focus, new object[2] { 3, true });
		}
	}
}
