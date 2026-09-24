using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    int _numOfTurns;
    public int NumOfTurns{get => _numOfTurns; set => _numOfTurns = value;}
    int _currentTurnPlayerNum;
    public int CurrentTurnPlayerNum{get => _currentTurnPlayerNum;}
    int _doubleTrainPlayerNum;
    public int DoubleTrainPlayerNum{get => _doubleTrainPlayerNum; set => _doubleTrainPlayerNum = value;}
    public enum PlayerType{CPU,HUMAN,NOT_PLAYING}
    Dictionary<int, PlayerType> _playerTypes;
    Dictionary<int, int> _dominoNumsAmounts;
    public Dictionary<int,int> DominoNumsAmounts{get => _dominoNumsAmounts;}
    Train _doubleTrainPlayerScript;
    public Train DoubleTrainPlayerScript{get => _doubleTrainPlayerScript; set => _doubleTrainPlayerScript = value;}
    List<int[]> _drawPile;
    public List<int[]> DrawPile{get => _drawPile;}
    Train _currentTurnTrainScript;
    public Train CurrentTurnTrainScript{get => _currentTurnTrainScript;}
    GameObject _mexicanTrain;
    public GameObject MexicanTrain{get => _mexicanTrain;}
    Train _mexicanTrainScript;
    public Train MexicanTrainScript{get => _mexicanTrainScript;}
    CameraScript _cameraScript;
    [SerializeField] GameObject[] _numbers; 
    public GameObject[] Numbers{get => _numbers;}
    [SerializeField] GameObject[] _trains;
    public GameObject[] Trains{get => _trains;}
    [SerializeField] GameObject _menuObjects;
    [SerializeField] GameObject _playerDropDowns;
    [SerializeField] GameObject _middleScreenTextObject;
    [SerializeField] TextMeshProUGUI _middleScreenText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        _currentTurnPlayerNum = 1;
        _clickedDomino = null;
        _drawPile = new();
        GenerateDrawPile();
        _cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
        _playerTypes = new();
        for (int i=0; i < _trains.Length; i++)
        {
            if (i + 1 == _trains.Length)
            {
                _playerTypes[i+1] = PlayerType.NOT_PLAYING; 
                break;
            }
            _playerTypes[i+1] = PlayerType.CPU;
        }
        _dominoNumsAmounts = new();
        for (int i = 0; i < _numbers.Length; i++){_dominoNumsAmounts[i] = 0;}
    } 
    void Start()
    {
        _mexicanTrain = _trains[^1];
        _mexicanTrainScript = _mexicanTrain.GetComponent<Train>();
        _currentTurnTrainScript = _trains[CurrentTurnPlayerNum - 1].GetComponent<Train>();
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
                bool isAValidTrain = false;
                do
                {
                    _currentTurnPlayerNum++;
                    if (_currentTurnPlayerNum == _trains.Length){_currentTurnPlayerNum = 1;}
                    if (_playerTypes[_currentTurnPlayerNum] != PlayerType.NOT_PLAYING)
                    {
                        isAValidTrain = true;
                    }
                } while (!isAValidTrain);
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

    void StartGame()
    {
        int numOfPlayers = 0;
        for(int i = 0; i < _trains.Length; i++)
        {
            if (_playerTypes[i + 1] == PlayerType.NOT_PLAYING){continue;}
            numOfPlayers++;
        }
        if (numOfPlayers < 2)
        {
            StartCoroutine(DisplayTextTimer("You must have at least two players to start"));
            return;
        }
        for(int i = 0; i < _trains.Length;i++)
        {
            PlayerType trainStatus = _playerTypes[i + 1];
            if (trainStatus == PlayerType.NOT_PLAYING){continue;}
            Train trainScript = _trains[i].GetComponent<Train>();
            if (trainStatus == PlayerType.CPU){trainScript.IsCPU = true;}
            trainScript.GenerateStartingDominoes(numOfPlayers); 
        }
        StopAllCoroutines();
        _middleScreenTextObject.SetActive(false);
        _menuObjects.SetActive(false);
        if (_currentTurnTrainScript.IsCPU){_isCpuTurn = true;}
        _cameraScript.ActivateGameView();
        _isAtStartOfTurn = true;
        _isGameActive = true;
    }

    IEnumerator DisplayTextTimer(string textToDisplay)
    {
        _middleScreenText.text = textToDisplay;
        _middleScreenTextObject.SetActive(true);
        yield return new WaitForSeconds(3);
        _middleScreenTextObject.SetActive(false);
    }

    public void StartGameButton(){StartGame();} 

    public void ChangePlayerStatus(int player)
    {

        int typeSelection = _playerDropDowns.transform.GetChild(player - 1).GetComponentInChildren<TMP_Dropdown>().value;
        print(typeSelection);
        _playerTypes[player] = (PlayerType) typeSelection;
    }

    public void ChangeDominoNumsAmounts(int dominoNum){_dominoNumsAmounts[dominoNum]++;}

    public void PlayerHasWon(int playerNum)
    {
        _isAtEndOfTurn = _isAtStartOfTurn = _isGameActive = false;
        print($"Player {playerNum} has won the game!");
        int[,] playerScoreValues = new int[_trains.Length,2];
        print(playerScoreValues.Length);
        for (int i = 0; i < _trains.Length; i++)
        {
            if (_playerTypes[i+1] == PlayerType.NOT_PLAYING || i+1 == playerNum)
            {
                playerScoreValues[i,0] = playerScoreValues[i,1] = 0;
                continue;
            }
            Train trainScript = _trains[i].GetComponent<Train>();
            int playerScore = trainScript.CalculateFinalScore();
            playerScoreValues[i,0] = trainScript.PlayerNum;
            playerScoreValues[i,1] = playerScore;
            // print($"Player: {scorePlayerValues[i,0]}, score: {scorePlayerValues[i,1]}");
        }
        bool isSorted = false;
        do
        {
            bool hasChanged = false;
            for (int i = 0; i < _trains.Length - 1; i++)
            {
                if (playerScoreValues[i,1] > playerScoreValues[i+1, 1])
                {
                    int tempPlayerNum = playerScoreValues[i+1,0];
                    int tempPlayerScore = playerScoreValues[i+1,1];
                    playerScoreValues[i+1,0] = playerScoreValues[i,0];
                    playerScoreValues[i+1,1] = playerScoreValues[i,1];
                    playerScoreValues[i,0] = tempPlayerNum;
                    playerScoreValues[i,1] = tempPlayerScore; 
                    hasChanged = true;
                }
            }
            if (!hasChanged){isSorted = true;}
        } while(!isSorted);
        for (int i = 0; i < _trains.Length; i++)
        {
            if(playerScoreValues[i,0] == 0){continue;}
            print($"Sorted: Player: {playerScoreValues[i,0]}, score: {playerScoreValues[i,1]}");
        }
        print($"Game took {_numOfTurns} turns");
    }

}
