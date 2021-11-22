using UnityEngine;

public class DoorLocator : MonoBehaviour
{
    Wall wall;
    BaseModule.Varian door_type;
    public GameObject obDoor;
    public GameObject obWindow;
    public Glow glowDoor;
    public Glow glowWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public int SetPosition(Wall w, Vector3 pos, BaseModule.Varian v)
    {
        wall = w;
        door_type = v;
        obDoor.SetActive(door_type != BaseModule.Varian.window);
        obWindow.SetActive(door_type == BaseModule.Varian.window);

        Vector3 wallPos = wall.pivot.position;
        Vector3 localPos = (wall.transform.position - wallPos).normalized;
        Vector3 distance = pos - wallPos;
        float yPos = distance.y;
        bool available = false;

        int posSnap = -1;
        if (yPos > 0.25f && yPos < 2.25f)
        {
            distance.y = 0;
            float xPos = distance.magnitude;
            for (int i = 0; i < wall.listDoor.Length; i++)
            {
                if (xPos >= wall.listDoor[i].min && xPos < wall.listDoor[i].max)
                {
                    posSnap = i;
                    available = wall.listDoor[i].available;
                }
            }
        }
        if (wall.varian == BaseModule.Varian.half && posSnap > 2) posSnap = -1;
        bool isWindow = door_type == BaseModule.Varian.window;
        glowDoor.enabled=!isWindow;
        glowWindow.enabled = isWindow;
        if (posSnap > -1)
        {
            Vector3 snap = wallPos + localPos * wall.listDoor[posSnap].xPos;
            snap.y = wall.transform.position.y + (isWindow? 1.5f : 1);
            transform.position = snap;
            if (isWindow) glowWindow.GlowColor = available? wall.yellow : wall.red;
            else glowDoor.GlowColor = available ? wall.yellow : wall.red;
        }
        else
        {
            transform.position = pos;
            if (isWindow) glowWindow.GlowColor = wall.red;
            else glowDoor.GlowColor = wall.red;
        }
        transform.rotation = wall.transform.rotation;
        return posSnap;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
