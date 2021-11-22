using UnityEngine;

public static class Grids
{
	public static float unit = 1.5f;
	public static Vector3 SnapToGrid(Vector3 p)
	{
		p.x = Mathf.Round(p.x / unit) * unit;
		p.y = Mathf.Round(p.y / unit) * unit;
		p.z = Mathf.Round(p.z / unit) * unit;
		return p;
	}
	public static Vector3 ConverToGrid(Vector3 p)
	{
		p.x = Mathf.Round(p.x / unit);
		p.y = Mathf.Round(p.y / unit);
		p.z = Mathf.Round(p.z / unit);
		return p;
	}
}
