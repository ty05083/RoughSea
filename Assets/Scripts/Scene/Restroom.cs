using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restroom : MonoBehaviour
{
    public TMPro.TextMeshProUGUI dialogText;
    public GameObject dialogPanel;
    public GameObject targetSceneObj; // Ä¿±êGameObject³¡¾°
    // Start is called before the first frame update
    public void Init()
    {
        Hide();


    }
    public void Show()
    {
        gameObject.SetActive(true);
        Invoke("InitDialogAfterDelay", 0.1f);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    void Start()
    {
        dialogPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
