using UnityEngine;
using System.Collections;
using System;

public class MenuControlGroup : MonoBehaviour
{

    #region Values
    public AudioSourceUtility Music;


    // Defines the inputting profile
    [HideInInspector]
    public GameManager.GameAspects.Profile Profile;

    public bool DisabledForClient;

    public MenuControlGroup ClosingActiveGroup;

    /// <summary>
    /// Determines if the start menu is allowed to be shown while this control group is active.
    /// </summary>
    public bool AllowStartMenu;
    /// <summary>
    /// Determines if the profile that showed this control group is the only profile allowed to input.
    /// </summary>
    public bool ProfileSpecific;
    /// <summary>
    /// Defines the duration in-which input is stunned after this control group has been shown.
    /// </summary>
    public float AllowInputDelay;
    /// <summary>
    /// The time in-which the control group is to allow input.
    /// </summary>
    [HideInInspector]
    public float AllowInputTime;
    public bool IgnoreScrollToEndInput;
    /// <summary>
    /// Determines if game input is halted when this control group is active.
    /// </summary>
    public bool DisableAllGameInput;

    public bool ResetSelectionOnShow;

    /// <summary>
    /// Determines if this game object is immediately deactivated upon close.
    /// </summary>
    public bool DeactivateOnClose;
    /// <summary>
    /// Determines if the active group of the menu controller is to be set to null upon the closing of this control group.
    /// </summary>
    public bool NullActiveGroupOnClose;
    [Serializable]
    public class AnimationCall
    {
        /// <summary>
        /// Defines the animation set.
        /// </summary>
        public UIAnimationSet AnimationSet;
        /// <summary>
        /// Defines the animation called in the above set.
        /// </summary>
        public string AnimationName;
    }
    /// <summary>
    /// A collection of animations that are to be played upon the closing of this control group.
    /// </summary>
    public AnimationCall[] ClosingAnimationCalls;

    /// <summary>
    /// Defines the selected control set.
    /// </summary>
    public int SelectedSet;
    [Serializable]
    public class ControlSet
    {
        /// <summary>
        /// Defines the name of the control set.
        /// </summary>
        public string Name;
        /// <summary>
        /// Defines the set upward from this set.
        /// </summary>
        public int UpSetIndex = -1;
        /// <summary>
        /// Defines the set downward from this set.
        /// </summary>
        public int DownSetIndex = -1;
        /// <summary>
        /// Defines the set left of this set.
        /// </summary>
        public int LeftSetIndex = -1;
        /// <summary>
        /// Defines the set right of this set.
        /// </summary>
        public int RightSetIndex = -1;

        /// <summary>
        /// Defines the selected control of the set.
        /// </summary>
        public int SelectedControlIndex;

        /// <summary>
        /// Defines the controls of the set.
        /// </summary>
        public MenuControl[] MenuControls;
    }
    public ControlSet[] ControlSets;
    private MenuControl _selectedControl;

    [Serializable]
    public class ButtonAction
    {
        /// <summary>
        /// Determines if this control group will be close on this button action.
        /// </summary>
        public bool CloseThis;
        /// <summary>
        /// Determines if the button action allows non-registered profiles to input.
        /// </summary>
        public bool DoesntRequireRegisteredPlayer;
        /// <summary>
        /// Determines if the controller button used for this button action will be ignored for the rest of this frame.
        /// </summary>
        public bool IgnoreButtonThisFrame;

        /// <summary>
        /// Defines the controller button that will invoke the actions.
        /// </summary>
        public MenuController.InputButton Button;

        /// <summary>
        /// Defines the path of the method to invoke upon button input.
        /// </summary>
        public string MethodPath;

        public enum Actions
        {
            None,
            ShowActionGroup,
            CloseActionGroup,
        }
        /// <summary>
        /// Defines the action called upon button input.
        /// </summary>
        [Space(10)]
        public Actions GroupAction;
        /// <summary>
        /// Defines the group to apply the action to.
        /// </summary>
        public MenuControlGroup ActionGroup;
    }
    public ButtonAction[] ButtonActions;

    private bool _open;
    private int _initialSelectedSet;
    private int _initialSelectedControl;

    private bool _initialized;


    #endregion

    #region Unity Functions
    private void Awake()
    {
        if (ControlSets.Length > 0)
        {
            _initialSelectedSet = SelectedSet;
            _initialSelectedControl = ControlSets[SelectedSet].SelectedControlIndex;
            _initialized = true;
        }
    }
    #endregion

    #region Functions
    public void Show(GameManager.GameAspects.Profile profile, bool closeActiveGroup = false)
    {
        if(!_initialized)
            Awake();

        if (Music != null)
            Music.Play();

        _open = true;
        // Define profile
        Profile = profile;

        // Set active group as this, and show this game object
        MenuController.Instance.SetActiveGroup(this, closeActiveGroup);
        this.gameObject.SetActive(true);


        foreach (AnimationCall animationCall in ClosingAnimationCalls)
        {
            animationCall.AnimationSet.StopAll();
            animationCall.AnimationSet.PlayOnEnable();
        }


        // Delay input
        AllowInputTime = Time.time + AllowInputDelay;

        // Disallow in-game player input
        if (DisableAllGameInput)
            GameManager.Instance.Game.AllowInput = false;

        LoadAllSettings();


        if (DisabledForClient && NetworkSessionManager.IsClient)
            return;

        if (ResetSelectionOnShow)
        {
            SelectControl(ControlSets[SelectedSet].SelectedControlIndex, false);

            SelectedSet = _initialSelectedSet;
            ControlSets[SelectedSet].SelectedControlIndex = _initialSelectedControl;
        }
        if (ControlSets.Length > 0)
        {
            SelectControl(ControlSets[SelectedSet].SelectedControlIndex);
        }

    }
    public void LoadAllSettings()
    {
        // Initialize setting controls
        foreach (ControlSet controlSet in ControlSets)
            foreach (MenuControl menuControl in controlSet.MenuControls)
                menuControl.Load(this);
    }
    public void Close(bool closeNextGroup = false)
    {
        if (!_open)
            return;
        if(Music != null)
            Music.Stop();

        _open = false;
        // Re-allow player input
        if (DisableAllGameInput)
            GameManager.Instance.Game.AllowInput = true;

        // Null active group
        if (NullActiveGroupOnClose)
            MenuController.Instance.SetActiveGroup(null, false);
        else
        {
            if (closeNextGroup)
                ClosingActiveGroup.Close(true);
            else
                MenuController.Instance.SetActiveGroup(ClosingActiveGroup);
        }


        // Set game object state, show closing animations
        this.gameObject.SetActive(!DeactivateOnClose);
        if (!DeactivateOnClose)
            foreach (AnimationCall animationCall in ClosingAnimationCalls)
            {
                animationCall.AnimationSet.StopAll();
                animationCall.AnimationSet.Play(animationCall.AnimationName, true);
            }
    }

    public bool ScrollRow(bool forward, GameManager.GameAspects.Profile profile)
    {
        // Returns if the selection was advanced

        // If the inputting profile not the active profile and the control is player specific, return false
        if (ProfileSpecific && profile != Profile)
            return false;

        Profile = profile;

        // If there are no controls, return false
        if (ControlSets.Length == 0)
            return false;

        // Deselect the previously selected control
        SelectControl(ControlSets[SelectedSet].SelectedControlIndex, false);

        if (forward)
        {
            // Scroll down
            bool atListBottom = ControlSets[SelectedSet].SelectedControlIndex == ControlSets[SelectedSet].MenuControls.Length - 1;
            if (atListBottom && ControlSets[SelectedSet].DownSetIndex != -1)
            {
                // If the selection is at the bottom of the set, is to be advanced, and there is another set to select, select it
                SelectedSet = ControlSets[SelectedSet].DownSetIndex;
            }
            else if (atListBottom)
            {
                // If the selection is at the bottom of the set, there is no other set to select, and the index is to be advanced,
                // Select the first control of the active set
                ControlSets[SelectedSet].SelectedControlIndex = 0;
            }
            else
            {
                // If the selection is to be advanced, advance
                ControlSets[SelectedSet].SelectedControlIndex += 1;
            }
        }
        else
        {
            // Scroll up
            bool atListTop = ControlSets[SelectedSet].SelectedControlIndex == 0;
            if (atListTop && ControlSets[SelectedSet].UpSetIndex != -1)
            {
                // If the selection is at the bottom of the set, is to be advanced, and there is another set to select, select it
                SelectedSet = ControlSets[SelectedSet].UpSetIndex;
            }
            else if (atListTop)
            {
                // If the selection is at the top of the set, there is no other set to select, and the index is to be advanced,
                // Select the first control of the active set
                ControlSets[SelectedSet].SelectedControlIndex = ControlSets[SelectedSet].MenuControls.Length - 1;
            }
            else
            {
                // If the selection is to be advanced, advance
                ControlSets[SelectedSet].SelectedControlIndex -= 1;
            }
        }

        SelectControl(ControlSets[SelectedSet].SelectedControlIndex);
        return true;
    }
    public bool ScrollColumn(bool forward, GameManager.GameAspects.Profile profile)
    {
        // Returns if the selection was advanced

        // If the inputting profile not the active profile and the control is player specific, return false
        if (ProfileSpecific && profile != Profile)
            return false;

        Profile = profile;

        // If there are no controls, return false
        if (ControlSets.Length == 0)
            return false;
        // Select right or left index based on forward state
        int selectedSet = forward ? ControlSets[SelectedSet].RightSetIndex : ControlSets[SelectedSet].LeftSetIndex;
        if (selectedSet == -1)
            return false;

        // Deselect previously selected control
        SelectControl(ControlSets[SelectedSet].SelectedControlIndex, false);
        // Define selected set
        SelectedSet = selectedSet;
        // Select new control
        SelectControl(ControlSets[SelectedSet].SelectedControlIndex);

        return true;
    }
    public bool ScrollToEnd(bool forward, GameManager.GameAspects.Profile profile)
    {
        if (ControlSets.Length == 0 || ProfileSpecific && profile != Profile || IgnoreScrollToEndInput)
            return false;

        // If the selected column has no options to vertically scroll, jump to the last or first set
        if (ControlSets[SelectedSet].MenuControls.Length == 1)
        {
            // Deselect previously selected control
            SelectControl(ControlSets[SelectedSet].SelectedControlIndex, false);
            // Define selected set
            SelectedSet = forward ? ControlSets.Length - 1 : 0;
            // Select new control
            SelectControl(ControlSets[SelectedSet].SelectedControlIndex);
        }
        else
        {
            // Otherwise jump to the first or last control
            SelectControl(ControlSets[SelectedSet].SelectedControlIndex, false);

            int index = forward ? ControlSets[SelectedSet].MenuControls.Length - 1 : 0;

            ControlSets[SelectedSet].SelectedControlIndex = index;
            SelectControl(index);
        }

        return true;
    }

    private void SelectControl(int index, bool select = true)
    {
        // Select or deselect control based on state
        ControlSets[SelectedSet].MenuControls[index].Select(select);
        if (select)
            _selectedControl = ControlSets[SelectedSet].MenuControls[index];
    }
    public void InvokeButton(GameManager.GameAspects.Profile profile)
    {
        // If the inputting profile not the active profile and the control is player specific, abort
        if (ProfileSpecific && profile != Profile)
            return;

        Profile = profile;

        // if there is no selected control, abort
        if (_selectedControl == null)
            return;
        // If the selected controls type is button, push the button
        if (_selectedControl.GetType() == typeof(MenuButton))
        {
            MenuButton button = (MenuButton)_selectedControl;
            if (button.Push())
            {
                Close();
            }
        }
    }
    public void InvokeSetting(bool forward, GameManager.GameAspects.Profile profile)
    {
        // If the inputting profile not the active profile and the control is player specific, abort
        if (ProfileSpecific && profile != Profile)
            return;

        Profile = profile;

        // if there is no selected control, abort
        if (_selectedControl == null)
            return;

        // If the selected controls type is setting, advance the setting
        if (_selectedControl.GetType() == typeof(MenuSetting))
        {
            MenuSetting menuSetting = (MenuSetting)_selectedControl;
            menuSetting.Change(forward);
        }
        // If the selected controls type is map selector, advance the selection
        else if (_selectedControl.GetType() == typeof(MenuMapSelector))
        {
            MenuMapSelector mapSelector = (MenuMapSelector)_selectedControl;
            mapSelector.Change(forward);
        }
    }

    public bool InvokeAction(MenuController.InputButton button, int controllerID, GameManager.GameAspects.Profile profile)
    {
        // Returns if the button is to be used for the rest of the frame

        // If the inputting profile not the active profile or is null and the control is player specific, return false
        if (ProfileSpecific && (profile == null || profile != Profile))
            return false;

        Profile = profile;

        // Find each action that uses the button and process it
        bool ignoreButtonThisFrame = false;
        foreach (ButtonAction buttonAction in ButtonActions)
            if (buttonAction.Button == button)
            {
                ProccessAction(buttonAction, controllerID);
                // Determine if the button is to be ignored for the rest of the frame
                if (buttonAction.IgnoreButtonThisFrame)
                    ignoreButtonThisFrame = true;
            }
        return ignoreButtonThisFrame;
    }
    private void ProccessAction(ButtonAction action, int controllerID)
    {
        // Close this group if specified
        if (action.CloseThis)
            Close();
        
        if (action.MethodPath != string.Empty)
        {
            string path;
            object root;
            GetPathInfo(out root, out path, action.MethodPath);

            // Attempt to pass the controllerID as a parameter, if it fails, pass no parameters instead
            try
            {
                ObjectReferencer.InvokeMethod(root, path, new object[] { controllerID });
            }
            catch(SystemException e)
            {
                ObjectReferencer.InvokeMethod(root, path, null);
            }
        }
        
        // Process action
        switch (action.GroupAction)
        {
            case ButtonAction.Actions.ShowActionGroup:
                if (action.ActionGroup != null)
                    action.ActionGroup.Show(Profile);
                break;

            case ButtonAction.Actions.CloseActionGroup:
                action.ActionGroup.Close();
                break;
        }
    }

    private void GetPathInfo(out object root, out string actualPath, string path)
    {
        // Define the root of the method to be invoked
        string[] directories = path.Split('/');
        actualPath = path.Replace(directories[0] + "/", string.Empty);
        if (directories[0] == "profile")
            root = Profile;
        else if (directories[0] == "game_manager")
            root = GameManager.Instance;
        else
        {
            actualPath = path;
            root = this;
        }
    }
    #endregion
}