using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public GameObject obSelected;
    public Image img;
    public int id = 0;
    ControlPanel panel;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetData(int i, Color color, ControlPanel cp)
    {
        id = i;
        img.color = color;
        panel = cp;
        obSelected.SetActive(false);
    }
    public void SetColor()
    {
        panel.SetColor(id);
    }
    public void SetSelected(bool selected)
    {
        obSelected.SetActive(selected);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
