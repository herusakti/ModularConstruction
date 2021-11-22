using UnityEngine;

public class Wall : BaseModule
{
    public enum DoorType
    {
        door, window
    }
    public enum SnapMode
    {
        free, dual, triple
    }
    [System.Serializable]
    public class WallPart
    {
        public GameObject door;
        public GameObject window;
    }
    public GameObject blankLeft;
    public GameObject blankRight;
    public WallPart[] swSingle;
    public WallPart[] swDouble;

    [System.Serializable]
    public class DoorSet
    {
        public float xPos;
        public float min;
        public float max;
        public bool exist = false;
        public bool available = true;
        public DoorType type = DoorType.door;
        public BaseModule module;
    }
    public DoorSet[] listDoor;
    public Transform pivot;
    // Start is called before the first frame update
    void Start()
    {
        listDoor = new DoorSet[7];
        for (int i = 0; i < 7; i++) listDoor[i] = new DoorSet();

        listDoor[0].min = 0;
        listDoor[0].xPos = 0.5f;
        listDoor[0].max = 0.6f;

        listDoor[1].min = 0.6f;
        listDoor[1].xPos = 0.75f;
        listDoor[1].max = 1.9f;

        listDoor[2].min = 0.9f;
        listDoor[2].xPos = 1;
        listDoor[2].max = 1.3f;

        listDoor[3].min = 1.3f;
        listDoor[3].xPos = 1.5f;
        listDoor[3].max = 1.7f;

        listDoor[4].min = 1.7f;
        listDoor[4].xPos = 2;
        listDoor[4].max = 2.1f;

        listDoor[5].min = 2.1f;
        listDoor[5].xPos = 2.25f;
        listDoor[5].max = 2.4f;

        listDoor[6].min = 2.4f;
        listDoor[6].xPos = 2.5f;
        listDoor[6].max = 3;
    }
    public void SetDoor(int index, BaseModule module)
    {
        if (index == 3 && varian == Varian.full)
        {
            listDoor[1].available = false;
            listDoor[2].available = false;
            listDoor[3].available = false;
            listDoor[4].available = false;
            listDoor[5].available = false;

            listDoor[index].available = false;
            listDoor[index].module = module;
        }
        else
        {
            if (index < 3)
            {
                listDoor[0].available = false;
                listDoor[1].available = false;
                listDoor[2].available = false;
                if(index>0) listDoor[3].available = false;

                listDoor[0].exist = false;
                listDoor[1].exist = false;
                listDoor[2].exist = false;
            }
            if (index > 3)
            {
                listDoor[4].available = false;
                listDoor[5].available = false;
                listDoor[6].available = false;
                if (index < 6) listDoor[3].available = false;

                listDoor[4].exist = false;
                listDoor[5].exist = false;
                listDoor[6].exist = false;
            }

            listDoor[index].available = false;
            listDoor[index].module = module;
            if (listDoor[1].exist || listDoor[2].exist || listDoor[4].exist || listDoor[5].exist || varian == Varian.half)
            {
                listDoor[3].available = false;
                listDoor[3].exist = false;
            }
        }
        for (int i = 0; i < listDoor.Length; i++)
        {
            if (i < 3 && listDoor[i].exist)
            {
                listDoor[0].available = false;
                listDoor[1].available = false;
                listDoor[2].available = false;
            }
            if (i > 3 && listDoor[i].exist)
            {
                listDoor[4].available = false;
                listDoor[5].available = false;
                listDoor[6].available = false;
            }
            if (i == 3 && listDoor[3].exist)
            {
                listDoor[1].available = false;
                listDoor[2].available = false;

                listDoor[3].available = false;

                listDoor[4].available = false;
                listDoor[5].available = false;
            }
        }
        if (index > -1)
        {
            Vector3 wallPos = pivot.position;
            Vector3 localPos = (transform.position - wallPos).normalized;
            Vector3 snap = wallPos + localPos * listDoor[index].xPos;
            snap.y = transform.position.y + (module.varian == BaseModule.Varian.window ? 1.5f : 1);
            module.transform.position = snap;
            bool isWindow = module.varian == BaseModule.Varian.window; 
            if(listDoor[3].exist)
            {
                int indexType = listDoor[3].module.varian == BaseModule.Varian.window ? 1 : 0;
                if (index == 0)
                {
                    ClearWallpart(0);
                    if (isWindow) swDouble[indexType].window.SetActive(true);
                    else swDouble[indexType].door.SetActive(true);
                }
                if (index == 6)
                {
                    indexType = listDoor[3].module.varian == BaseModule.Varian.window ? 5 : 6;
                    ClearWallpart(1);
                    if (isWindow) swDouble[indexType].window.SetActive(true);
                    else swDouble[indexType].door.SetActive(true);
                }
            }else
            {
                if (index < 3)
                {
                    ClearWallpart(0);
                    if (isWindow) swSingle[index].window.SetActive(true);
                    else swSingle[index].door.SetActive(true);
                }
                if (index > 3)
                {
                    ClearWallpart(1);
                    if (isWindow) swSingle[index].window.SetActive(true);
                    else swSingle[index].door.SetActive(true);
                }
                if (index == 3)
                {
                    ClearWallpart(0);
                    ClearWallpart(1);
                    if (listDoor[0].exist)
                    {
                        int indexType = isWindow ? 1 : 0;
                        if (listDoor[0].module.varian == BaseModule.Varian.window) swDouble[indexType].window.SetActive(true);
                        else swDouble[indexType].door.SetActive(true);
                    }else
                    {
                        if (isWindow) swSingle[3].window.SetActive(true);
                        else swSingle[3].window.SetActive(false);
                    }
                    if (listDoor[6].exist)
                    {
                        int indexType = isWindow ? 5 : 6;
                        if (listDoor[6].module.varian == BaseModule.Varian.window) swDouble[indexType].window.SetActive(true);
                        else swDouble[indexType].door.SetActive(true);
                    }else
                    {
                        if (isWindow) swDouble[3].window.SetActive(true);
                        else swDouble[3].door.SetActive(true);
                    }
                    if (listDoor[0].exist==false && listDoor[6].exist == false)
                    {
                        int indexType = isWindow ? 1 : 0;
                        if (isWindow) swSingle[3].window.SetActive(true);
                        else swSingle[3].door.SetActive(true);

                        if (isWindow) swDouble[3].window.SetActive(true);
                        else swDouble[3].door.SetActive(true);
                    }
                }
            }
            listDoor[index].exist = true;
            SetVisible(false);
            CheckBlank();
        }
    }
    void SetVisible(bool visible)
    {
        mesh.enabled = visible;
    }
    void ClearWallpart(int side)
    {
        if(side == 1)
        {
            for (int i = 4; i <=6; i++)
            {
                swSingle[i].door.SetActive(false);
                swSingle[i].window.SetActive(false);
            }
            swDouble[3].door.SetActive(false);
            swDouble[3].window.SetActive(false);
            swDouble[5].door.SetActive(false);
            swDouble[5].window.SetActive(false);
            swDouble[6].door.SetActive(false);
            swDouble[6].window.SetActive(false);
        }
        else
        {
            for (int i=0;i<=3;i++)
            {
                swSingle[i].door.SetActive(false);
                swSingle[i].window.SetActive(false);
            }
            swDouble[0].door.SetActive(false);
            swDouble[0].window.SetActive(false);
            swDouble[1].door.SetActive(false);
            swDouble[1].window.SetActive(false);
        }
    }
    public void RemovePart(int index)
    {
        bool isWindow = listDoor[index].module.varian == BaseModule.Varian.window;
        if (index < 3)
        {
            ClearWallpart(0);
            if (listDoor[3].exist)
            {
                if(isWindow)swSingle[3].window.SetActive(true);
                else swSingle[3].door.SetActive(true);
            }
        }

        if (index > 3)
        {
            ClearWallpart(1);
            if (listDoor[3].exist)
            {
                if (isWindow) swDouble[3].window.SetActive(true);
                else swDouble[3].door.SetActive(true);
            }
        }
        if (index == 3)
        {
            ClearWallpart(0);
            ClearWallpart(1);
            if (listDoor[0].exist)
            {
                if (listDoor[0].module.varian == BaseModule.Varian.window) swSingle[0].window.SetActive(true);
                else swSingle[0].door.SetActive(true);
            }else blankLeft.SetActive(true);
            if (listDoor[6].exist)
            {
                if (listDoor[6].module.varian == BaseModule.Varian.window) swSingle[6].window.SetActive(true);
                else swSingle[6].door.SetActive(true);
            }else blankRight.SetActive(true);
            if (listDoor[0].exist == false && listDoor[6].exist == false)
            {
                blankLeft.SetActive(false);
                blankRight.SetActive(false);
                SetVisible(true);
            }
        }
        CheckBlank();
    }
    public void CheckBlank()
    {
        bool leftBlank = true;
        for(int i=0;i<=3;i++)
        {
            if (listDoor[i].exist) leftBlank = false;
        }
        bool rightBlank = true;
        for (int i = 3; i <= 6; i++)
        {
            if (listDoor[i].exist) rightBlank = false;
        }
        if(leftBlank) ClearWallpart(0);
        blankLeft.SetActive(leftBlank);
        if (rightBlank) ClearWallpart(1);
        blankRight.SetActive(rightBlank);

        if(leftBlank && rightBlank)
        {
            blankLeft.SetActive(false);
            blankRight.SetActive(false);
            SetVisible(true);
        }
    }
    public int RemoveDoor(BaseModule module)
    {
        int index = GetDoorSegment(module);
        if(index>-1)
        {
            listDoor[index].exist = false;
            if (index == 0)
            {
                listDoor[0].available = true;
                listDoor[1].available = !listDoor[3].exist;
                listDoor[2].available = !listDoor[3].exist;

                listDoor[0].exist = false;
                listDoor[1].exist = false;
                listDoor[2].exist = false;
            }
            if (index == 1 || index == 2)
            {
                listDoor[0].available = true;
                listDoor[1].available = true;
                listDoor[2].available = true;
                if(listDoor[4].exist==false && listDoor[5].exist == false)listDoor[3].available = true;

                listDoor[0].exist = false;
                listDoor[1].exist = false;
                listDoor[2].exist = false;
                listDoor[3].exist = false;
            }
            if (index == 4 || index == 5)
            {
                listDoor[4].available = true;
                listDoor[5].available = true;
                listDoor[6].available = true;
                if(listDoor[1].exist == false && listDoor[2].exist == false)listDoor[3].available = true;

                listDoor[3].exist = false;
                listDoor[4].exist = false;
                listDoor[5].exist = false;
                listDoor[6].exist = false;
            }
            if (index == 6)
            {
                listDoor[4].available = !listDoor[3].exist;
                listDoor[5].available = !listDoor[3].exist;
                listDoor[6].available = true;

                listDoor[4].exist = false;
                listDoor[5].exist = false;
                listDoor[6].exist = false;
            }
            if (index==3 && varian == Varian.full)//triple
            {
                listDoor[1].available = !listDoor[0].exist;
                listDoor[2].available = !listDoor[0].exist;
                listDoor[4].available = !listDoor[6].exist;
                listDoor[5].available = !listDoor[6].exist;

                listDoor[3].available = true;

                listDoor[1].exist = false;
                listDoor[2].exist = false;
                listDoor[3].exist = false;
                listDoor[4].exist = false;
                listDoor[5].exist = false;
            }
        }
        return index;
    }
    public int GetDoorSegment(BaseModule module)
    {
        int index = -1;
        for (int i = 0; i < listDoor.Length; i++)
        {
            if (module == listDoor[i].module) index = i;
        }
        if(index == -1)
        {
            Vector3 wallPos = pivot.position;
            Vector3 distance = module.transform.position - wallPos;
            float yPos = distance.y;
            if (yPos > 0.25f && yPos < 2.25f)
            {
                distance.y = 0;
                float xPos = distance.magnitude;
                for (int i = 0; i < listDoor.Length; i++)
                {
                    if ((xPos > listDoor[i].min) && (xPos < listDoor[i].max)) index = i;
                }
                if (varian == Varian.half && index > 2) index = -1;
                if (index > -1)
                {
                    SetDoor(index, module);
                    listDoor[index].module = module;
                }
            }
        }
        return index;
    }
    public override void SetColor(int id, Color color)
    {
        for (int i = 0; i < swSingle.Length; i++)
        {
            MeshRenderer renderer;
            if (swSingle[i].door.TryGetComponent<MeshRenderer>(out renderer))
            {
                renderer.material.color = color;
            }
            if (swSingle[i].window.TryGetComponent<MeshRenderer>(out renderer))
            {
                renderer.material.color = color;
            }
        }
        blankLeft.GetComponent<MeshRenderer>().material.color = color;
        if(varian == Varian.full)
        {
            blankRight.GetComponent<MeshRenderer>().material.color = color;
            for (int i = 0; i < swDouble.Length; i++)
            {
                MeshRenderer renderer;
                if (swDouble[i].door.TryGetComponent<MeshRenderer>(out renderer))
                {
                    renderer.material.color = color;
                }
                if (swDouble[i].window.TryGetComponent<MeshRenderer>(out renderer))
                {
                    renderer.material.color = color;
                }
            }
        }
        mesh.material.color = color;
        colorId = id;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
