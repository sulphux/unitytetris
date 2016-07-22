using UnityEngine;
using System.Collections;

public class MainBoard : MonoBehaviour {

    [SerializeField]
    private GameObject _MainBoardObject;

    [SerializeField]
    private Transform _BoardTransform;

    [Header("Brick Prefabs")]

    [SerializeField]
    private GameObject _BrickPrefab;

    [SerializeField]
    private GameObject _LBrickPrefab;

    [Header("Parametres")]

    [SerializeField]
    private float _DroppingSpeed;

    [Header("Input")]

    [SerializeField]
    private KeyCode _LeftKey = KeyCode.LeftArrow;

    [SerializeField]
    private KeyCode _RightKey = KeyCode.RightArrow;

    [SerializeField]
    private KeyCode _RotateKey = KeyCode.LeftAlt;

    private bool _LPressed = false;
    private bool _RPressed = false;
    private bool _RotatePressed = false;

    private bool _BlinkState = true;
    private bool _ItsPlaced = true;

    private float _BlinkingTimer = 0.0f;
    private short _CurrentBrickType = 5;
    private static Vector3 _StartingPosition;


    private GameObject[,] _BoardOfBricks = new GameObject[10,20];
    private bool[,] _BoardOfOccupancy = new bool[10, 20];
    private GameObject _CurrentBrickDropping;
    private int _CurrentBrickSide = 0;

    private float[,] brickCoords =
    {//   x1  y1  x2  y2  x3  y3, x4, y4   // brick type:
        {  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f },//0// nothing
	    { -1.0f,  0.0f,  0.0f, -1.0f,  1.0f,  0.0f,  0.0f,  0.0f },//1// T
	    {  0.0f, -1.0f,  1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  0.0f },//2// O
	    { -1.0f,  0.0f,  1.0f,  0.0f,  2.0f,  0.0f,  0.0f,  0.0f },//3// I
	    { -1.0f,  0.0f,  1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  0.0f },//4// J
	    { -1.0f,  0.0f,  1.0f,  0.0f, -1.0f,  1.0f,  0.0f,  0.0f },//5// L
	    { -1.0f,  0.0f,  0.0f, -1.0f,  1.0f, -1.0f,  0.0f,  0.0f },//6// S
	    { -1.0f, -1.0f,  0.0f, -1.0f,  1.0f,  0.0f,  0.0f,  0.0f } //7// Z
    };


	// Use this for initialization
	void Start () {

        _LeftKey = KeyCode.LeftArrow;
        _RightKey = KeyCode.RightArrow;
        _RotateKey = KeyCode.LeftAlt;

        _StartingPosition = _BoardTransform.position + new Vector3(4.0f, 18.0f, 0.0f);

        _BulidBoard();
        _DeactivateAll();


        float test1 = 0.001f;
        float test2 = 0.9999f;
        float test3 = 1.01f;

        Debug.Log((short)test1);
        Debug.Log((short)test2);
        Debug.Log((short)test3);
    }


    // Fixed update
	void FixedUpdate()
    {
        _BlinkingTimer += Time.deltaTime;

        if (_BlinkingTimer > 0.5f)
        {
            _BlinkState = !_BlinkState;
            _BlinkingTimer = 0.0f;
            _BoardOfBricks[7, 7].gameObject.SetActive(_BlinkState);

        }

    }

	// Update is called once per frame
	void Update () {

        _UpdateStates();
        _UpdateMove();

        _LPressed = Input.GetKeyDown(_LeftKey);
        _RPressed = Input.GetKeyDown(_RightKey);
        _RotatePressed = Input.GetKeyDown(_RotateKey);
	}

    private void _UpdateStates()
    {
        if(_ItsPlaced)
        {
            _CurrentBrickDropping = Instantiate(_LBrickPrefab, _StartingPosition, Quaternion.identity) as GameObject;
            _ItsPlaced = false;
            _ResetSideState();
        }

        if(_LPressed)
        {
            if (_TryShiftHorizontal(-1))
                _CurrentBrickDropping.transform.position += Vector3.left;
        }

        if (_RPressed)
        {
            if (_TryShiftHorizontal(1))
                _CurrentBrickDropping.transform.position += Vector3.right;
        }

        if (_RotatePressed)
        {
            if (_TryRotateLeftBrick())
            {
                _CurrentBrickDropping.transform.Rotate(Vector3.forward, 90.0f);
                Debug.Log(brickCoords[_CurrentBrickType, 0] + " " +
                          brickCoords[_CurrentBrickType, 1] + " " +
                          brickCoords[_CurrentBrickType, 2] + " " +
                          brickCoords[_CurrentBrickType, 3] + " " +
                          brickCoords[_CurrentBrickType, 4] + " " +
                          brickCoords[_CurrentBrickType, 5]);
            }
        }
    }

    private void _UpdateMove()
    {
        if(!_ItsPlaced)
        {
            if(_TryShiftDown( -_DroppingSpeed))
            {
                _CurrentBrickDropping.transform.position += Vector3.down * _DroppingSpeed;
            }
            else
            {
                Vector3 tmpPos = _CurrentBrickDropping.transform.position;
                Debug.Log("Before round: Y:" + tmpPos.y);
                tmpPos = new Vector3(tmpPos.x, Mathf.Round( tmpPos.y), tmpPos.z);
                Debug.Log("After round: Y:" + tmpPos.y);
                _CurrentBrickDropping.transform.position = tmpPos;

                for (int i = 0; i < 3; i++)
                {
                    short tmpX = (short)(brickCoords[_CurrentBrickType, 2 * i] + tmpPos.x);
                    short tmpY = (short)(brickCoords[_CurrentBrickType, 2 * i + 1] + tmpPos.y);

                    Debug.Log("Before Occupancy array X:" + tmpX + " Y:" + tmpY);
                    _BoardOfOccupancy[tmpX, tmpY] = true;
                }
                _BoardOfOccupancy[(short)tmpPos.x, (short)tmpPos.y] = true;
                _ItsPlaced = true;
                _DebugOccupancy();
            }
            
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
            short discreteX = (short)(_CurrentBrickDropping.transform.position.x + brickCoords[_CurrentBrickType, 2 * i]);
            short discreteY = (short)(_CurrentBrickDropping.transform.position.y - 1.0f + yInterval + brickCoords[_CurrentBrickType, 2 * i + 1]);
            short discreteY2 = (short)(_CurrentBrickDropping.transform.position.y + yInterval + +brickCoords[_CurrentBrickType, 2 * i + 1]);

            if (discreteY <= -1)
                return false;

            if (_BoardOfOccupancy[discreteX, discreteY2])
                return false;
        }
        
        return true;
    }

    private bool _TryShiftHorizontal(short xInterval)
    {
        short tmpX = 0;
        short tmpY = 0;

        for (int i = 0; i < 4; i++)
        {
            tmpX = (short)(_CurrentBrickDropping.transform.position.x + brickCoords[_CurrentBrickType, 2 * i]);
            tmpY = (short)(_CurrentBrickDropping.transform.position.y + brickCoords[_CurrentBrickType, 2 * i + 1]);

            if (tmpX + xInterval >= 10 || tmpX + xInterval <= -1)
                return false;

            if (_BoardOfOccupancy[tmpX + xInterval, tmpY])
                return false;
        }
        
        return true;
    }


    private bool _TryRotateLeftBrick()
    {
        _RotateSchemeLeft();
        /*
        if (currentBrickType == 2) return false;

        pickUpBrick();
        rotateBrickSchemeLeft();
        for (int i = 0; i < 3; i++)
        {
            int tmpX = currBrickX + brickCoords[currentBrickType][2 * i];
            int tmpY = currBrickY + brickCoords[currentBrickType][2 * i + 1];
            if (tmpX < 0 || tmpX > 9 || tmpY < 0 || tmpY > 19 || mainBoard[tmpX][tmpY] != 0)
            {
                rotateBrickSchemeRight();
                putDownBrick();
                return false;
            }
        }
        putDownBrick();*/
        return true;
    }

    private void _RotateSchemeLeft()
    {
        for (int i = 0; i < 3; i++)
        {
            float newX = brickCoords[_CurrentBrickType, 2 * i + 1] * -1.0f;
            float newY = brickCoords[_CurrentBrickType, 2 * i];
            brickCoords[_CurrentBrickType, 2 * i] = newX;
            brickCoords[_CurrentBrickType, 2 * i + 1] = newY;
        }
        _CurrentBrickSide--;
    }

    private void _RotateSchemeRight()
    {
        for (int i = 0; i < 3; i++)
        {
            float newX = brickCoords[_CurrentBrickType, 2 * i + 1];
            float newY = brickCoords[_CurrentBrickType, 2 * i] * -1.0f;
            brickCoords[_CurrentBrickType, 2 * i] = newX;
            brickCoords[_CurrentBrickType, 2 * i + 1] = newY;
        }
        _CurrentBrickSide++;
    }

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

    private void _DebugOccupancy()
    {
        string tmp = "";
        for (short y = 19; y >= 0; y--)
        {
            for (short x = 0; x < 10; x++)
            {
                if (_BoardOfOccupancy[x, y])
                    tmp += "X";
                else
                    tmp += "O";
            }
            tmp += "\n";
        }
        Debug.Log(tmp);
    }
}
