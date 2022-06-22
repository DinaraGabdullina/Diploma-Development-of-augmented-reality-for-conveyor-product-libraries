using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePCBTypeUI : MonoBehaviour
{
    private const KeyCode _oneFingerTapKeyCode = (KeyCode)330;
    private const KeyCode _oneFingerSwipeForwardKeyCode = (KeyCode)275;
    private const KeyCode _oneFingerSwipeBackKeyCode = (KeyCode)276;

    public ScrollRect scrollRect;
    public GameObject RawImage;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(_oneFingerTapKeyCode))
        {
            RawImage.SetActive(false);

        }
        if (Input.GetKeyDown(_oneFingerTapKeyCode))
        {
            RawImage.SetActive(false);
        }

        if (Input.GetKeyDown(_oneFingerSwipeForwardKeyCode))
        {
            scrollRect.horizontalNormalizedPosition += 320.0f;
        }

        if (Input.GetKeyDown(_oneFingerSwipeBackKeyCode))
        {
            scrollRect.horizontalNormalizedPosition -= 320.0f;
        }
    }
}
