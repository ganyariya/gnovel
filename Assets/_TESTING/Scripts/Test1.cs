using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    [SerializeField]
    private Test2 test2;

    [SerializeField]
    private Test3 test3;

    void Awake()
    {
        this.test2 = new Test2("hello", 18);
        this.test3.Test3Name = "test1 to test3 dayo";
        Debug.Log("Awake Test1");
    }

    // Start is called before the first frame update
    void Start()
    {
        var transform = this.gameObject.GetComponent<UnityEngine.Transform>();
        transform.localPosition = new Vector3(10, 10, 10);
        Debug.Log("Start Test1");
        Debug.Log(this.test2.Age);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            StartCoroutine("HelloIndex");
        }
    }

    IEnumerator HelloIndex()
    {
        for (int i = 0; i < 100; i++)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1);
        }
    }
}
