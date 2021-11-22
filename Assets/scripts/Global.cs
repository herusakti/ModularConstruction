using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public int idDesign = -1;
    private static Global globalInstance;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
