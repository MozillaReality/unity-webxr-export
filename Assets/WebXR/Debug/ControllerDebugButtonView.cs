using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ControllerDebugButtonView : MonoBehaviour
{
    [SerializeField]
    private Text index;
    [SerializeField]
    private Toggle pressed;
    [SerializeField]
    private Text value;

    public int Index
    {
        set { index.text = value.ToString(); }
    }

    public bool Pressed
    {
        set { pressed.isOn = value; }
    }

    public float Value
    {
        set { this.value.text = value.ToString(CultureInfo.InvariantCulture); }
    }
}
