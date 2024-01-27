using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISprite : MonoBehaviour
{
    public float aniSpeed;
    public Sprite[] sprites;
    public Image image;

    private int spriteIndex = 0;
    private bool isDone = true;
    private Coroutine coroutine;


    public async void playAnimationOnce()
    {
        isDone = false;
        StartCoroutine(Func_PlayAnimUI());
    }

    IEnumerator Func_PlayAnimUI()
    {
        yield return new WaitForSeconds(aniSpeed);
        if (spriteIndex >= sprites.Length)
        {
            spriteIndex = 0;
            isDone = true;
        }
        image.sprite = sprites[spriteIndex];
        spriteIndex += 1;
        if (isDone == false)
            coroutine = StartCoroutine(Func_PlayAnimUI());
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
