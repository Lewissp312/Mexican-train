using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    GameObject _clickedDomino;
    public GameObject ClickedDomino{get => _clickedDomino; set => _clickedDomino = value;}
    bool _isCpuTurn;
    public bool IsCpuTurn{get => _isCpuTurn;}
    int _currentTurn;
    public int CurrentTurn{get => _currentTurn;}
    int _doubleTrain;
    public int DoubleTrain{get => _doubleTrain; set => _doubleTrain = value;}
    // List<int[]> _bestDominoPath;
    List<int[]> _drawPile;
    public List<int[]> DrawPile{get => _drawPile;}
    // List<int[]> _spareDominoes;
    // Dictionary<int[],GameObject> _dominoObjects;
    // Train _trainScript;
    // [SerializeField] GameObject _domino;
    [SerializeField] GameObject[] _numbers; 
    public GameObject[] Numbers{get => _numbers;}
    [SerializeField] GameObject[] _trains;
    public GameObject[] Trains{get => _trains;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _currentTurn = 1;
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        _currentTurn = 1;
        _clickedDomino = null;
        _drawPile = new();
        GenerateDrawPile();
    } 
    void Start()
    {
        foreach(GameObject train in _trains)
        {
            if (train.name == "Train 1")
            {
                train.GetComponent<Train>().GenerateStartingDominoes();   
            }

            // Train trainScript =  train.GetComponent<Train>();
            // if (train.name != "Mexican Train")
            // {
            //     train.GetComponent<Train>().GenerateStartingDominoes();   
            // }
            // else
            // {
            //     print("Not loaded");
            // }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateDrawPile()
    {
        for(int i=0; i < _numbers.Length - 1; i++)
        {
            for (int k=i; k < _numbers.Length; k++)
            {
                int[] numCombination = {i,k};
                _drawPile.Add(numCombination);
            }
        }
        print($"Draw pile total: {_drawPile.Count}");
    }
}
