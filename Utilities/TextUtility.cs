using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUtility : MonoBehaviour
{
    private Text _text;

    public bool RandomizeOnEnable;
    public string[] RandomStrings;



	private void Awake ()
    {
        _text = this.gameObject.GetComponent<Text>();

        if (!RandomizeOnEnable)
            Initialize();
	}
    
    public void OnEnable()
    {
        if(RandomizeOnEnable)
            Initialize();
    }

    private void Initialize()
    {
        if (_text != null)
        {
            _text.text = RandomStrings[Random.Range(0, RandomStrings.Length)];
        }
    }
}
