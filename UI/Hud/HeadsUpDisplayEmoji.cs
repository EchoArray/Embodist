using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplayEmoji : MonoBehaviour
{
    #region Values
    [SerializeField]
    private string _text;
    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            if (Label != null)
                Label.text = _text;
        }
    }
    public Text Label;
    #endregion
}
