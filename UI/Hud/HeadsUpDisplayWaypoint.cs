using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplayWaypoint : MonoBehaviour
{
    #region Values

    [SerializeField]
    private Color _color;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            if (Label != null)
                Label.color = _color;
            if (Icon != null)
                Icon.color = _color;
            if (Meter != null)
                Meter.material.SetColor("_MeterColor", value);
        }
    }

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

    [SerializeField]
    private float _progress;
    public float Progress
    {
        get { return _progress; }
        set
        {
            _progress = value;
            if (Meter != null)
                Meter.material.SetFloat("_Progress", value);
        }
    }


    public Text Label;
    public MaskableGraphic Icon;
    public MaskableGraphic Meter; 
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (Meter != null)
            Destroy(Meter.material);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        Color = _color;
        Text = _text;
        if (Meter != null)
            Meter.material = new Material(Meter.material);
    }
    #endregion
}
