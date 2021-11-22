using System.Collections;
using UnityEngine;

public class BaseModule : MonoBehaviour
{
	public enum ModuleType
	{
		floor, wall, roof, door, furniture
	}
	public enum Varian
	{
		full, half, quarter, door, window
	}
	public enum ModuleState
	{
		standby, moving, rotating
	}
	public Renderer mesh;
	public Collider collider;
	public Vector3 pos_grid;
	public ModuleType type = ModuleType.floor;
	public Varian varian = Varian.full;
	public ModuleState state = ModuleState.standby;
	public float direction = 0;
	public Glow glow;
	public GameObject obGlow;
	public ModuleManager manager;

	public Color red;
	public Color yellow;

	public int colorId = 0;
	Vector3 last_stay = new Vector3();
	// Start is called before the first frame update
	void Start()
    {
        
    }
	// Update is called once per frame
	void Update()
	{
	}
	public void SetPosition(Vector3 p, bool vertical = false)
    {
		if (vertical)
		{
			transform.position = p;
		}
		else
		{
			Grids.unit = type == ModuleType.furniture ? 0.5f : 1.5f;
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = -Input.GetAxis("Mouse Y");
			float l = (new Vector2(mouseX, mouseY)).magnitude;
			float limit = vertical ? 0.005f : 0.05f;
			if (l < limit)
			{
				transform.position = Grids.SnapToGrid(p);
				last_stay = Grids.ConverToGrid(transform.position);
			}
			else
			{
				if (last_stay != Grids.ConverToGrid(p))
				{
					transform.position = p;
				}
			}
		}
		state = ModuleState.moving;
    }	
	public void SetDrop()
    {
		transform.position = Grids.SnapToGrid(transform.position);
		pos_grid = Grids.ConverToGrid(transform.position);
		state = ModuleState.standby;
	}
	public void Rotate(float clockwise)
    {
		if (state != ModuleState.standby) return;
		StartCoroutine(CoroutineRotate(clockwise));
    }
	IEnumerator CoroutineRotate(float clockwise)
    {
		float t = 0;
		float duration = 0.25f;
		float start_angle = direction * 90;
		state = ModuleState.rotating;
		while (t < duration)
		{
			t += Time.deltaTime;
			if (t > duration) t = duration;
			float y_angle = start_angle + (t * 90 / duration) * clockwise;
			transform.localEulerAngles = new Vector3(0, y_angle, 0);
			yield return null;
		}
		direction += clockwise;
		if(DetectOverlap())
        {
			SetGlow(Glow.Type.red);
			yield return new WaitForSeconds(0.15f);
			clockwise *= -1;
			t = 0;
			duration = 0.2f;
			start_angle = direction * 90;
			while (t < duration)
			{
				t += Time.deltaTime;
				if (t > duration) t = duration;
				float y_angle = start_angle + (t * 90 / duration) * clockwise;
				transform.localEulerAngles = new Vector3(0, y_angle, 0);
				yield return null;
			}
			direction += clockwise;
			SetGlow(Glow.Type.yellow);
		}
		if (direction < 0) direction = 3;
		if (direction > 3) direction = 0;
		state = ModuleState.standby;
	}
	public void SetGlow(Glow.Type color)
    {
		if(color == Glow.Type.red)
        {
			glow.enabled = true;
			glow.GlowColor = red;
        }else
		if (color == Glow.Type.yellow)
		{
			glow.enabled = true;
			glow.GlowColor = yellow;
		}else
		{
			glow.enabled = false;
		}
		if (obGlow != null) obGlow.SetActive(color != Glow.Type.none);
	}
	public bool DetectOverlap()
	{
		bool overlap = false;
		for (int i = 0; i < manager.listModule.Count && !overlap; i++)
		{
			if (this != manager.listModule[i])
			{
				if (collider.bounds.Intersects(manager.listModule[i].collider.bounds))
				{
					overlap = true;
				}
			}
		}
		return overlap;
	}
	public virtual void SetColor(int id, Color color)
    {
		mesh.material.color = color;
		colorId = id;
    }
}
