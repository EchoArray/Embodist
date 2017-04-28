using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuProfile : MenuControl
{

    public Text NameLabel;

    public void SetName(string name)
    {
        NameLabel.text = name;
    }
}
