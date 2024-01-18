using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using LoLSDK;

//provides persistent information to the player
public class Sidebar : MonoBehaviour
{
    public Button sidebarButton;                //opens and closes the sidebar
    public List<TextMeshProUGUI> sidebarText;   //contains keys from language JSON
    public List<Image> sidebarImageList;
    public List<string> sidebarKeys;            //count must match sidebarText list count.
    public bool sidebarOpen;
    bool coroutineOn;
    //public RectTransform sidebarAnchor, onscreenAnchor, offscreenAnchor;      //used to determine where sidebar should end up when travelling.

    // Start is called before the first frame update
    void Start()
    {
        //sidebar adjustment
        /*if (!sidebarOpen)
        {
            float moveSpeed = 128;
            if (sidebarButton.transform.position.x > onscreenAnchor.position.x)
            {
                do
                {
                    Vector3 currentPos = transform.position;
                    transform.position = new Vector3(currentPos.x - moveSpeed * Time.deltaTime, currentPos.y, currentPos.z);
                }
                while (sidebarButton.transform.position.x > onscreenAnchor.position.x);
            }
            else
            {
                do
                {
                    Vector3 currentPos = transform.position;
                    transform.position = new Vector3(currentPos.x + moveSpeed * Time.deltaTime, currentPos.y, currentPos.z);
                }
                while (sidebarButton.transform.position.x < onscreenAnchor.position.x);
            }
        }*/

        //populate text
        for (int i = 0; i < sidebarKeys.Count; i++) 
        {
            sidebarText[i].text = Singleton.instance.GetText(sidebarKeys[i]);
        }

        //gameObject.SetActive(false);
    }

    public void OnSidebarButtonPressed()
    {
        if (coroutineOn)
            return;
        
        sidebarOpen = !sidebarOpen;
        //StartCoroutine(ActivateSidebar(sidebarOpen));

        gameObject.SetActive(sidebarOpen); 
    }

    /*public void ReadSidebar()
    {
        StartCoroutine(ReadSidebarText());
    }*/


    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    /*IEnumerator ReadSidebarText()
    {
        if (sidebarKeys.Count == 1)
        {
            LOLSDK.Instance.SpeakText(sidebarKeys[0]);
        }
        else
        {
            foreach (string key in sidebarKeys)
            {
                LOLSDK.Instance.SpeakText(key);
                yield return new WaitForSeconds(5);
            }
        }
    }*/

    /*IEnumerator ActivateSidebar(bool sidebarOpen)
    {
        coroutineOn = true;
        float moveSpeed = 1024;
        if (sidebarOpen == false)
        {
            //close sidebar. It moves to the right until offscreen. The button lines up with the onscreen anchor.
            while (sidebarButton.transform.position.x < onscreenAnchor.position.x)
            {
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x + moveSpeed * Time.deltaTime, currentPos.y, currentPos.z);
                yield return null;
            }
        }
        else
        {
            //open sidebar. It moves to the left a certain distance.
            while (sidebarAnchor.position.x > onscreenAnchor.position.x)
            {
                //move the sidebar
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x - moveSpeed * Time.deltaTime, currentPos.y, currentPos.z);
                yield return null;
            }
            //close all other open sidebars.
        }
        //yield return null;


        coroutineOn = false;
    }*/
}
