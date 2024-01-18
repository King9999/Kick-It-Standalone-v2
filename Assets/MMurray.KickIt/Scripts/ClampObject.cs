using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*this script allows mouse actions on objects by using a UI image. Note that this will result in each object having its own canvas,
which may or may not impact performance if you have a lot of objects. */
public class ClampObject : MonoBehaviour
{
   public Image objectHitbox;             //allows mouse actions to occur when mouse hovers over object.

    void Start()
    {       
        Vector3 imgPos = Camera.main.WorldToScreenPoint(transform.position);   //attach to an avatar object

        //image must be enabled for mouse hover to work but alpha is reduced to 0
        objectHitbox.transform.position = imgPos;
        //objectHitbox.transform.localScale = new Vector3(0.5f, 0.5f, 0);   //scale the hitbox as necessary
        objectHitbox.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        //need this bit of code so that the hitboxes are repositioned correctly when the camera or object moves.
        Clamp(transform.position);
    }

    public void Clamp(Vector3 position)
    {
        Vector3 imgPos = Camera.main.WorldToScreenPoint(position); 
        objectHitbox.transform.position = imgPos;
    }
}
