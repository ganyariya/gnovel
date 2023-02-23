using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3 : MonoBehaviour
{
    public string Test3Name;

    void Awake()
    {
        this.Test3Name = "test3";
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Test3" + this.Test3Name);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
