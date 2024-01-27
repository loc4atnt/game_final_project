using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public GameObject pickMenu;
    public GameObject comboCount;
    public GameObject timmer;
    public Text scoreNum;
    public Text comboNum;
    public Text missedNum;

    public void setData(int score, int combo, int missed)
    {
        scoreNum.text = score.ToString();
        comboNum.text = combo.ToString();
        missedNum.text = missed.ToString();
    }

    public void deactive()
    {
        pickMenu.SetActive(true);
        comboCount.SetActive(false);
        timmer.SetActive(false);
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
