using System;
using UnityEngine;


/// <summary>
/// Handles player input
/// </summary>
[Serializable]
public class PlayerInput : MonoBehaviour {

    #region Inspector Variables
    [Header("Input")]
    [SerializeField] private KeyCode _LeftKey           = KeyCode.LeftArrow;
    [SerializeField] private KeyCode _RightKey          = KeyCode.RightArrow;
    [SerializeField] private KeyCode _DownKey           = KeyCode.DownArrow;
    [SerializeField] private KeyCode _RotateKey         = KeyCode.LeftAlt;
    [SerializeField] private KeyCode _InfShiftKey       = KeyCode.Space;
    [SerializeField] private KeyCode _ResetKey          = KeyCode.R;
    [SerializeField] private KeyCode _HoldKey           = KeyCode.LeftShift;
    [SerializeField] private KeyCode _PauseKey = KeyCode.P;
    #endregion Inspector Variables

    #region Public Variables
    public bool LeftD       { get { if (!_IsPaused) return _LeftPressed; return false; } }
    public bool LeftU       { get { if (!_IsPaused) return _LeftReleased; return false; } }
    public bool RightD      { get { if (!_IsPaused) return _RightPressed; return false; } }
    public bool RightU      { get { if (!_IsPaused) return _RightReleased; return false; } }
    public bool DownD       { get { if (!_IsPaused) return _DownPressed; return false; } }
    public bool DownU       { get { if (!_IsPaused) return _DownReleased; return false; } }
    public bool Reset       { get { if (!_IsPaused) return _ResetPressed; return false; } }
    public bool Hold        { get { if (!_IsPaused) return _HoldPressed; return false; } }
    public bool Rotate      { get { if (!_IsPaused) return _RotatePressed; return false; } }
    public bool Infinity    { get { if (!_IsPaused) return _InfinityPressed; return false; } }
    public bool Pause       { get { return _PausePressed; } }
    #endregion Public Variables

    #region Private Variables
    private bool _IsPaused = false;
    private bool _LeftPressed = false;
    private bool _LeftReleased = false;
    private bool _RightPressed = false;
    private bool _RightReleased = false;
    private bool _DownPressed = false;
    private bool _DownReleased = false;
    private bool _ResetPressed = false;
    private bool _HoldPressed = false;
    private bool _RotatePressed = false;
    private bool _InfinityPressed = false;
    private bool _PausePressed = false;
    #endregion Private Variables


    #region Unity Messages
    private void Update()
    {
        if (Input.GetKey("escape"))
            Application.Quit();

        _LeftPressed    = Input.GetKeyDown(_LeftKey);
        _LeftReleased   = Input.GetKeyUp(_LeftKey);
        _RightPressed   = Input.GetKeyDown(_RightKey);
        _RightReleased  = Input.GetKeyUp(_RightKey);
        _DownPressed    = Input.GetKeyDown(_DownKey);
        _DownReleased   = Input.GetKeyUp(_DownKey);
        _HoldPressed    = Input.GetKeyDown(_HoldKey);

        _RotatePressed   = Input.GetKeyDown(_RotateKey);
        _InfinityPressed = Input.GetKeyDown(_InfShiftKey);

        _ResetPressed = Input.GetKeyDown(_ResetKey);
        _PausePressed = Input.GetKeyDown(_PauseKey);

    }
    #endregion Unity Messages

    #region Public Methods
    public void SetPauseState(bool state)
    {
        _IsPaused = state;
    }
    #endregion Public Methods
}
