using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainBoard : MonoBehaviour {
    
    #region Inspector Variables
    [Header("Classes")]
    [SerializeField] private PlayerInput _PlayerInput;
    [SerializeField] private GamePrefabs _GamePrefabs;
    [SerializeField] private Randomizer _Randomizer;
    [SerializeField] private ScoreController _ScoreController;

    [Header("Transforms")]
    [SerializeField] private Transform _BoardTransform;
    [SerializeField] private Transform _StartingPosition;
    [SerializeField] private Transform _NextBrickPosition;
    [SerializeField] private Transform _HoldBrickPosition;

    [Header("Game Objects")]
    [SerializeField] private GameObject _MainBoardObject;
    [SerializeField] private GameObject _PutParticle;
    [SerializeField] private GameObject _BlinkPrefab;

    [Header("Materials")]
    [SerializeField] private Material _GhostMaterial;

    [Header("Canvas")]
    [SerializeField] private Text _Debug;

    [Header("Parametres")]
    [SerializeField] private float _DroppingSpeed;
    [SerializeField] private float _DefaultShiftingHop = 7.0f;
    [SerializeField] private float _ShiftingHopAccurancy = 0.2f;
    #endregion Inspector Variables

    #region Private Variables - Bool
    private bool _ItsPlaced = true;
    private bool _IsEndGame = false;
    private bool _IsEliminatingLines = false;
    private bool _IsHoldUsed = false;
    private bool _IsLeftShifting = false;
    private bool _IsRightShifting = false;
    private bool _IsCountingPut = false;
    private bool _IsPaused = false;
    private bool[,] _BoardOfOccupancy = new bool[10, 20];
    #endregion

    #region Private Variables - Numeric
    private short _NextBrickType = 4;
    private short _CurrentBrickType = 5;
    private short _HoldBrickType = 0;
    private short[] _ToEliminate = new short[4];

    private int _CurrentBrickSide = 0;

    private float _DeathBlinkTimer = 0.0f;
    private float _RLTimer = 0.0f;
    [SerializeField] private float _PutTimer = 0.0f;
    private float _ShiftingHop;

    private float[,] brickCoords =
    {//   x1  y1  x2  y2  x3  y3, x4, y4   // brick type:
        {  0.0f,  0.0f,     0.0f,  0.0f,    0.0f,  0.0f,    0.0f,  0.0f },//0// nothing
	    { -1.0f,  0.0f,     0.0f,  1.0f,    1.0f,  0.0f,    0.0f,  0.0f },//1// T
	    {  1.0f,  0.0f,     0.0f,  1.0f,    1.0f,  1.0f,    0.0f,  0.0f },//2// O
	    { -1.0f,  0.0f,     1.0f,  0.0f,    2.0f,  0.0f,    0.0f,  0.0f },//3// I
	    { -1.0f,  0.0f,     1.0f,  1.0f,    1.0f,  0.0f,    0.0f,  0.0f },//4// J
	    { -1.0f,  0.0f,     1.0f,  0.0f,   -1.0f,  1.0f,    0.0f,  0.0f },//5// L
	    { -1.0f,  1.0f,     0.0f,  1.0f,    1.0f,  0.0f,    0.0f,  0.0f },//6// S
	    { -1.0f,  0.0f,     0.0f,  1.0f,    1.0f,  1.0f,    0.0f,  0.0f } //7// Z
    };

    private short[,] ghostCoords =
    {
        {0,18},
        {0,18},
        {0,18},
        {0,18}
    };
    #endregion Private Variables - Numeric

    #region Private Variables - Game Objects
    private GameObject[,] _BoardOfBricks = new GameObject[10,20];
    private GameObject[] _Blinks = new GameObject[20];
    private GameObject _CurrentBrickDropping;
    private GameObject _NextBrickObject;
    private GameObject _HoldBrickObject;
    #endregion Private Variables - Game Objects

    #region Unity Messages
    void Awake()
    {
    }

	// Use this for initialization
	void Start () {

        _Debug.text = "";
        _ShiftingHop = _DefaultShiftingHop;
        _NextBrickType = (short)Randomizer.getNewRand();

        _BulidBoard();
        _DeactivateAll();

        for (int i = 0; i < 4; i++)
            _ToEliminate[i] = -1;
    }
    
    void FixedUpdate()
    {
        if(!_IsEndGame && !_IsPaused)
        {
            _UpdateMove();
        }
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 1.0f);
    }

	// Update is called once per frame
	void Update () {

        if (!_IsEndGame)
        {
            _UpdateStates();
        }

        //_ShowDebug();
	}
    #endregion Unity Messages

    private void _ResetGame()
    {
        Destroy(_CurrentBrickDropping);
        _DeactivateAll();
        for (int y = 0; y < 20; y++)
            for (int x = 0; x < 10; x++)
                _BoardOfOccupancy[x, y] = false;
        _ItsPlaced = true;
    }

    #region States Update
    private void _UpdateStates()
    {
        _PlayerInput.SetPauseState(_IsPaused);
     
        if(_ItsPlaced)
        {
            _CheckLines();
            _ResetSideState();
            _CurrentBrickType = _NextBrickType;
            _NextBrickType = (short)Randomizer.getNewRand();


            if (_CanPlaceAnother())
            {
                GameObject obj = _GamePrefabs.GetBrick(_CurrentBrickType);

                _CurrentBrickDropping = Instantiate(obj, _StartingPosition.position, _MainBoardObject.transform.rotation) as GameObject;

                Destroy(_NextBrickObject);

                obj = _GamePrefabs.GetBrick(_NextBrickType);

                _NextBrickObject = Instantiate(obj, _NextBrickPosition.position, _MainBoardObject.transform.rotation) as GameObject;

                _ItsPlaced = false;
                
                _CalculateGhost();
                _ShowGhost();

                _IsHoldUsed = false;
            }
            else
                _IsEndGame = true;
        }

        #region Checking Inputs
        if (_PlayerInput.Hold)
        {
            if(!_IsHoldUsed && _HoldBrickType == 0)
            {
                _HideGhost();
                _CurrentBrickDropping.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                _ResetSideState();

                _HoldBrickObject = Instantiate(_CurrentBrickDropping, _HoldBrickPosition.position, Quaternion.identity) as GameObject;
                _HoldBrickType = _CurrentBrickType;

                Destroy(_CurrentBrickDropping);


                _IsHoldUsed = true;

                _HoldBrickType = _CurrentBrickType;

                _ItsPlaced = true;
            }
            else if(!_IsHoldUsed)
            {
                _HideGhost();
                GameObject go;
               
                _CurrentBrickDropping.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                _ResetSideState();
               

                short tmp = _CurrentBrickType;
                _CurrentBrickType = _HoldBrickType;
                _HoldBrickType = tmp;

                go = _HoldBrickObject;
                _HoldBrickObject = _CurrentBrickDropping;
                _CurrentBrickDropping = go;

                _CurrentBrickDropping.transform.position = _StartingPosition.position;
                _HoldBrickObject.transform.position = _HoldBrickPosition.transform.position;
                _IsHoldUsed = true;
                _CalculateGhost();
                _ShowGhost();
            }
        }

        if(_PlayerInput.LeftD)
        {
            _IsLeftShifting = true;
            if (_TryShiftHorizontal(-1))
            {
                _CurrentBrickDropping.transform.localPosition += Vector3.left;
                _PutTimer = 0.4f;
            }
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
            _RLTimer = 0;
        }

        if(_PlayerInput.LeftU)
        {
            _IsLeftShifting = false;
            _ShiftingHop = _DefaultShiftingHop;
        }

        if (_PlayerInput.RightD)
        {
            _IsRightShifting = true;
            if (_TryShiftHorizontal(1))
            {
                _CurrentBrickDropping.transform.localPosition += Vector3.right;
                _PutTimer = 0.4f;
            }
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
            _RLTimer = 0.0f;
        }

        if (_PlayerInput.RightU)
        {
            _IsRightShifting = false;
            _ShiftingHop = _DefaultShiftingHop;
        }

        
        if (_PlayerInput.DownD)
        {
            _DroppingSpeed *= 3.0f;
        }


        if (_PlayerInput.DownU)
        {
            _DroppingSpeed /= 3.0f;
        }

        if (_PlayerInput.Rotate)
        {
            if (_TryRotateLeftBrick())
            {
                _CurrentBrickDropping.transform.localEulerAngles += new Vector3(0.0f, 0.0f, 90.0f);
            }
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
        }

        if(_PlayerInput.Reset)
            _ResetGame();


        if (_PlayerInput.Pause)
        {
            if (_IsPaused)
                _IsPaused = false;
            else
                _IsPaused = true;
        }

        if (_PlayerInput.Infinity)
            _TryShiftInfinitlyDown();
        #endregion Input Checking

        #region States Checking

        if (_IsLeftShifting)
        {
            _RLTimer += Time.deltaTime;
            if (_RLTimer > _ShiftingHop)
            {
                if (_TryShiftHorizontal(-1))
                    _CurrentBrickDropping.transform.localPosition += Vector3.left;
                _HideGhost();
                _CalculateGhost();
                _ShowGhost();
                _RLTimer = 0.0f;
                _ShiftingHop -= _ShiftingHopAccurancy;
            }
        }
        
        if (_IsRightShifting)
        {
            _RLTimer += Time.deltaTime;
            if (_RLTimer > _ShiftingHop)
            {
                if (_TryShiftHorizontal(1))
                    _CurrentBrickDropping.transform.localPosition += Vector3.right;
                _HideGhost();
                _CalculateGhost();
                _ShowGhost();
                _RLTimer = 0;
                _ShiftingHop -= _ShiftingHopAccurancy;
            }
        }

        if (_IsEliminatingLines)
        {
            _DeathBlinkTimer -= Time.deltaTime;
            
            if (_DeathBlinkTimer < 0.0f)
            {
                _HideGhost();
                short diff = 0;
                for(int i = 0; i <4; i++)
                {
                    if (_ToEliminate[i] != -1)
                    {
                        _ShiftBoardDown(_ToEliminate[i] - diff);
                        diff++;
                    }

                    _ToEliminate[i] = -1;
                }

                _IsEliminatingLines = false;
                _CalculateGhost();
                _ShowGhost();
            }
        }

        if (_IsCountingPut)
        {
            _PutTimer -= Time.deltaTime;
        }
        #endregion States Checking
    }

    #endregion States Update

    #region Playing Brick Manipulators 
    private void _UpdateMove()
    {
        if (!_ItsPlaced)
        {
            if (_TryShiftDown(-_DroppingSpeed))
            {
                _CurrentBrickDropping.transform.localPosition += Vector3.down * _DroppingSpeed;
                _IsCountingPut = false;
            }
            else
            {
                if (!_IsCountingPut)
                {
                    _IsCountingPut = true;
                    _PutTimer = 0.4f;
                }

                if (_IsCountingPut && _PutTimer < 0.0)
                {
                    _HideGhost();
                    Vector3 tmpPos = _CurrentBrickDropping.transform.localPosition;
                    tmpPos = new Vector3(tmpPos.x, Mathf.Round(tmpPos.y), tmpPos.z);
                    _CurrentBrickDropping.transform.localPosition = tmpPos;



                    for (int i = 0; i < 4; i++)
                    {
                        short tmpX = (short)(brickCoords[_CurrentBrickType, 2 * i] + tmpPos.x);
                        short tmpY = (short)(brickCoords[_CurrentBrickType, 2 * i + 1] + tmpPos.y);

                        _BoardOfOccupancy[tmpX, tmpY] = true;


                        //_BoardOfBricks[tmpX, tmpY].gameObject.GetComponent<Renderer>().materials = _CurrentBrickDropping.GetComponentInChildren<Renderer>().material;
                        Material mat = new Material(_CurrentBrickDropping.GetComponentInChildren<Renderer>().material);
                        _BoardOfBricks[tmpX, tmpY].GetComponent<Renderer>().material.CopyPropertiesFromMaterial(mat);
                        _BoardOfBricks[tmpX, tmpY].SetActive(true);

                        Instantiate(_PutParticle, _BoardTransform.position + new Vector3((float)tmpX, (float)tmpY, 0.0f), Quaternion.identity);

                        FlashBehavior flash = _BoardOfBricks[tmpX, tmpY].GetComponent<FlashBehavior>();
                        flash._Flash();

                    }


                    Destroy(_CurrentBrickDropping);
                    _ItsPlaced = true;
                    _IsCountingPut = false;
                }
            }

        }
    }

    private void _TryShiftInfinitlyDown()
    {
        _CurrentBrickDropping.transform.localPosition = new Vector3(ghostCoords[3, 0], ghostCoords[3, 1]);
        _IsCountingPut = true;
        _PutTimer = 0.00f;
    }

        private void _RotateScheme(float direction)
    {
        for (int i = 0; i < 3; i++)
        {
            float newX = brickCoords[_CurrentBrickType, 2 * i + 1] * direction;
            float newY = brickCoords[_CurrentBrickType, 2 * i] * -1.0f * direction;
            brickCoords[_CurrentBrickType, 2 * i] = newX;
            brickCoords[_CurrentBrickType, 2 * i + 1] = newY;
        }
        _CurrentBrickSide += (int)direction;
    }

    private void _RotateSchemeLeft() { _RotateScheme(-1.0f); }
    private void _RotateSchemeRight() { _RotateScheme(1.0f); }

    private void _ResetSideState()
    {
        if (_CurrentBrickSide != 0)
        {
            _CurrentBrickSide %= 4;

            while (_CurrentBrickSide > 0)
                _RotateSchemeLeft();

            while (_CurrentBrickSide < 0)
                _RotateSchemeRight();
        }
    }

    #endregion Playing Brick Manipulators
    
    #region Board Manipulators Methods
    private void _BulidBoard()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                _BoardOfBricks[x, y] = Instantiate(_GamePrefabs.Single, _MainBoardObject.transform.position + new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
                _BoardOfBricks[x, y].GetComponent<Renderer>().material = _GhostMaterial;
                _BoardOfBricks[x, y].transform.parent = _MainBoardObject.transform;
            }
        }
   
        for (int y = 0; y < 20; y++)
        {
            _Blinks[y] = Instantiate(_BlinkPrefab, _MainBoardObject.transform.position + new Vector3(4.5f, y, 0.0f), Quaternion.identity) as GameObject;
        }
    }


    private void _ShiftBoardDown(int indexY)
    {
        for (int y = indexY; y < 19; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                _BoardOfBricks[x, y].gameObject.GetComponent<Renderer>().material = _BoardOfBricks[x, y + 1].gameObject.GetComponent<Renderer>().material;
                _BoardOfBricks[x, y].gameObject.SetActive(_BoardOfBricks[x, y + 1].activeInHierarchy);
                _BoardOfOccupancy[x, y] = _BoardOfOccupancy[x, y + 1];
            }
        }
    }

    private void _DeactivateAll()
    {
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 20; y++)
                _BoardOfBricks[x, y].gameObject.SetActive(false);
    }

    private void _CheckLines()
    {
        int i = 0;
        int y = 0;
        while (y < 19)
        {
            int x = 0;
            while (x < 10 && _BoardOfOccupancy[x, y])
            {
                x++;
            }
            if (x > 9)
            {
                _ToEliminate[i] = (short)y;
                i++;
                y++;
            }
            else
            {
                y++;
            }
        }

        if (i > 0)
        {
            _ScoreController.GrantPointsForLines(_ToEliminate);
            _IsEliminatingLines = true;
            _DeathBlinkTimer = 0.4f;
        }

        for (int j = 0; j < 4; j++)
        {
            if (_ToEliminate[j] != -1)
                _Blinks[_ToEliminate[j]].GetComponent<BlinkBehavior>()._Flash();
        }

    }

    #endregion Board Manipulators Methods
    
    #region Bool Methods
    private bool _TryShiftDown(float yInterval)
    {

        for (int i = 0; i < 4; i++)
        {
            short discreteX = (short)(_CurrentBrickDropping.transform.localPosition.x + brickCoords[_CurrentBrickType, 2 * i]);
            short discreteY = (short)(_CurrentBrickDropping.transform.localPosition.y - 1.0f + yInterval + brickCoords[_CurrentBrickType, 2 * i + 1]);
            short discreteY2 = (short)(_CurrentBrickDropping.transform.localPosition.y + yInterval + +brickCoords[_CurrentBrickType, 2 * i + 1]);

            if (discreteY <= -1)
                return false;

            if (_BoardOfOccupancy[discreteX, discreteY2])
                return false;
        }

        return true;
    }


    private bool _CanPlaceAnother()
    {
        for (int i = 0; i < 4; i++)
        {
            short tmpX = (short)(brickCoords[_CurrentBrickType, 2 * i] + _StartingPosition.position.x);
            short tmpY = (short)(brickCoords[_CurrentBrickType, 2 * i + 1] + _StartingPosition.position.y);

            if (_BoardOfOccupancy[tmpX, tmpY])
            {
                _IsEndGame = true;
                return false;
            }

        }
        return true;
    }


    private bool _TryShiftHorizontal(short xInterval)
    {
        short tmpX = 0;
        short tmpY = 0;

        for (int i = 0; i < 4; i++)
        {
            tmpX = (short)(_CurrentBrickDropping.transform.localPosition.x + brickCoords[_CurrentBrickType, 2 * i]);
            tmpY = (short)(_CurrentBrickDropping.transform.localPosition.y + brickCoords[_CurrentBrickType, 2 * i + 1]);

            if (tmpX + xInterval >= 10 || tmpX + xInterval <= -1)
                return false;

            if (_BoardOfOccupancy[tmpX + xInterval, tmpY])
                return false;
        }

        return true;
    }


    private bool _TryRotateLeftBrick()
    {

        if (_CurrentBrickType == 2) return false;


        _RotateSchemeLeft();

        for (int i = 0; i < 3; i++)
        {
            short tmpX = (short)(_CurrentBrickDropping.transform.localPosition.x + brickCoords[_CurrentBrickType, 2 * i]);
            short tmpY = (short)(_CurrentBrickDropping.transform.localPosition.y + brickCoords[_CurrentBrickType, 2 * i + 1]);
            if (tmpX < 0 || tmpX > 9 || tmpY < 0 || tmpY > 19 || _BoardOfOccupancy[tmpX, tmpY])
            {
                _RotateSchemeRight();
                return false;
            }
        }

        return true;
    }

    #endregion Bool Methods
    
    #region Ghost Functions

    private void _CalculateGhost()
    {
        short checkingY = (short)(_CurrentBrickDropping.transform.localPosition.y);
        bool goodHeight = false;

        while(!goodHeight && checkingY >= 0)
        {
            for (int i = 0; i < 4; i++)
            {
                short tmpX = (short)(_CurrentBrickDropping.transform.localPosition.x + brickCoords[_CurrentBrickType, 2 * i]);
                short tmpY = (short)(checkingY + brickCoords[_CurrentBrickType, 2 * i + 1]);
                 if (checkingY >= 0 && (tmpY < 0 || _BoardOfOccupancy[tmpX, tmpY]))
                 {
                     checkingY++;
                     goodHeight = true;
                     break;
                 }
            }
            checkingY--;
        }
        checkingY++;

        for (int i = 0; i < 4; i++)
        {

            short tmpX = (short)(_CurrentBrickDropping.transform.localPosition.x + brickCoords[_CurrentBrickType, 2 * i]);
            short tmpY = (short)(checkingY + brickCoords[_CurrentBrickType, 2 * i + 1]);
            ghostCoords[i, 0] = tmpX;
            ghostCoords[i, 1] = tmpY;
        }


    }
    private void _ShowGhost()
    {
        for (int i = 0; i < 4; i++)
        {
            short tmpX = ghostCoords[i, 0];
            short tmpY = ghostCoords[i, 1];
            _BoardOfBricks[tmpX, tmpY].gameObject.GetComponent<Renderer>().material = _GhostMaterial;
            _BoardOfBricks[tmpX, tmpY].gameObject.SetActive(true);
        }
    }

    private void _HideGhost()
    {
        for (int i = 0; i < 4; i++)
        {
            short tmpX = ghostCoords[i, 0];
            short tmpY = ghostCoords[i, 1];
            _BoardOfBricks[tmpX, tmpY].gameObject.SetActive(false);
        }
    }

    #endregion Ghost Functions

    #region Validation Methods
    private void ValidateReferences()
    {
        FindReferenceInHierarchy(ref _PlayerInput);
        FindReferenceInHierarchy(ref _GamePrefabs);
        FindReferenceInHierarchy(ref _Randomizer);
        FindReferenceInHierarchy(ref _MainBoardObject);
    }
    #endregion Validation Methods
    
    #region Helper Methods
    /// <summary>
    /// Given parameter of type T, find component in GameObject or its children
    /// <T> is a declaration of generic method, where T is a type resolved from usage
    /// </summary>
    private void FindReferenceInHierarchy<T>(ref T reference) where T : class
    {
        if (!reference.Equals(null))
        {
            return;
        }

        reference = GetComponent<T>();
        if (!reference.Equals(null))
        {
            return;
        }

        reference = GetComponentInChildren<T>();

        if (reference.Equals(null))
        {
            Debug.LogError("Reference to object: " + typeof(T).Name + " not found");
        }
    }
    #endregion Helper Methods

    #region Debug Functions

    private bool[,] _BrickDebug = new bool[5, 5];
    private void _ShowDebug()
    {
        _Debug.text = "Current Brick Type: " + _CurrentBrickType.ToString() + "\n";

        for (int y = 19; y >= 0; y--)
        {
            for (int x = 0; x < 10; x++)
            {
                if (_BoardOfOccupancy[x, y])
                    _Debug.text += "X";
                else
                    _Debug.text += "O";
            }
            _Debug.text += "\n";
        }

        _Debug.text += "\n";

        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                _BrickDebug[x, y] = false;

        for (int i = 0; i < 4; i++ )
        {
            int tmpX = (int)brickCoords[_CurrentBrickType, 2 * i];
            int tmpY = (int)brickCoords[_CurrentBrickType, 2 * i + 1];

            _BrickDebug[tmpX + 2, tmpY + 2] = true;
        }

        for (int y = 4; y >= 0; y--)
        {
            for (int x = 0; x < 5; x++)
            {
                if (_BrickDebug[x, y])
                    _Debug.text += "X";
                else
                    _Debug.text += "O";
            }
            _Debug.text += "\n";
        }

    }
    #endregion Debug Functions
}
