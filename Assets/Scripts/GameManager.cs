using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    GameObject _clickedDomino;
    public GameObject ClickedDomino{get => _clickedDomino; set => _clickedDomino = value;}
    bool _isGameActive;
    public bool IsGameActive{get => _isGameActive; set => _isGameActive = value;}
    bool _isAtStartOfTurn;
    bool _isAtEndOfTurn; 
    public bool IsAtEndOfTurn{set => _isAtEndOfTurn = value;}
    bool _isCpuTurn;
    public bool IsCpuTurn{get => _isCpuTurn;}
    bool _hasTurnEnded;
    public bool HasTurnEnded{get => _hasTurnEnded;set => _hasTurnEnded = value;}
    bool _hasPlayerWon;
    public bool HasPlayerWon{get => _hasPlayerWon;set => _hasPlayerWon = value;}
    int _currentTurnPlayerNum;
    public int CurrentTurnPlayerNum{get => _currentTurnPlayerNum;}
    int _doubleTrainPlayerNum;
    public int DoubleTrainPlayerNum{get => _doubleTrainPlayerNum; set => _doubleTrainPlayerNum = value;}
    Train _doubleTrainPlayerScript;
    public Train DoubleTrainPlayerScript{get => _doubleTrainPlayerScript; set => _doubleTrainPlayerScript = value;}
    // List<int[]> _bestDominoPath;
    List<int[]> _drawPile;
    public List<int[]> DrawPile{get => _drawPile;}
    Train _currentTurnTrainScript;
    public Train CurrentTurnTrainScript{get => _currentTurnTrainScript;}
    // List<int[]> _spareDominoes;
    // Dictionary<int[],GameObject> _dominoObjects;
    // Train _trainScript;
    // [SerializeField] GameObject _domino;
    GameObject _mexicanTrain;
    public GameObject MexicanTrain{get => _mexicanTrain;}
    Train _mexicanTrainScript;
    public Train MexicanTrainScript{get => _mexicanTrainScript;}
    CameraScript _cameraScript;
    [SerializeField] GameObject[] _numbers; 
    public GameObject[] Numbers{get => _numbers;}
    [SerializeField] GameObject[] _trains;
    public GameObject[] Trains{get => _trains;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        _currentTurnPlayerNum = 1;
        _clickedDomino = null;
        _drawPile = new();
        GenerateDrawPile();
        _cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
    } 
    void Start()
    {
        _mexicanTrain = _trains[8];
        _mexicanTrainScript = _mexicanTrain.GetComponent<Train>();
        _currentTurnTrainScript = _trains[CurrentTurnPlayerNum - 1].GetComponent<Train>();
        foreach(GameObject train in _trains)
        {
            if (train.GetComponent<Train>().PlayerNum != 1)
            {
                train.GetComponent<Train>().IsCPU = true;
            }  
            train.GetComponent<Train>().GenerateStartingDominoes(); 
        }
        if (_currentTurnTrainScript.IsCPU){_isCpuTurn = true;}
        _isAtStartOfTurn = true;
        _isGameActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isGameActive)
        {
            if (_isAtStartOfTurn)
            {
                _currentTurnTrainScript.TakeTurn();
                _isAtStartOfTurn = false;
            }
            else if (_isAtEndOfTurn)
            {
                _currentTurnPlayerNum++;
                if (_currentTurnPlayerNum - 1 == 8){_currentTurnPlayerNum = 1;}
                _currentTurnTrainScript = _trains[CurrentTurnPlayerNum - 1].GetComponent<Train>();
                _isCpuTurn = _currentTurnTrainScript.IsCPU;
                if (_isCpuTurn && _cameraScript.IsViewingDeck){_cameraScript.ActivateGameView();}
                _isAtStartOfTurn = true;
                _isAtEndOfTurn = false;
            }   
        }
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

    public void PlayerHasWon(int playerNum)
    {
        _isGameActive = false;
        print($"Player {playerNum} has won the game!");
    }
}
