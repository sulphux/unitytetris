using System;
using UnityEngine;

/// <summary>
/// Holds brick prefabs
/// </summary>
[Serializable]
public class GamePrefabs : MonoBehaviour {

    #region Inspector Variables
    [Header("Brick Prefabs")]
    [SerializeField] private GameObject _BrickPrefab;
    [SerializeField] private GameObject _IBrickPrefab;
    [SerializeField] private GameObject _JBrickPrefab;
    [SerializeField] private GameObject _LBrickPrefab;
    [SerializeField] private GameObject _OBrickPrefab;
    [SerializeField] private GameObject _SBrickPrefab;
    [SerializeField] private GameObject _TBrickPrefab;
    [SerializeField] private GameObject _ZBrickPrefab;
    #endregion Inspector Variables

    #region Public Variables
    public GameObject Single    { get { return _BrickPrefab;  } }
    public GameObject I         { get { return _IBrickPrefab; } }
    public GameObject J         { get { return _JBrickPrefab; } }
    public GameObject L         { get { return _LBrickPrefab; } }
    public GameObject O         { get { return _OBrickPrefab; } }
    public GameObject S         { get { return _SBrickPrefab; } }
    public GameObject T         { get { return _TBrickPrefab; } }
    public GameObject Z         { get { return _ZBrickPrefab; } }
    #endregion Public Variables

    #region Public Methods
    public GameObject GetBrick (int which)
    {
        GameObject obj = null; 
        switch (which)
        {
            case 1: obj = _TBrickPrefab; break;
            case 2: obj = _OBrickPrefab; break;
            case 3: obj = _IBrickPrefab; break;
            case 4: obj = _JBrickPrefab; break;
            case 5: obj = _LBrickPrefab; break;
            case 6: obj = _SBrickPrefab; break;
            case 7: obj = _ZBrickPrefab; break;
            default: obj = null; break;
        }

        return obj;
    }

    public GameObject GetBrick (short which) { return GetBrick((int)which); }

    #endregion Public Methods

}
