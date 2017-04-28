using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuButton : MenuControl
{
    #region Values

    /// <summary>
    /// Defines the path of the method to invoke upon button push.
    /// </summary>
    [Space(10)]
    public string MethodPath;

    public enum MenuButtonAction
    {
        None,
        ShowGroup,
        CloseGroup
    }
    /// <summary>
    /// Defines the action called upon button push.
    /// </summary>
    [Space(10)]
    public MenuButtonAction GroupAction;
    /// <summary>
    /// Defines the group to apply the action to.
    /// </summary>
    public MenuControlGroup ActionGroup;
    /// <summary>
    /// Determines if the parent control group of this button will be closed upon push.
    /// </summary>
    [Space(10)]
    public bool CloseParentOnPush;
	#endregion

    #region Functions
    public bool Push()
    {
        if (MethodPath != string.Empty)
        {
            string path;
            object root;
            GetPathInfo(out root, out path, MethodPath);

            if (root != null)
                ObjectReferencer.InvokeMethod(root, path);
        }

        if (ActionGroup)
        {
            switch (GroupAction)
            {
                case MenuButtonAction.ShowGroup:
                    ActionGroup.Show(Parent.Profile);
                    break;
            }
        }
        return CloseParentOnPush;
    }
    #endregion
}
