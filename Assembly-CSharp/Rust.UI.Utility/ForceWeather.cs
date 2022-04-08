using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.Utility;

[RequireComponent(typeof(Toggle))]
internal class ForceWeather : MonoBehaviour
{
	private Toggle component;

	public bool Rain;

	public bool Fog;

	public bool Wind;

	public bool Clouds;

	public void OnEnable()
	{
		component = GetComponent<Toggle>();
	}

	public void Update()
	{
		if (!(SingletonComponent<Climate>.Instance == null))
		{
			if (Rain)
			{
				SingletonComponent<Climate>.Instance.Overrides.Rain = Mathf.MoveTowards(SingletonComponent<Climate>.Instance.Overrides.Rain, component.isOn ? 1 : 0, Time.deltaTime / 2f);
			}
			if (Fog)
			{
				SingletonComponent<Climate>.Instance.Overrides.Fog = Mathf.MoveTowards(SingletonComponent<Climate>.Instance.Overrides.Fog, component.isOn ? 1 : 0, Time.deltaTime / 2f);
			}
			if (Wind)
			{
				SingletonComponent<Climate>.Instance.Overrides.Wind = Mathf.MoveTowards(SingletonComponent<Climate>.Instance.Overrides.Wind, component.isOn ? 1 : 0, Time.deltaTime / 2f);
			}
			if (Clouds)
			{
				SingletonComponent<Climate>.Instance.Overrides.Clouds = Mathf.MoveTowards(SingletonComponent<Climate>.Instance.Overrides.Clouds, component.isOn ? 1 : 0, Time.deltaTime / 2f);
			}
		}
	}
}
