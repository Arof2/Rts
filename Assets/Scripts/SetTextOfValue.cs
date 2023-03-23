using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetTextOfValue : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Slider slid;
    public string playerPrefsName;

    public void Awake()
    {
        slid.SetValueWithoutNotify(Settings.instace.targetFps);
        SetValue();
    }

    public void SetValue()
    {
        text.text = Mathf.RoundToInt(slid.value).ToString();
        Settings.instace.ChangeFpsCap(Mathf.RoundToInt(slid.value));
    }
}
