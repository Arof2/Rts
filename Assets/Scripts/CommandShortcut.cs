using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandShortcut : MonoBehaviour
{
    private Button butt;
    public KeyCode Shortcut;
    public string commandName;

    private void Start()
    {
        butt = GetComponent<Button>();
        gameObject.transform.Find("Shortcut").gameObject.GetComponent<TextMeshProUGUI>().text = Shortcut.ToString();
    }

    private void Update()
    {
        if(Input.GetKeyDown(Shortcut))
        {
            butt.onClick.Invoke();
        }
    }
}
