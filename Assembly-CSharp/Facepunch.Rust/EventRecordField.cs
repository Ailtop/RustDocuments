using System;
using UnityEngine;

namespace Facepunch.Rust;

public struct EventRecordField
{
	public string Key1;

	public string Key2;

	public string String;

	public long? Number;

	public double? Float;

	public Vector3? Vector;

	public Guid? Guid;

	public bool IsObject;

	public EventRecordField(string key1)
	{
		Key1 = key1;
		Key2 = null;
		String = null;
		Number = null;
		Float = null;
		Vector = null;
		Guid = null;
		IsObject = false;
	}

	public EventRecordField(string key1, string key2)
	{
		Key1 = key1;
		Key2 = key2;
		String = null;
		Number = null;
		Float = null;
		Vector = null;
		Guid = null;
		IsObject = false;
	}
}
