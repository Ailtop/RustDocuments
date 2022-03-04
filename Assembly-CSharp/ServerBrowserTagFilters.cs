using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Rust.UI;
using UnityEngine;
using UnityEngine.Events;

public class ServerBrowserTagFilters : MonoBehaviour
{
	public RustButton Button;

	public RectTransform OptionsParent;

	public UnityEvent TagFiltersChanged = new UnityEvent();

	private ServerBrowserTag[] _allTags;

	private List<bool> _previousState;

	public void Start()
	{
		_allTags = ((OptionsParent != null) ? OptionsParent.GetComponentsInChildren<ServerBrowserTag>() : Array.Empty<ServerBrowserTag>());
		if (Button != null && Button.Text != null)
		{
			Button.Text.SetPhraseArguments(0);
		}
	}

	public void Open()
	{
		if (OptionsParent != null)
		{
			OptionsParent.SetActive(true);
		}
		_previousState = GetCurrentSelections();
	}

	public void Close()
	{
		if (OptionsParent != null)
		{
			OptionsParent.SetActive(false);
		}
		if (_previousState != null)
		{
			List<bool> currentSelections = GetCurrentSelections();
			if (!currentSelections.SequenceEqual(_previousState))
			{
				TagFiltersChanged?.Invoke();
			}
			if (Button != null && Button.Text != null)
			{
				Button.Text.SetPhraseArguments(currentSelections.Count((bool b) => b));
			}
			_previousState = null;
		}
		else
		{
			TagFiltersChanged?.Invoke();
		}
	}

	public void GetTags(out HashSet<string> searchTags, out HashSet<string> excludeTags)
	{
		searchTags = new HashSet<string>();
		excludeTags = new HashSet<string>();
		ServerBrowserTag[] allTags = _allTags;
		foreach (ServerBrowserTag serverBrowserTag in allTags)
		{
			if (serverBrowserTag.IsActive)
			{
				string[] serverHasAnyOf = serverBrowserTag.serverHasAnyOf;
				foreach (string item in serverHasAnyOf)
				{
					searchTags.Add(item);
				}
			}
		}
		allTags = _allTags;
		foreach (ServerBrowserTag serverBrowserTag2 in allTags)
		{
			if (!serverBrowserTag2.IsActive)
			{
				continue;
			}
			string[] serverHasAnyOf = serverBrowserTag2.serverHasNoneOf;
			foreach (string item2 in serverHasAnyOf)
			{
				if (!searchTags.Contains(item2))
				{
					excludeTags.Add(item2);
				}
			}
		}
	}

	private List<bool> GetCurrentSelections()
	{
		return _allTags.Select((ServerBrowserTag t) => t.IsActive).ToList();
	}
}
