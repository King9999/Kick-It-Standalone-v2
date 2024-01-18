using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//scrolls the background during gameplay. A quad is used to create the scrolling effect.
public class BackgroundScroller : MonoBehaviour
{
    public Renderer bgRenderer;
    public float scrollSpeed;


    // Update is called once per frame
    void Update()
    {
        bgRenderer.material.mainTextureOffset += new Vector2(scrollSpeed * Time.deltaTime, scrollSpeed * Time.deltaTime);
    }
}
