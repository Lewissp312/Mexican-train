using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    bool _isGameActive;
    public bool IsGameActive{get => _isGameActive; set => _isGameActive = value;}
    bool _isAtEndOfTurn; 
    public bool IsAtEndOfTurn{set => _isAtEndOfTurn = value;}
    bool _isCpuTurn;
    public bool IsCpuTurn{get => _isCpuTurn;}
    bool _isAtStartOfTurn;
    int _numOfTurns;
    public int NumOfTurns{get => _numOfTurns; set => _numOfTurns = value;}
    int _currentTurnPlayerNum;
    public int CurrentTurnPlayerNum{get => _currentTurnPlayerNum;}
    int _doubleTrainPlayerNum;
    public int DoubleTrainPlayerNum{get => _doubleTrainPlayerNum; set => _doubleTrainPlayerNum = value;}
    int _mexicanTrainPlayerNum;
    public int MexicanTrainPlayerNum{get => _mexicanTrainPlayerNum; set => _mexicanTrainPlayerNum = value;}
    int _numOfTrains;
    public int NumOfTrains{get => _numOfTrains;}
    public enum PlayerType{CPU,HUMAN,NOT_PLAYING}
    Dictionary<int, PlayerType> _playerTypes;
    Dictionary<int, int> _dominoNumsAmounts;
    public Dictionary<int,int> DominoNumsAmounts{get => _dominoNumsAmounts;}
    List<int[]> _drawPile;
    public List<int[]> DrawPile{get => _drawPile;}
    Train _doubleTrainPlayerScript;
    public Train DoubleTrainPlayerScript{get => _doubleTrainPlayerScript; set => _doubleTrainPlayerScript = value;}
    Train _currentTurnTrainScript;
    public Train CurrentTurnTrainScript{get => _currentTurnTrainScript;}
    Train _mexicanTrainScript;
    public Train MexicanTrainScript{get => _mexicanTrainScript;}
    readonly Vector3 _endPos = new(-110,0,-1);
    CameraScript _cameraScript;
    readonly WaitForSeconds gameDelay = new(2.5f);
    GameObject _mexicanTrain;
    GameObject _clickedDomino;
    public GameObject ClickedDomino{get => _clickedDomino; set => _clickedDomino = value;}
    [SerializeField] GameObject[] _numbers; 
    public GameObject[] Numbers{get => _numbers;}
    [SerializeField] GameObject[] _trains;
    public GameObject[] Trains{get => _trains;}
    [SerializeField] GameObject _menuObjects;
    [SerializeField] GameObject _playerDropDowns;
    [SerializeField] GameObject _middleScreenTextObject;
    [SerializeField] GameObject _endingObjects;
    [SerializeField] TextMeshProUGUI _middleScreenText;
    [SerializeField] TextMeshProUGUI _endingWinnerText;
    [SerializeField] TextMeshProUGUI _endingScoresText;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Unity functions

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        _currentTurnPlayerNum = 1;
        _numOfTrains = _trains.Length;
        _clickedDomino = null;
        _drawPile = new();
        _playerTypes = new();
        _mexicanTrain = _trains[^1];
        _mexicanTrainScript = _mexicanTrain.GetComponent<Train>();
        _mexicanTrainPlayerNum = _mexicanTrainScript.PlayerNum;
        _playerTypes[_mexicanTrainPlayerNum] = PlayerType.NOT_PLAYING;
        for (int i=0; i < _numOfTrains - 1; i++)
        {
            //The Mexican Train is the last train in the list, so ignore the last value 
            _playerTypes[i+1] = PlayerType.CPU;
        }
        _dominoNumsAmounts = new();
        for (int i = 0; i < _numbers.Length; i++){_dominoNumsAmounts[i] = 0;}
        _cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
        _currentTurnTrainScript = _trains[CurrentTurnPlayerNum - 1].GetComponent<Train>();
        GenerateDrawPile();
    } 

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
                    if (_currentTurnPlayerNum == _mexicanTrainPlayerNum){_currentTurnPlayerNum = 1;}
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

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Private

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
        for(int i = 0; i < _numOfTrains; i++)
        {
            if (_playerTypes[i + 1] == PlayerType.NOT_PLAYING){continue;}
            numOfPlayers++;
        }
        if (numOfPlayers < 2)
        {
            StartCoroutine(DisplayTextTimer("You must have at least two players to start"));
            return;
        }
        for(int i = 0; i < _numOfTrains;i++)
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
        yield return gameDelay;
        _middleScreenTextObject.SetActive(false);
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Public

    public void StartGameButton(){StartGame();} 

    public void ChangePlayerStatus(int playerNum)
    {
        int typeSelection = _playerDropDowns.transform.GetChild(playerNum - 1).GetComponentInChildren<TMP_Dropdown>().value;
        _playerTypes[playerNum] = (PlayerType) typeSelection;
    }

    public void ChangeDominoNumsAmounts(int dominoNum){_dominoNumsAmounts[dominoNum]++;}

    public void ShowBestPathButton()
    {
        CurrentTurnTrainScript.ShowBestPath();
    }

    public void PlayerHasWon(int playerNum)
    {
        Camera.main.transform.position = _endPos;
        _isAtEndOfTurn = _isAtStartOfTurn = _isGameActive = false;
        _endingObjects.SetActive(true);
        _endingWinnerText.text = $"Player {playerNum} has won the game!";
        int[,] playerScoreValues = new int[_numOfTrains,2];
        for (int i = 0; i < _numOfTrains; i++)
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
        }
        bool isSorted = false;
        do
        {
            bool hasChanged = false;
            for (int i = 0; i < _numOfTrains - 1; i++)
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
        int place = 2;
        int previousScore = 0;
        for (int i = 0; i < _numOfTrains; i++)
        {
            if(playerScoreValues[i,0] == 0){continue;}
            int currentScore = playerScoreValues[i,1];
            if (previousScore > 0)
            {
                if (currentScore != previousScore){place++;}
            }
            string placeEnding = place switch
            {
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
            _endingScoresText.text +=  $"{place}{placeEnding} place: Player {playerScoreValues[i,0]}, {currentScore} point{(currentScore == 1 ? "" : "s")}\n";
            previousScore = currentScore;
        }
        _endingScoresText.text += $"Game took {_numOfTurns} turns"; 
        print($"Game took {_numOfTurns} turns");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
