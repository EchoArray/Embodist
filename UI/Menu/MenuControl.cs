using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuControl : MonoBehaviour
{
    #region Values

    public bool Locked = false;
    public bool HideForClient;
    /// <summary>
    /// Defines the menu control group that contains this menu control.
    /// </summary>
    [HideInInspector]
    public MenuControlGroup Parent;

    /// <summary>
    /// Defines the options color upon selection
    /// </summary>
    public Color ActiveColor;
    /// <summary>
    /// Defines the options color while selected and locked.
    /// </summary>
    public Color LockedColor;
    /// <summary>
    /// Defines the options color while not selected
    /// </summary>
    public Color InactiveColor;

    /// <summary>
    /// Defines the text component that is to have its text changed upon selection.
    /// </summary>
    [Space(10)]
    public Text SelectedText;
    /// <summary>
    /// Defines the string that will be applied to the name identifier upon selection.
    /// </summary>
    public string SelectedString;

    /// <summary>
    /// Defines an object that's active state is based on the selection state of this control.
    /// </summary>
    [Space(10)]
    public GameObject SelectedIdentifier;

    private Image _image;
    private RawImage _rawImage;
    private MaskableGraphic _maskableGraphic;
	#endregion

    #region Unity Functions
    public void Awake()
    {
        _maskableGraphic = this.gameObject.GetComponent<MaskableGraphic>();
        _rawImage = this.gameObject.GetComponent<RawImage>();
        _image = this.gameObject.GetComponent<Image>();
    } 
    #endregion

    #region Functions
    public virtual void Select(bool state)
    {
        if (SelectedText != null)
            SelectedText.text = SelectedString;

        // Set the color of the maskable graphic
        Color color = InactiveColor;
        if (state)
            color = Locked ? LockedColor : ActiveColor;

        _maskableGraphic.color = color;
        // Set the active state of the selected identifier
        if (SelectedIdentifier != null)
            SelectedIdentifier.SetActive(state);
    }

    public void SetTexture(Texture2D texture)
    {
        // If the control has a raw image component attached, set the texture
        if (_rawImage != null)
            _rawImage.texture = texture;
    }
    public void SetSprite(Sprite sprite)
    {
        // If the control has a image component attached, set the sprite
        if (_image != null)
            _image.sprite = sprite;
    }

    public virtual void Load(MenuControlGroup parent, bool changed = false)
    {
        if (parent.DisabledForClient && NetworkSessionManager.IsClient)
            Select(false);

        this.gameObject.SetActive(!(HideForClient && NetworkSessionManager.IsClient));
        Parent = parent;
    }

    public void GetPathInfo(out object root, out string actualPath, string path)
    {
        // Define the root of the method to be invoked
        string[] directories = path.Split('/');
        actualPath = path.Replace(directories[0] + "/", string.Empty);
        if (directories[0] == "profile")
            root = Parent.Profile;
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
