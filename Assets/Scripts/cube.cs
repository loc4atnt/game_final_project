using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    private GameManager gameManager;

    public GameObject obj;
    public Material inEffect;
    Material dafaultEffect;

    public int index;

    // Start is called before the first frame update
    void Start()
    {
        GameObject gameManagerObj = GameObject.Find("GameManager");
        gameManager = gameManagerObj.GetComponent<GameManager>();

        dafaultEffect = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "hand" || other.tag == "foot")
        {
            print("INN");
            GetComponent<MeshRenderer>().material = inEffect;
            gameManager.toggleState(index, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "hand" || other.tag == "foot")
        {
            print("OUTTT");
            GetComponent<MeshRenderer>().material = dafaultEffect;
            gameManager.toggleState(index, false);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        print("stayyy");
    }
}
