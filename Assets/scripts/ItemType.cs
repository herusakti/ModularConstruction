using UnityEngine;
using UnityEngine.UI;

public class ItemType : MonoBehaviour
{
    public DragModule drag;
    public Image img;
    public Text txt_label;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetData(ModuleManager manager, ModuleManager.TypeSet typeSet, Transform canvas)
    {
        img.sprite = typeSet.sprite;
        txt_label.text = typeSet.name;
        drag.SetData(manager, typeSet, canvas);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
