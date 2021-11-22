using UnityEngine;
using UnityEngine.UI;

public class ItemDesign : MonoBehaviour
{
    ControlPanel panel;
    public Text txtNum;
    public Text txtName;
    public Text txtDate;
    public string filename;
    public int index = 0;
    int no = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetData(int n, int id, string name, string date, string file, ControlPanel cp)
    {
        index = id;
        no = n;
        txtNum.text = "" + (no + 1);
        txtName.text = name;
        txtDate.text = date;
        filename = file;
        panel = cp;
    }

    public void SelectItem()
    {
        panel.SelectDesign(no);
    }
    public void Activate(bool active)
    {
        Color color = active ? Color.yellow : Color.white;
        txtNum.color = color;
        txtName.color = color;
        txtDate.color = color;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
