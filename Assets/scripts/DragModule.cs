using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class DragModule : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ModuleManager manager;
    public ModuleManager.TypeSet typeSet;
    public Transform parent;
    public Transform canvas;
    public Image img;
    public Sprite doorIcon;
    public Sprite windowIcon;
    bool transfered = false;
    Plane planeDrag;
    public DoorLocator locator;
    bool detecting = false;

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0, 0, 0);
        img.color = new Color(1, 1, 1, 0);
        if (typeSet.type == BaseModule.ModuleType.door)
        {
            GameObject ob = GameObject.Find("locator");
            locator = (DoorLocator)ob.GetComponent<DoorLocator>();
        }
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void SetData(ModuleManager mgr, ModuleManager.TypeSet ts, Transform cvs)
    {
        manager = mgr;
        typeSet = ts;
        parent = transform.parent;
        canvas = cvs;
        if (typeSet.type == BaseModule.ModuleType.door)
        {
            img.sprite = doorIcon;
        }
        else
        {
            img.sprite = typeSet.sprite;
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas);
        img.color = new Color(1, 1, 1, 0.5f);
        float ground = 0;
        float camPosition = Camera.main.transform.position.y;
        float grid = Grids.unit * 6;
        ground = Mathf.Floor(camPosition / grid) * grid;
        if (typeSet.type == BaseModule.ModuleType.roof) ground += grid / 3;

        Vector3 point = Vector3.zero;
        point.y = ground;
        planeDrag = new Plane(Vector3.up, point);
        transfered = false;
        if (typeSet.type == BaseModule.ModuleType.door)
        {
            manager.state = ModuleManager.State.door;
            manager.SelectNone();
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (typeSet.type == BaseModule.ModuleType.door)
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
                img.color = new Color(1, 1, 1, 0);
                locator.SetPosition(wall, hit.point, typeSet.varian);
            }
            else
            {
                locator.gameObject.SetActive(false);
                img.sprite = typeSet.varian==BaseModule.Varian.window? windowIcon : doorIcon;
                img.color = new Color(1, 1, 1, 1);
                transform.position = data.position;
            }
        }
        else
        {
            transform.position = data.position;
            if (transfered == false)
            {
                float screenWidth = Screen.width;
                if (Input.mousePosition.x > 300 * screenWidth / 1280)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float enter = 0.0f;
                    if (planeDrag.Raycast(ray, out enter))
                    {
                        if (enter < 20)
                        {
                            transfered = true;
                            img.color = new Color(1, 1, 1, 0);
                            Vector3 hitPoint = ray.GetPoint(enter);
                            manager.AddModule(typeSet.prefab, hitPoint);
                        }
                    }
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0, 0, 0);
        img.color = new Color(1, 1, 1, 0);
        if (typeSet.type == BaseModule.ModuleType.door)
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
                        if (wall.type == BaseModule.ModuleType.wall)
                        {
                            segment = locator.SetPosition(wall, hit.point, typeSet.varian);
                        }
                    }
                }
            }
            if (segment>-1)
            {
                wall = hit.transform.parent.gameObject.GetComponent<Wall>();
                manager.AddDoor(wall, locator.transform.position, locator.transform.rotation, typeSet.prefab, segment);
            }
            else
            {
                manager.CancelDrag();
            }
            locator.gameObject.SetActive(false);
        }
    }
}