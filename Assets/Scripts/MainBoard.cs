using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainBoard : MonoBehaviour {

    [SerializeField] private GameObject _MainBoardObject;
    [SerializeField] private Transform _BoardTransform;
    [SerializeField] private Text _Debug;
    [SerializeField] private Transform _StartingPosition;
    [SerializeField] private Material _GhostMaterial;

    [Header("Brick Prefabs")]

    [SerializeField] private GameObject _BrickPrefab;
    [SerializeField] private GameObject _IBrickPrefab;
    [SerializeField] private GameObject _JBrickPrefab;
    [SerializeField] private GameObject _LBrickPrefab;
    [SerializeField] private GameObject _OBrickPrefab;
    [SerializeField] private GameObject _SBrickPrefab;
    [SerializeField] private GameObject _TBrickPrefab;
    [SerializeField] private GameObject _ZBrickPrefab;

    [Header("Parametres")]

    public Randomizer _Randomizer;
    public int seed;
    [SerializeField] private float _DroppingSpeed;
    [SerializeField] private float _DefaultShiftingHop = 7.0f;
    [SerializeField] private float _ShiftingHopAccurancy = 0.2f;


    [Header("Input")]

    [SerializeField] private KeyCode _LeftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode _RightKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode _DownKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode _RotateKey = KeyCode.LeftAlt;
    [SerializeField] private KeyCode _InfinityShiftKey = KeyCode.Space;

    private bool _LPressed = false;
    private bool _LReleased = false;
    private bool _RPressed = false;
    private bool _RReleased = false;
    private bool _DPressed = false;
    private bool _DReleased = false;
    private bool _LeftShifting = false;
    private bool _RightShifting = false;

    private bool _RotatePressed = false;
    private bool _InfinityPressed = false;
    
    private bool _ItsPlaced = true;
    private bool _IsEndGame = false;
    
    private short _CurrentBrickType = 5;


    private GameObject[,] _BoardOfBricks = new GameObject[10,20];
    private bool[,] _BoardOfOccupancy = new bool[10, 20];
    private GameObject _CurrentBrickDropping;
    private int _CurrentBrickSide = 0;

    private float _RLTimer = 0;
    private float _ShiftingHop;
    

    private float[,] brickCoords =
    {//   x1  y1  x2  y2  x3  y3, x4, y4   // brick type:
        {  0.0f,  0.0f,     0.0f,  0.0f,    0.0f,  0.0f,    0.0f,  0.0f },//0// nothing
	    { -1.0f,  0.0f,     0.0f,  1.0f,    1.0f,  0.0f,    0.0f,  0.0f },//1// T
	    {  1.0f,  0.0f,     0.0f,  1.0f,    1.0f,  1.0f,    0.0f,  0.0f },//2// O
	    { -1.0f,  0.0f,     1.0f,  0.0f,    2.0f,  0.0f,    0.0f,  0.0f },//3// I
	    { -1.0f,  0.0f,     1.0f,  1.0f,    1.0f,  0.0f,    0.0f,  0.0f },//4// J
	    { -1.0f,  0.0f,     1.0f,  0.0f,   -1.0f,  1.0f,    0.0f,  0.0f },//5// L
	    { -1.0f,  0.0f,     0.0f, -1.0f,    1.0f, -1.0f,    0.0f,  0.0f },//6// S
	    { -1.0f, -1.0f,     0.0f, -1.0f,    1.0f,  0.0f,    0.0f,  0.0f } //7// Z
    };

    private short[,] ghostCoords =
    {
        {0,18},
        {0,18},
        {0,18},
        {0,18}
    };

    void Awake()
    {
        Randomizer.seed = seed;
    }

	// Use this for initialization
	void Start () {

        _Debug.text = "";
        _ShiftingHop = _DefaultShiftingHop;

        _LeftKey = KeyCode.LeftArrow;
        _RightKey = KeyCode.RightArrow;
        _RotateKey = KeyCode.LeftAlt;
        _DownKey = KeyCode.DownArrow;
        
        _BulidBoard();
        _DeactivateAll();
    }
    
	// Update is called once per frame
	void Update () {

        if (!_IsEndGame)
        {
            _UpdateStates();
            _UpdateMove();

            _LPressed = Input.GetKeyDown(_LeftKey);
            _LReleased = Input.GetKeyUp(_LeftKey);
            _RPressed = Input.GetKeyDown(_RightKey);
            _RReleased = Input.GetKeyUp(_RightKey);
            _DPressed = Input.GetKeyDown(_DownKey);
            _DReleased = Input.GetKeyUp(_DownKey);

            _RotatePressed = Input.GetKeyDown(_RotateKey);
            _InfinityPressed = Input.GetKeyDown(_InfinityShiftKey);
        }

        _ShowDebug();
	}

    private void _UpdateStates()
    {
        if(_ItsPlaced)
        {
            _CheckLines();
            _ResetSideState();
            GameObject obj = _TBrickPrefab;
            _CurrentBrickType = (short)Randomizer.getNewRand();

            _CanPlaceAnother();

            switch (_CurrentBrickType)
            {
                case 1: obj = _TBrickPrefab; break;
                case 2: obj = _OBrickPrefab; break;
                case 3: obj = _IBrickPrefab; break;
                case 4: obj = _JBrickPrefab; break;
                case 5: obj = _LBrickPrefab; break;
                case 6: obj = _SBrickPrefab; break;
                case 7: obj = _ZBrickPrefab; break;
            }

            _CurrentBrickDropping = Instantiate(obj, _StartingPosition.transform.position, _MainBoardObject.transform.rotation) as GameObject;
            _CurrentBrickDropping.transform.parent = _MainBoardObject.transform;
            _ItsPlaced = false;
            if (!_IsEndGame)
            {
                _CalculateGhost();
                _ShowGhost();
            }
        }

        if(_LPressed)
        {
            _LeftShifting = true;
            if (_TryShiftHorizontal(-1))
                _CurrentBrickDropping.transform.localPosition += Vector3.left;
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
            _RLTimer = 0;
        }

        if(_LReleased)
        {
            _LeftShifting = false;
            _ShiftingHop = _DefaultShiftingHop;
        }

        if (_LeftShifting)
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

        if (_RPressed)
        {
            _RightShifting = true;
            if (_TryShiftHorizontal(1))
                _CurrentBrickDropping.transform.localPosition += Vector3.right;
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
            _RLTimer = 0.0f;
        }

        if (_RReleased)
        {
            _RightShifting = false;
            _ShiftingHop = _DefaultShiftingHop;
        }

        if (_RightShifting)
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
        
        if (_DPressed)
        {
            _DroppingSpeed *= 3.0f;
        }


        if (_DReleased)
        {
            _DroppingSpeed /= 3.0f;
        }

        if (_RotatePressed)
        {
            if (_TryRotateLeftBrick())
            {
                _CurrentBrickDropping.transform.localEulerAngles += new Vector3(0.0f, 0.0f, 90.0f);
            }
            _HideGhost();
            _CalculateGhost();
            _ShowGhost();
        }
    }

    private void _CanPlaceAnother()
    {
        for (int i = 0; i < 4; i++)
        {
            short tmpX = (short)(brickCoords[_CurrentBrickType, 2 * i] + _StartingPosition.position.x);
            short tmpY = (short)(brickCoords[_CurrentBrickType, 2 * i + 1] + _StartingPosition.position.y);

            if (_BoardOfOccupancy[tmpX, tmpY])
            {
                _IsEndGame = true;
                break;
            }

        }
    }

    private void _UpdateMove()
    {
        if(!_ItsPlaced)
        {
            if(_TryShiftDown( -_DroppingSpeed))
            {
                _CurrentBrickDropping.transform.localPosition += Vector3.down * _DroppingSpeed;
            }
            else
            {
                _HideGhost();
                Vector3 tmpPos = _CurrentBrickDropping.transform.localPosition;
                tmpPos = new Vector3(tmpPos.x, Mathf.Round( tmpPos.y), tmpPos.z);
                _CurrentBrickDropping.transform.localPosition = tmpPos;

                
              
                for (int i = 0; i < 4; i++)
                {
                    short tmpX = (short)(brickCoords[_CurrentBrickType, 2 * i] + tmpPos.x);
                    short tmpY = (short)(brickCoords[_CurrentBrickType, 2 * i + 1] + tmpPos.y);
                    
                    _BoardOfOccupancy[tmpX, tmpY] = true;


                    _BoardOfBricks[tmpX, tmpY].gameObject.GetComponent<Renderer>().material = _CurrentBrickDropping.GetComponentInChildren<Renderer>().material;
                    _BoardOfBricks[tmpX, tmpY].gameObject.SetActive(true);

                }
                

                Destroy(_CurrentBrickDropping);
                _ItsPlaced = true;
            }
            if (_InfinityPressed)
                _TryShiftInfinitlyDown(-_DroppingSpeed);
        }
    }

    private void _BulidBoard()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                _BoardOfBricks[x, y] = Instantiate(_BrickPrefab, _MainBoardObject.transform.position + new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
                _BoardOfBricks[x, y].transform.parent = _MainBoardObject.transform;
            }
        }
    }

    private void _DeactivateAll()
    {
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 20; y++)
                _BoardOfBricks[x, y].gameObject.SetActive(false);
    }

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

    private void _TryShiftInfinitlyDown(float yInterval)
    {
        _CurrentBrickDropping.transform.localPosition = new Vector3(ghostCoords[3, 0], ghostCoords[3, 1]);
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

    private void _CheckLines()
    {
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
                _ShiftBoardDown(y);
            }
            else
            {
                y++;
            }
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
}
