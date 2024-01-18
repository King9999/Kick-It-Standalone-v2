using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//shows which way the player is facing. Appears when the player is close to a block.
public class Arrow : MonoBehaviour
{
    SpriteRenderer arrowRenderer;
    //float inverse;
    // Start is called before the first frame update
    void Start()
    {
        arrowRenderer = GetComponent<SpriteRenderer>();
        //inverse = 1;
    }

    private void OnEnable()
    {
        if (arrowRenderer == null)
        {
            arrowRenderer = GetComponent<SpriteRenderer>();
        }

        /*renderer.flipX = false;
        renderer.flipY = false;
        transform.rotation = Quaternion.identity;

        //get the player's current direction. Default arrow direction is up
        Player player = Singleton.instance.GameManager.player;
        //transform.position = new Vector2(player.transform.position.x + 0.5f, player.transform.position.y);

        if(player.facingDirection.x < 0)    //facing left
        {
            transform.Rotate(0, 0, 90);
        }
        else if (player.facingDirection.x > 0)  //facing right
        {
            transform.Rotate(0, 0, 90);
            renderer.flipY = true;      //Y is flipped because the rotation alters how X and Y behave.
        }
        else if (player.facingDirection.y < 0)  //facing down
        {
            renderer.flipY = true;
        }*/
        //if we get here, then player is facing up. We do nothing
    }

    // Update is called once per frame
    void Update()
    {
        Player player = Singleton.instance.GameManager.player;
        //transform.position = new Vector2(player.transform.position.x + (1f * inverse), player.transform.position.y);

        //---Update arrow direction. Default direction is up---
        if (player.facingDirection.x < 0)    //facing left
        {
            transform.rotation = Quaternion.identity;
            arrowRenderer.flipY = false;
            transform.Rotate(0, 0, 90);
            //inverse = 1;
        }
        else if (player.facingDirection.x > 0)  //facing right
        {
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, 0, 90);
            arrowRenderer.flipY = true;      //Y is flipped because the rotation alters how X and Y behave.
            //inverse = -1;
        }
        else if (player.facingDirection.y < 0)  //facing down
        {
            transform.rotation = Quaternion.identity;
            arrowRenderer.flipY = true;
            //inverse = 1;
        }
        else  //facing up
        {
            arrowRenderer.flipY = false;
            transform.rotation = Quaternion.identity;
            //inverse = 1;
        }
    }
}
