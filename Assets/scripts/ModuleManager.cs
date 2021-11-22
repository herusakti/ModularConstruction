using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Networking;
using System.Collections;
public class ModuleManager : MonoBehaviour
{
	public enum State
	{
		standby, add, clone, door
	}

	[System.Serializable]
	public class TypeSet
	{
		public string name;
		public GameObject prefab;
		public BaseModule.ModuleType type;
		public BaseModule.Varian varian;
		public Sprite sprite;
	}
	public TypeSet[] listType;
	public List<BaseModule> listModule;
	public GameObject prefab_item;
	public RectTransform rectParent;
	public Transform pos_canvas;
	int currentType = 0;
	public BaseModule currentModule;
	public BaseModule backupModule;
	Global global;

	public ControlPanel controlPanel;

	int segment_backup = -1;
	Wall wall_backup = null;

	public State state = State.standby;

	public DoorLocator locator;
	public Image imgDoor;
	public Sprite doorIcon;
	public Sprite windowIcon;


	Rect rectLeftPanel;
	public Rect rectRightPanel;

	Vector3 v_jarak = new Vector3();
	Vector3 vertical_pos = new Vector3();
	int drag_phase = 0;
	Plane plane_drag;
	float max_drag = 30;
	bool vertical_move = false;
	// Start is called before the first frame update
	void Start()
	{
		listModule = new List<BaseModule>();
		for (int i = 0; i < listType.Length; i++)
		{
			GameObject ob = GameObject.Instantiate(prefab_item, rectParent);
			ItemType klip = ob.GetComponent<ItemType>();
			klip.SetData(this, listType[i], pos_canvas);
		}
		Vector2 sz = new Vector2(130, 130);
		sz.y = (float)listType.Length * 140;
		rectParent.sizeDelta = sz;
		rectParent.localPosition = new Vector2(-15, -5f);

		StartCoroutine(LoadDesign());

		float wsc = Screen.width;
		float hsc = Screen.height;
		float scale = wsc / 1280;
		rectLeftPanel = new Rect(new Vector2(15 * scale, 15 * scale), new Vector2(150 * scale, hsc - 30 * scale));
		rectRightPanel = new Rect(new Vector2(wsc - 220 * scale, 20 * scale), new Vector2(200 * scale, 300 * scale));
	}
	void SetMovementPlane(Vector3 point, bool vertical)
	{
		if (vertical)
		{
			Vector3 arah = (Camera.main.transform.position - currentModule.transform.position).normalized;
			arah.y = 0;
			plane_drag = new Plane(arah, point);
			vertical_pos = point;
		}
		else
		{
			plane_drag = new Plane(Vector3.up, point);
		}
		vertical_move = vertical;
	}
	// Update is called once per frame
	void Update()
	{
		if (controlPanel.obDialog.active) return;
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Vector3 mousePos = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(mousePos);
			//ray from mouse position
			if (Physics.Raycast(ray, out hit))
			{
				//check if mouse is over objects
				if (hit.collider != null)
				{
					bool valid = true;
					mousePos.y = Screen.height - mousePos.y;
					BaseModule test;
					if (currentModule != null && currentModule.state != BaseModule.ModuleState.standby) valid = false;
					if (hit.transform.parent == null) valid = false;
					if (hit.transform.parent != null && !hit.transform.parent.TryGetComponent<BaseModule>(out test)) valid = false;
					if (rectLeftPanel.Contains(mousePos) || rectRightPanel.Contains(mousePos)) valid = false;
					if (valid && SelectModule(hit.transform.parent.gameObject))
					{
						if (currentModule.type == BaseModule.ModuleType.door)
						{
							bool cloneMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
							Wall wall = currentModule.transform.parent.GetComponent<Wall>();
							segment_backup = -1;
							if (cloneMode) segment_backup = wall.GetDoorSegment(currentModule);
							else segment_backup = wall.RemoveDoor(currentModule);
							if (segment_backup > -1)
							{
								if (cloneMode)
								{
									state = State.clone;
									currentModule.SetGlow(Glow.Type.none);
								}
								else
								{
									currentModule.gameObject.SetActive(false);
								}

								drag_phase = 2;
								v_jarak = currentModule.transform.position - hit.point;
								if (wall.direction == 1 || wall.direction == 3) v_jarak.x = 0;
								else v_jarak.z = 0;
								wall_backup = wall;
								imgDoor.sprite = currentModule.varian == BaseModule.Varian.window ? windowIcon : doorIcon;
							}
						}
						else
						{
							drag_phase = 1;
							v_jarak.x = currentModule.transform.position.x - hit.point.x;
							v_jarak.y = 0;
							v_jarak.z = currentModule.transform.position.z - hit.point.z;
							vertical_move = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
							SetMovementPlane(hit.point, vertical_move);
							if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) CloneModule();
						}
						controlPanel.SetModule(currentModule);
					}
				}
			}
		}
		if (Input.GetMouseButton(0))
		{
			if (drag_phase == 1)
			{
				Vector3 pos = Vector3.zero;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float enter = 0.0f;
				if (plane_drag.Raycast(ray, out enter))
				{
					if (enter < max_drag)
					{
						//Get the point that is clicked
						Vector3 hitPoint = ray.GetPoint(enter);
						bool vertical = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
						if (vertical != vertical_move)
						{
							SetMovementPlane(hitPoint, vertical);
						}
						if (vertical_move)
						{
							pos = new Vector3(vertical_pos.x, hitPoint.y, vertical_pos.z) + v_jarak;
							if (pos.y < 0) pos.y = 0;
						}
						else
						{
							pos = new Vector3(hitPoint.x, currentModule.transform.position.y, hitPoint.z) + v_jarak;
						}
						currentModule.SetPosition(pos, vertical);
						bool collided = currentModule.DetectOverlap();
						currentModule.SetGlow(collided ? Glow.Type.red : Glow.Type.yellow);
					}
				}
			}
			else
			if (drag_phase == 2)
			{
				bool onWall = false;
				Wall wall;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit))
				{
					if (hit.transform.parent != null)
					{
						if (hit.transform.parent.gameObject.TryGetComponent<Wall>(out wall))
						{
							onWall = true;
						}
					}
				}
				if (onWall)
				{
					wall = hit.transform.parent.gameObject.GetComponent<Wall>();
					locator.gameObject.SetActive(true);
					locator.SetPosition(wall, hit.point + v_jarak, currentModule.varian);

					imgDoor.gameObject.SetActive(false);
					imgDoor.color = new Color(1, 1, 1, 0);
				}
				else
				{
					locator.gameObject.SetActive(false);

					imgDoor.gameObject.SetActive(true);
					imgDoor.color = new Color(1, 1, 1, 1);
					imgDoor.rectTransform.position = Input.mousePosition;
				}
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (drag_phase == 1)
			{
				if (currentModule.DetectOverlap())
				{
					CancelDrag();
				}
				else
				{
					currentModule.SetDrop();
					if (state == State.add || state == State.clone)
					{
						listModule.Add(currentModule);
						controlPanel.SetModule(currentModule);
					}
				}
				state = State.standby;
				if (currentModule != null)
				{
					currentModule.SetGlow(Glow.Type.yellow);
					currentModule.state = BaseModule.ModuleState.standby;
				}
			}
			else
			if (drag_phase == 2)
			{
				int segment = -1;
				Wall wall;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit))
				{
					if (hit.transform.parent != null)
					{
						if (hit.transform.parent.gameObject.TryGetComponent<Wall>(out wall))
						{
							segment = locator.SetPosition(wall, hit.point, currentModule.varian);
							if (segment > -1 && !wall.listDoor[segment].available) segment = -1;
						}
					}
				}
				if (segment > -1)
				{
					wall = hit.transform.parent.gameObject.GetComponent<Wall>();
					if (state == State.clone)
					{
						wall_backup.listDoor[segment_backup].exist = true;
						AddDoor(wall, locator.transform.position, locator.transform.rotation, currentModule.gameObject, segment);
					}
					else
					{
						wall_backup.RemovePart(segment_backup);
						wall_backup.listDoor[segment_backup].module = null;
						MoveDoor(wall, locator.transform.position, locator.transform.rotation, segment);
					}
				}
				else
				{
					currentModule.gameObject.SetActive(true);
					wall_backup.SetDoor(segment_backup, currentModule);
					currentModule.SetGlow(Glow.Type.yellow);
				}
				currentModule.gameObject.SetActive(true);
				locator.gameObject.SetActive(false);
				imgDoor.gameObject.SetActive(false);
			}
			if (currentModule != null) currentModule.state = BaseModule.ModuleState.standby;
			state = State.standby;
			drag_phase = 0;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Alpha2))
		{
			RotateModule(1);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Alpha1))
		{
			RotateModule(-1);
		}
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			DeleteModule();
		}
	}
	public void RotateModule(float clockwise)
	{
		if (currentModule != null && currentModule.type != BaseModule.ModuleType.door) currentModule.Rotate(clockwise);
	}
	public void DeleteModule()
	{
		if (currentModule != null)
		{
			if (currentModule.type == BaseModule.ModuleType.door)
			{
				Wall wall = currentModule.transform.parent.GetComponent<Wall>();
				wall.RemoveDoor(currentModule);
				wall.RemovePart(wall.GetDoorSegment(currentModule));
			}
			else
			{
				listModule.Remove(currentModule);
			}
			GameObject.Destroy(currentModule.gameObject);
			SelectNone();
		}
	}
	public void CancelDrag()
	{
		if (state == State.standby)
		{
			currentModule.transform.position = currentModule.pos_grid * Grids.unit;
			currentModule.SetGlow(Glow.Type.yellow);
		}
		else
		if (state == State.door)
		{
			currentModule = backupModule;
			currentModule.SetGlow(Glow.Type.yellow);
		}
		else
		{
			GameObject.Destroy(currentModule.gameObject);
			currentModule = backupModule;
		}
		currentModule.state = BaseModule.ModuleState.standby;
	}
	bool SelectModule(GameObject ob)
	{
		BaseModule module;
		if (ob.TryGetComponent<BaseModule>(out module))
		{
			if (currentModule != null) currentModule.SetGlow(Glow.Type.none);
			currentModule = module;
			currentModule.SetGlow(Glow.Type.yellow);
			controlPanel.SetModule(currentModule);
			return true;
		}
		return false;
	}
	public void AddModule(GameObject prefab, Vector3 pos, BaseModule.ModuleType type = 0)
	{
		if (currentModule != null) currentModule.SetGlow(Glow.Type.none);
		backupModule = currentModule;

		GameObject ob = GameObject.Instantiate(prefab, pos, Quaternion.identity);
		currentModule = (BaseModule)ob.GetComponent<BaseModule>();
		currentModule.manager = this;
		state = State.add;

		SelectModule(ob);

		v_jarak.x = 0;
		v_jarak.y = 0;
		v_jarak.z = 0;
		drag_phase = 1;
		vertical_move = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		SetMovementPlane(pos, vertical_move);
	}
	public void CloneModule()
	{
		drag_phase = 0;
		currentModule.SetGlow(Glow.Type.none);
		backupModule = currentModule;

		GameObject ob = GameObject.Instantiate(currentModule.gameObject, currentModule.transform.position, currentModule.transform.rotation);
		currentModule = (BaseModule)ob.GetComponent<BaseModule>();
		currentModule.manager = this;
		state = State.add;
		SelectModule(ob);

		state = State.clone;
		currentModule.SetGlow(Glow.Type.yellow);

		backupModule.transform.position = backupModule.pos_grid * Grids.unit;
		backupModule.state = BaseModule.ModuleState.standby;
		drag_phase = 1;
	}
	public void AddDoor(Wall wall, Vector3 pos, Quaternion rot, GameObject prefabDoor, int segment)
	{
		GameObject ob = GameObject.Instantiate(prefabDoor, pos, Quaternion.identity);
		currentModule = (BaseModule)ob.GetComponent<BaseModule>();
		currentModule.manager = this;
		currentModule.transform.SetParent(wall.transform);
		currentModule.transform.position = pos;
		currentModule.transform.rotation = rot;
		currentModule.SetGlow(Glow.Type.yellow);
		currentModule.state = BaseModule.ModuleState.standby;
		wall.SetDoor(segment, currentModule);
		SelectModule(ob);
	}
	public void MoveDoor(Wall wall, Vector3 pos, Quaternion rot, int segment)
	{
		currentModule.gameObject.SetActive(true);
		currentModule.transform.SetParent(wall.transform);
		currentModule.transform.position = pos;
		currentModule.transform.rotation = rot;
		currentModule.SetGlow(Glow.Type.yellow);
		currentModule.state = BaseModule.ModuleState.standby;
		wall.SetDoor(segment, currentModule);
	}
	public void SelectNone()
	{
		if (currentModule != null)
		{
			currentModule.SetGlow(Glow.Type.none);
			backupModule = currentModule;
			currentModule = null;
			controlPanel.SetModule(null);
		}
	}
	IEnumerator LoadDesign()
	{
		yield return new WaitForSeconds(0.5f);
		GameObject obGlobal = GameObject.Find("Global");
		global = obGlobal.GetComponent<Global>();
		int id = global.idDesign;
		if (id > -1)
		{
			string filename = PlayerPrefs.GetString("file_saved_" + id, "");
			string folder = Application.persistentDataPath + "/Modular Construction";
			string path = System.IO.Path.Combine(folder, filename);
			controlPanel.id_save = id;
			UnityWebRequest www = UnityWebRequest.Get(path);
			yield return www.SendWebRequest();
			if (!www.isNetworkError)
			{
				string str = www.downloadHandler.text;
				XmlDocument xml;
				xml = new XmlDocument();
				xml.LoadXml(str);
				XmlNode root = xml.FirstChild;

				Vector3 posCam = new Vector3();
				posCam.x = float.Parse(root.Attributes.GetNamedItem("xCam").Value);
				posCam.y = float.Parse(root.Attributes.GetNamedItem("yCam").Value);
				posCam.z = float.Parse(root.Attributes.GetNamedItem("zCam").Value);

				Vector3 rotCam = new Vector3();
				rotCam.x = float.Parse(root.Attributes.GetNamedItem("rotX").Value);
				rotCam.y = float.Parse(root.Attributes.GetNamedItem("rotY").Value);
				rotCam.z = float.Parse(root.Attributes.GetNamedItem("rotZ").Value);

				Camera.main.transform.position = posCam;
				Camera.main.transform.rotation = Quaternion.Euler(rotCam);

				if (root.ChildNodes.Count > 0)
				{
					for (int i = 0; i < root.ChildNodes.Count; i++)
					{
						XmlNode node = root.ChildNodes[i];
						string str_type = node.Attributes.GetNamedItem("type").Value;
						string str_varian = node.Attributes.GetNamedItem("varian").Value;

						BaseModule.ModuleType type = (BaseModule.ModuleType)System.Enum.Parse(typeof(BaseModule.ModuleType), str_type);
						BaseModule.Varian varian = (BaseModule.Varian)System.Enum.Parse(typeof(BaseModule.Varian), str_varian);
						Vector3 posGrid = new Vector3();
						posGrid.x = float.Parse(node.Attributes.GetNamedItem("xGrid").Value);
						posGrid.y = float.Parse(node.Attributes.GetNamedItem("yGrid").Value);
						posGrid.z = float.Parse(node.Attributes.GetNamedItem("zGrid").Value);
						float unit = type == BaseModule.ModuleType.furniture ? 0.5f : 1.5f;
						Vector3 pos = posGrid * unit;

						float direction = float.Parse(node.Attributes.GetNamedItem("direction").Value);
						Quaternion rot = Quaternion.Euler(0, direction * 90, 0);
						int colorId = int.Parse(node.Attributes.GetNamedItem("colorId").Value);

						float r = float.Parse(node.Attributes.GetNamedItem("r").Value);
						float g = float.Parse(node.Attributes.GetNamedItem("g").Value);
						float b = float.Parse(node.Attributes.GetNamedItem("b").Value);
						Color color = new Color(r, g, b);

						GameObject prefab = GetPrefab(type, varian);
						if (prefab != null)
						{
							GameObject ob = Instantiate(prefab, pos, rot);
							BaseModule module = ob.GetComponent<BaseModule>();
							module.manager = this;
							module.SetColor(colorId, color);
							module.pos_grid = posGrid;
							module.direction = direction;
							module.SetGlow(Glow.Type.none);
							if (type == BaseModule.ModuleType.wall && node.ChildNodes.Count > 0)
							{
								Wall wall = ob.GetComponent<Wall>();
								yield return new WaitForSeconds(0.1f);
								for (int d = 0; d < node.ChildNodes.Count; d++)
								{
									XmlNode node_door = node.ChildNodes[d];
									string str_varian_door = node_door.Attributes.GetNamedItem("varian").Value;
									BaseModule.Varian varian_door = (BaseModule.Varian)System.Enum.Parse(typeof(BaseModule.Varian), str_varian_door);

									int segment = int.Parse(node_door.Attributes.GetNamedItem("segment").Value);
									int colorId_door = int.Parse(node_door.Attributes.GetNamedItem("colorId").Value);
									r = float.Parse(node_door.Attributes.GetNamedItem("r").Value);
									g = float.Parse(node_door.Attributes.GetNamedItem("g").Value);
									b = float.Parse(node_door.Attributes.GetNamedItem("b").Value);
									Color color_door = new Color(r, g, b);

									GameObject prefab_door = GetPrefab(BaseModule.ModuleType.door, varian_door);
									if (prefab_door != null)
									{
										GameObject ob_door = Instantiate(prefab_door, wall.transform);
										BaseModule door = ob_door.GetComponent<BaseModule>();
										door.manager = this;
										door.SetColor(colorId_door, color_door);
										door.SetGlow(Glow.Type.none);
										wall.SetDoor(segment, door);
									}
								}
							}
							listModule.Add(module);
						}
					}
				}
			}
		}
	}
	GameObject GetPrefab(BaseModule.ModuleType type, BaseModule.Varian varian)
    {
		GameObject prefab = null;
		for(int i=0; i<listType.Length && prefab == null;i++)
        {
			if (type == listType[i].type && varian == listType[i].varian) prefab = listType[i].prefab;
        }
		return prefab;
    }
}
