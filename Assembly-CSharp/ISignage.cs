using UnityEngine;

public interface ISignage
{
	Vector2i TextureSize { get; }

	int TextureCount { get; }

	uint NetworkID { get; }

	bool CanUpdateSign(BasePlayer player);

	float Distance(Vector3 position);

	uint[] GetTextureCRCs();

	void SetTextureCRCs(uint[] crcs);
}
