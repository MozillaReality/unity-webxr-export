using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebXR;

public class ControllerDebugDetailView : MonoBehaviour
{
    [SerializeField]
    private Text id;
    [SerializeField]
    private Text index;
    [SerializeField]
    private Text hand;
    [SerializeField]
    private Transform buttonContainer;

    public GameObject buttonPrefab;
    
    private List<ControllerDebugButtonView> _buttons = new List<ControllerDebugButtonView>();
    
    public string ID
    {
        set { id.text = value; }
    }

    public int Index
    {
        set { index.text = value.ToString(); }
    }

    public string Hand
    {
        set { hand.text = value; }
    }

    public void ApplyButtons(IList<WebXRControllerButton> buttons)
    {
        if (_buttons.Count < buttons.Count)
        {
            for (var i = _buttons.Count; i < buttons.Count; ++i)
            {
                var go = Instantiate(buttonPrefab, buttonContainer, false);
                _buttons.Add(go.GetComponent<ControllerDebugButtonView>());
            }
        }

        for (var i = 0; i < buttons.Count; ++i)
        {
            var b = _buttons[i];
            b.Index = i;
            b.Pressed = buttons[i].pressed;
            b.Value = buttons[i].value;
        }
    }
}
