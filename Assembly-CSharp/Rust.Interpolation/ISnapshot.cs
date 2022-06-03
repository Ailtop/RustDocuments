namespace Rust.Interpolation;

public interface ISnapshot<T>
{
	float Time { get; set; }

	void MatchValuesTo(T entry);

	void Lerp(T prev, T next, float delta);

	T GetNew();
}
