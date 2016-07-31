using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreController : MonoBehaviour {

    [SerializeField] private Text _ScoreString;


    private int _MainScore;

	// Use this for initialization
	void Start () {
        _MainScore = 0;
	}
	
	// Update is called once per frame
	void Update () {
        _ScoreString.text = _FormatScore();
	}

    public void GrantPointsForLines(short[] linesCoordInstance)
    {
        short[] linesCoord = new short[4];

        for (int i = 0; i <4; i++)
            linesCoord[i] = linesCoordInstance[i];

        short zero = linesCoord[0];

        int howMany = 0;

        for (int i = 0; i < 4; i++)
        {
            if (linesCoord[i] >= 0)
            {
                linesCoord[i] -= zero;
                howMany++;
            }
        }
        if (howMany == 1)
        {
            _OneLine();
            return;
        }
        else if (howMany == 4)
        {
            _Tetris();
            return;
        }
        else
        {
            if (howMany == 2)
            {
                if (linesCoord[1] - linesCoord[0] == 1)
                {
                    _TwoLines();
                    return;
                }
                else
                {
                    _OneLine();
                    _OneLine();
                    return;
                }
            }

            if(howMany == 3)
            {
                if(linesCoord[1] - linesCoord[0] == 1 && linesCoord[2] - linesCoord[1] == 1)
                {
                    _ThreeLines();
                    return;
                }
                else
                {
                    _TwoLines();
                    _OneLine();
                }
            }
        }
        
    }

    private void _OneLine()    { _MainScore += 100; Debug.Log("One Line!"); }
    private void _TwoLines()   { _MainScore += 250; Debug.Log("Double Line!"); }
    private void _ThreeLines() { _MainScore += 600; Debug.Log("Triple Line!"); }
    private void _Tetris()     { _MainScore += 1200; Debug.Log("Tetris!"); }

    private string _FormatScore()
    {
        string tmp = "";
        string score = _MainScore.ToString();
        
        for (int i = 0; i < 7 - score.Length; i++)
        {
            tmp += "0";
        }
        tmp += score;
        
        return tmp;
    }
}
