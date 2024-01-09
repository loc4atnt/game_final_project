using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    public GameObject obj;
    public Material inEffect;
    Material dafaultEffect;
    // Start is called before the first frame update
    void Start()
    {
        dafaultEffect = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print("INN");
        GetComponent<MeshRenderer>().material = inEffect;
    }

    private void OnTriggerExit(Collider other)
    {
        print("OUTTT");
        GetComponent<MeshRenderer>().material = dafaultEffect;
    }

    private void OnCollisionStay(Collision collision)
    {
        print("stayyy");
    }
}
