using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
public class ControlPanel : MonoBehaviour
{
    public enum State
    {
        standby, open, save, help
    }
    public ModuleManager manager;
    public Color[] listColor;
    public ColorPicker[] listPicker;
    public GameObject prefab;
    public Transform pickersParent;
    public GameObject btRotate1;
    public GameObject btRotate2;

    public GameObject obInspector;

    public InputField inputSaveName;

    public GameObject obDialog;
    public GameObject obOpen;
    public GameObject obSave;
    public GameObject obHelp;

    public List<ItemDesign> listDesign;
    public GameObject prefabDesign;
    public RectTransform parentDesign;
    int indexDesign = 0;
    public GameObject btLoad;

    public GameObject obResponse;
    public Text txtResponse;

    Global global;

    public int id_save = -1;
    int currentIndex =0;

    // Start is called before the first frame update
    void Start()
    {
        listPicker = new ColorPicker[listColor.Length];
        for(int i=0;i<listColor.Length;i++)
        {
            GameObject ob = GameObject.Instantiate(prefab, pickersParent);
            listPicker[i] = ob.GetComponent<ColorPicker>();
            listPicker[i].SetData(i, listColor[i], this);
            listPicker[i].SetSelected(false);
        }
        obInspector.SetActive(false);
        GameObject obGlobal = GameObject.Find("Global");
        global = obGlobal.GetComponent<Global>();
        if (PlayerPrefs.GetInt("show_info", 1) == 1)
        {
            OpenHelpDialog();
            PlayerPrefs.SetInt("show_info", 0);
            PlayerPrefs.Save();
        }
        else
        {
            CloseDialog();
        }
    }
    public void SetColor(int index)
    {
        if(manager.currentModule != null)
        {
            manager.currentModule.SetColor(index, listColor[index]);
            listPicker[currentIndex].SetSelected(false);
            currentIndex = index;
            listPicker[currentIndex].SetSelected(true);
        }
    }
    public void SetModule(BaseModule module)
    {
        listPicker[currentIndex].SetSelected(false);
        if(module==null)
        {
            obInspector.SetActive(false);
        }
        else
        {
            obInspector.SetActive(true);
            currentIndex = module.colorId;
            listPicker[currentIndex].SetSelected(true);
            btRotate1.SetActive(module.type != BaseModule.ModuleType.door);
            btRotate2.SetActive(module.type != BaseModule.ModuleType.door);
        }
    }
    public void OpenDialog(State s)
    {
        obDialog.SetActive(true);
        obSave.SetActive(s == State.save);
        obOpen.SetActive(s == State.open);
        obHelp.SetActive(s == State.help);
    }
    public void CloseDialog()
    {
        obDialog.SetActive(false);
        obSave.SetActive(false);
        obOpen.SetActive(false);
        obHelp.SetActive(false);
    }
    public void OpenOpenDialog()
    {
        if (listDesign != null)
        {
            for (int i = 0; i < listDesign.Count; i++)
            {
                GameObject.Destroy(listDesign[i].gameObject);
            }
            listDesign.Clear();
        }
        listDesign = new List<ItemDesign>();
        int total_saved = PlayerPrefs.GetInt("total_saved", 0);
        if (total_saved == 0) return;
        int n = 0;
        for(int i = total_saved-1; i>=0;i--)
        {
            GameObject ob = GameObject.Instantiate(prefabDesign, parentDesign);
            ItemDesign item = ob.GetComponent<ItemDesign>();
            string name = PlayerPrefs.GetString("name_saved_" + i, "");
            string filename = PlayerPrefs.GetString("file_saved_" + i, "");
            string folder = Application.persistentDataPath + "/Modular Construction";
            string path = System.IO.Path.Combine(folder, filename);
            DateTime creationDate = File.GetCreationTime(@path);
            string date = creationDate.ToString("dd/MM/yyyy HH:mm");
            item.SetData(n, i, name, date, filename, this);
            listDesign.Add(item);
            n++;
        }
        Vector2 sz = parentDesign.sizeDelta;
        sz.y = (float)total_saved * 42f;
        parentDesign.sizeDelta = sz;
        btLoad.SetActive(false);
        OpenDialog(State.open);
    }
    public void SelectDesign(int index)
    {
        if (indexDesign > -1)listDesign[indexDesign].Activate(false);
        listDesign[index].Activate(true);
        indexDesign = index;
        btLoad.SetActive(true);
    }
    public void LoadDesign()
    {
        if (indexDesign > -1)
        {
            global.idDesign = listDesign[indexDesign].index;
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
    public void NewDesign()
    {
        global.idDesign = -1;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    public void OpenSaveDialog()
    {
        if (manager.listModule.Count == 0) return;
        if (id_save > -1)
        {
            inputSaveName.text = PlayerPrefs.GetString("name_saved_" + id_save, "");
            if (inputSaveName.text != "")
            {
                Save();
            }
        }
        else
        {
            int total_saved = PlayerPrefs.GetInt("total_saved", 0);
            inputSaveName.text = "Design " + total_saved;
            OpenDialog(State.save);
        }
    }
    public void Save()
    {
        string name = inputSaveName.text;
        if (name == "") return;
        Vector3 posCam = Camera.main.transform.position;
        Vector3 rotCam = Camera.main.transform.rotation.eulerAngles;
        string xml = "<construction name=\"" + name + "\" xCam=\"" + posCam.x + "\" yCam=\"" + posCam.y + "\" zCam=\"" + posCam.z + "\" rotX=\"" + rotCam.x + "\" rotY=\"" + rotCam.y + "\" rotZ=\"" + rotCam.z + "\" >\n";
        for (int i = 0; i < manager.listModule.Count; i++)
        {
            BaseModule module = manager.listModule[i];

            string type = module.type.ToString();
            string varian = module.varian.ToString();

            int xGrid = (int)module.pos_grid.x;
            int yGrid = (int)module.pos_grid.y;
            int zGrid = (int)module.pos_grid.z;

            int direction = (int)module.direction;
            int colorId = module.colorId;
            Color color = module.mesh.material.color;
            string r = color.r.ToString("F3");
            string g = color.g.ToString("F3");
            string b = color.b.ToString("F3");
            xml += "    <module type=\"" + type + "\"  varian=\"" + varian + "\" xGrid=\"" + xGrid + "\"  yGrid=\"" + yGrid + "\"  zGrid=\"" + zGrid + "\"  direction= \"" + direction + "\"  colorId=\"" + colorId + "\"  r=\"" + r + "\"  g=\"" + g + "\"  b=\"" + b + "\" ";
            bool hasChild = false;
            BaseModule[] doors = null;
            if (module.type == BaseModule.ModuleType.wall)
            {
                doors = module.GetComponentsInChildren<BaseModule>();
            }
            if (doors != null && doors.Length > 0)
            {
                Wall wall = module.GetComponent<Wall>();
                xml += ">\n";
                for (int d = 0; d < doors.Length; d++)
                {
                    if (doors[d] != module)
                    {
                        BaseModule door = doors[d];
                        int segment = wall.GetDoorSegment(door);
                        string varian_door = door.varian.ToString();
                        int colorId_door = door.colorId;
                        Color color_door = door.mesh.material.color;
                        r = color_door.r.ToString("F3");
                        g = color_door.g.ToString("F3");
                        b = color_door.b.ToString("F3");

                        xml += "        <door segment=\"" + segment + "\" varian=\"" + varian_door + "\"   colorId=\"" + colorId_door + "\"  r=\"" + r + "\"  g=\"" + g + "\"  b=\"" + b + "\" />\n";
                    }
                }
                xml += "    </module>\n";
            }
            else
            {
                xml += "/>\n";
            }
        }
        xml += "</construction>";
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string filename = "MC" + timestamp + ".xml";
        string folder = Application.persistentDataPath + "/Modular Construction";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        string path = System.IO.Path.Combine(folder, filename);
        byte[] data = Encoding.ASCII.GetBytes(xml);
        File.WriteAllBytes(path, data);

        if (id_save == -1)
        {
            id_save = PlayerPrefs.GetInt("total_saved", 0);
            PlayerPrefs.SetInt("total_saved", id_save + 1);
        }
        PlayerPrefs.SetString("name_saved_"+id_save, name);
        PlayerPrefs.SetString("file_saved_" + id_save, filename);
        PlayerPrefs.Save();
        CloseDialog();
        StartCoroutine(ShowResponse(name + " has saved"));
    }
    IEnumerator ShowResponse(string response)
    {
        txtResponse.text = response;
        obResponse.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        obResponse.SetActive(false);
    }
    public void OpenHelpDialog()
    {
        OpenDialog(State.help);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
