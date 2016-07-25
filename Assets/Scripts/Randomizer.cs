using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Randomizer : MonoBehaviour {

    public static Randomizer instance = null;
    
    private static int last;
    private static int[] pieces = new int[28];
    private float[] noiseValues;

    // Use this for initialization
    void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Random.seed = Random.Range(0,100);
        

        newRandom();
        last = 0;
        
        
	}
	
    public static int getNewRand()
    {
        if (last < 27)
        {
            int tmp = pieces[last];
            last++;
            return tmp;
        }
        else
        {
            int tmp = pieces[27];
            newRandom();
            last = 0;
            return tmp;
        }
    }

    private static void newRandom()
    {
        for (int i = 0; i < 7; i++)
        {
            pieces[i] = pieces[i + 7] = pieces[i + 14] = pieces[i + 21] = i + 1;
        }
        for (int i = 0; i < 50; i++)
        {
            swap(Random.Range(0, 28), Random.Range(0, 28));
        }
    }

    private static void swap(int index1, int index2)
    {
        int k = pieces[index1];
        pieces[index1] = pieces[index2];
        pieces[index2] = k;
    }
}
