using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Train : MonoBehaviour, IPointerClickHandler
{
    bool _isCPU;
    public bool IsCPU{get => _isCPU; set => _isCPU = value;}
    bool _canAddDiffDomino;
    public bool CanAddDiffDomino{get => _canAddDiffDomino; set => _canAddDiffDomino = value;}
    bool _isUsable;
    public bool IsUsable{get => _isUsable; set => _isUsable = value;}
    bool _canInteract;
    public bool CanInteract{get => _canInteract;}
    bool _canTakeTurn;
    bool _canGoToPart1;
    bool _canGoToPart2;
    bool _canGoToPart3;
    bool _canGoToPart4;
    bool _canGoToPart5;
    int _currentTurnPart;
    public int CurrentTurnPart{set => _currentTurnPart = value;}
    int _lastPlayedDominoNum;
    public int LastPlayedDominoNum{get => _lastPlayedDominoNum;}
    int _trainSize;
    SpriteRenderer _trainIndicatorRenderer;
    Vector3 _lastPlayedDominoPos;
    readonly Vector3 _rightPosVector = new(0.42f,-0.6f,0);
    readonly Vector3 _leftPosVector = new(-0.42f,-0.6f,0);
    readonly Vector3 _deckViewingPos = new(-5,-53,0);
    Dictionary<int[],GameObject> _dominoObjects;
    List<int[]> _bestPathDominoes;
    List<int[]> _spareDominoes;
    readonly WaitForSeconds gameDelay = new(1f);
    [SerializeField] bool _isPublicTrain;
    public bool IsPublicTrain{get => _isPublicTrain;}
    [SerializeField] int _playerNum;
    public int PlayerNum{get => _playerNum;}
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject _trainIndicator;
    [SerializeField] GameObject _middleScreenTextObject;
    [SerializeField] TextMeshProUGUI _middleScreenText;
    [SerializeField] Button _showBestPathButton;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Unity functions

    void Awake()
    {
        _canGoToPart1 = true;
        _currentTurnPart = 1;
        _lastPlayedDominoNum = 12;
        _bestPathDominoes = new();
        _spareDominoes = new();
        _dominoObjects = new();
        _trainIndicatorRenderer = _trainIndicator.GetComponent<SpriteRenderer>();
    } 

    void Start()
    {
        if (_playerNum == GameManager.Instance.MexicanTrainScript.PlayerNum){_isPublicTrain = true; ChangeUsability(true);}
    }

    // Update is called once per frame
    void Update()
    {
        if (_canTakeTurn)
        {
            switch (_currentTurnPart)
            {
                case 1:
                    if (!_canGoToPart1){return;}
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer($"Player {_playerNum}'s turn!")); 
                    _currentTurnPart = 2;
                    _canInteract = false;  
                    break;
                case 2:
                    if (!_canGoToPart2){return;}
                    FindBestPath();
                    if (GameManager.Instance.DoubleTrainPlayerNum == 0)
                    {
                        //if there is no double train active
                        ChangeUsability(true);
                        if (_isPublicTrain)
                        {
                            //If you have something that can be placed on your public train, you must do it
                            if (_bestPathDominoes.Count > 0)
                            {
                                foreach(GameObject train in GameManager.Instance.Trains)
                                {
                                    Train trainScript = train.GetComponent<Train>();
                                    if (trainScript.PlayerNum != _playerNum)
                                    {
                                        trainScript.ChangeUsability(false);
                                    }
                                }
                            }
                        }
                    }
                    if (!_isCPU && !_dominoObjects[_spareDominoes[0]].activeSelf)
                    {
                        //Do not need to do this if it's your second go after placing a double, as it's already been done
                        foreach(int[] dominoes in _spareDominoes){_dominoObjects[dominoes].SetActive(true);}
                        foreach(int[] dominoes in _bestPathDominoes){_dominoObjects[dominoes].SetActive(true);}
                    }
                    if (!CanPlayerActuallyDoAnything())
                    {
                        StopAllCoroutines();
                        _canInteract = false;
                        if (GameManager.Instance.DoubleTrainPlayerNum != 0 && GameManager.Instance.DominoNumsAmounts[GameManager.Instance.DoubleTrainPlayerScript.LastPlayedDominoNum] >= 13)
                        {
                            //If there's a double train and there are no more dominoes that can complete it
                            _canAddDiffDomino = true;
                            if (_isCPU)
                            {
                                GameManager.Instance.DoubleTrainPlayerScript.AddDominoToTrain(_spareDominoes.Count > 0 ? _dominoObjects[_spareDominoes[0]] : _dominoObjects[_bestPathDominoes[0]]);
                            }
                            else
                            {
                                StartCoroutine(DisplayTextTimer($"Player {_playerNum}, as there are no more dominoes to properly complete the double, you can add one of your choosing"));
                                _currentTurnPart = 0;
                            }
                        }
                        else if (GameManager.Instance.DoubleTrainPlayerNum == 0 && GameManager.Instance.DominoNumsAmounts[_lastPlayedDominoNum] >= 13)
                        {
                            //If there's no more dominoes that can complete the player's train
                            _canAddDiffDomino = true;
                            if (_isCPU)
                            {
                                AddDominoToTrain(_spareDominoes.Count > 0 ? _dominoObjects[_spareDominoes[0]] : _dominoObjects[_bestPathDominoes[0]]);
                            }
                            else
                            {
                                _isPublicTrain = true;
                                foreach(GameObject train in GameManager.Instance.Trains)
                                {
                                    Train trainScript = train.GetComponent<Train>();
                                    if (trainScript.PlayerNum != _playerNum)
                                    {
                                        trainScript.ChangeUsability(false);
                                    }
                                }
                                StartCoroutine(DisplayTextTimer($"Player {_playerNum}, as there are no more dominoes to add to your train,\n you can add one of your choosing"));
                                _currentTurnPart = 0;
                            }
                        } 
                        else if (GameManager.Instance.DrawPile.Count == 0)
                        {
                            if (!_isPublicTrain)
                            {
                                _isPublicTrain = true;   
                            }
                            StartCoroutine(DisplayTextTimer($"Player {_playerNum}, there are no moves you can make and the domino pile is empty. Your train is public"));
                            _currentTurnPart = 5;  
                            _canInteract = false;
                        }
                        else
                        {
                            StartCoroutine(DisplayTextTimer($"Player {_playerNum}, there are no moves you can make, getting a domino from the pile"));
                            _currentTurnPart = 3; 
                            _canInteract = false;     
                        }
                    }
                    break;
                case 3:
                    if (!_canGoToPart3){return;}
                    AddDominoToDeck();
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer($"Player {_playerNum}, ({_spareDominoes[^1][0]},{_spareDominoes[^1][1]}) was added to your deck")); 
                    _currentTurnPart = 4;  
                    _canInteract = false;
                    break;
                case 4:
                    if (!_canGoToPart4){return;}
                    FindBestPath();
                    if (_bestPathDominoes.Count > 0 && _isPublicTrain && GameManager.Instance.DoubleTrainPlayerNum == 0)
                    {
                        //Again, if you have something that can be placed on your public train, you must do it
                        foreach(GameObject train in GameManager.Instance.Trains)
                        {
                            Train trainScript = train.GetComponent<Train>();
                            if (trainScript.PlayerNum != _playerNum)
                            {
                                trainScript.ChangeUsability(false);
                            }
                        } 
                    }
                    if (!CanPlayerActuallyDoAnything())
                    {
                        StopAllCoroutines();
                        _canGoToPart5 = false;
                        _currentTurnPart = 5;
                        _canInteract = false;
                        if (GameManager.Instance.DoubleTrainPlayerNum == 0)
                        {
                            if (!_isPublicTrain)
                            {
                                _isPublicTrain = true;
                                StartCoroutine(DisplayTextTimer($"Player {PlayerNum}, that domino doesn't help you and your train is now public"));   
                            }
                            else
                            {
                                StartCoroutine(DisplayTextTimer($"Player {PlayerNum}, unfortunately that doesn't help you"));      
                            }
                        }
                        else
                        {
                            StartCoroutine(DisplayTextTimer($"Player {PlayerNum}, you did not complete the double"));
                        }
                    }   
                    break;
                case 5:
                    if (!_canGoToPart5){return;}
                    GameManager.Instance.NumOfTurns++;
                    _canInteract = false;
                    if(!_isCPU)
                    {
                        foreach(int[] dominoes in _spareDominoes){_dominoObjects[dominoes].SetActive(false);}
                        foreach(int[] dominoes in _bestPathDominoes){_dominoObjects[dominoes].SetActive(false);}
                    }
                    if (GameManager.Instance.DoubleTrainPlayerNum == _playerNum)
                    {
                        foreach(GameObject train in GameManager.Instance.Trains)
                        {
                            Train trainScript = train.GetComponent<Train>();
                            if (trainScript.PlayerNum != _playerNum)
                            {
                                trainScript.ChangeUsability(false);
                            }
                        }
                    }
                    else if (!_isPublicTrain)
                    {
                        ChangeUsability(false);
                    }
                    _canTakeTurn = false;
                    if (_spareDominoes.Count == 0 && _bestPathDominoes.Count == 0)
                    {
                        GameManager.Instance.PlayerHasWon(_playerNum);
                    }
                    else
                    {
                        GameManager.Instance.IsAtEndOfTurn = true; 
                    }   
                    break;
            }
        }
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CheckIfDominoCanBeAdded();
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Private

    void AddDominoToDeck()
    {
        int[] randDomino = GameManager.Instance.DrawPile[Random.Range(0,GameManager.Instance.DrawPile.Count)];
        GameObject dominoCopy = Instantiate(_domino);
        GameObject firstNumber = Instantiate(GameManager.Instance.Numbers[randDomino[0]],dominoCopy.transform);
        GameObject secondNumber = Instantiate(GameManager.Instance.Numbers[randDomino[1]],dominoCopy.transform);
        Domino dominoScript = dominoCopy.GetComponent<Domino>();
        dominoScript.DominoNums = randDomino;
        dominoScript.TrainScript = this;
        firstNumber.transform.localPosition = new(0,0.25f,0);
        secondNumber.transform.localPosition = new(0,-0.25f,0);
        _dominoObjects.Add(randDomino,dominoCopy);
        _spareDominoes.Add(randDomino);
        if (!_isCPU)
        {
           dominoCopy.transform.position = new(_deckViewingPos.x,_deckViewingPos.y - 4, 1);
           if (GameManager.Instance.CurrentTurnPlayerNum != _playerNum){dominoCopy.SetActive(false);} 
        }
        else
        {
            dominoCopy.SetActive(false);
        }
        GameManager.Instance.DrawPile.Remove(randDomino);                
    }

    void FindBestPath()
    {
        if(_bestPathDominoes.Count > 0)
        {
            foreach(int[] domino in _bestPathDominoes)
            {
                _spareDominoes.Add(domino);
            }
            _bestPathDominoes.Clear();
        }
        List<int[]> currentDominoPath = new();
        for (int i = 0; i < _spareDominoes.Count; i++)
        {
            FindBestPathRecurLoop(currentDominoPath,_lastPlayedDominoNum);
        }
        foreach(int[] domino in _bestPathDominoes)
        {
            _spareDominoes.Remove(domino);
        }
    }

    void FindBestPathRecurLoop(List<int[]> currentDominoPath, int numTomatch)
    {
        for (int i = 0; i < _spareDominoes.Count; i++)
        {
            if (_spareDominoes[i][0] == numTomatch || _spareDominoes[i][1] == numTomatch)
            {
                if (!currentDominoPath.Contains(_spareDominoes[i]))
                {
                    currentDominoPath.Add(_spareDominoes[i]);
                    int otherNum = _spareDominoes[i][0] == numTomatch ? _spareDominoes[i][1] : _spareDominoes[i][0];
                    FindBestPathRecurLoop(currentDominoPath,otherNum);
                    currentDominoPath.Remove(_spareDominoes[i]);   
                }
            }
        }
        if (currentDominoPath.Count > _bestPathDominoes.Count)
        {
            _bestPathDominoes.Clear();
            foreach(int[] domino in currentDominoPath){_bestPathDominoes.Add(domino);}
        } 
        else if(currentDominoPath.Count == _bestPathDominoes.Count)
        {
            if (IsCurrentPathWorthMoreThanBest(currentDominoPath))
            {
                _bestPathDominoes.Clear();
                foreach(int[] domino in currentDominoPath){_bestPathDominoes.Add(domino);}
            }
        }
    }

    /// <summary>
    /// Checks if a potential new best path has more points than the currrent one, as it is better to have lower numbers on your dominoes if you don't win
    /// </summary>
    bool IsCurrentPathWorthMoreThanBest(List<int[]> currentDominoPath)
    {
        int currentDominoPathTotal = 0;
        int bestDominoPathTotal = 0;
        for(int i = 0; i < _bestPathDominoes.Count; i++)
        {
            if (currentDominoPath[i][0] == 0 && currentDominoPath[i][1] == 0)
            {
                currentDominoPathTotal += 50;
            }
            else
            {
                currentDominoPathTotal += currentDominoPath[i][0] + currentDominoPath[i][1];
            }
            if (_bestPathDominoes[i][0] == 0 && _bestPathDominoes[i][1] == 0)
            {
                bestDominoPathTotal += 50;
            }
            else
            {
                bestDominoPathTotal += _bestPathDominoes[i][0] + _bestPathDominoes[i][1];
            }
        }
        return currentDominoPathTotal > bestDominoPathTotal;
    }

    bool CanPlayerActuallyDoAnything()
    {
        //CPU Players
        if (_isCPU)
        {
            int numToMatch;
            //If there's a double train, the player must add to it
            if (GameManager.Instance.DoubleTrainPlayerNum != 0)
            {
                numToMatch = GameManager.Instance.DoubleTrainPlayerScript.LastPlayedDominoNum;
                foreach(int[] dominoNums in _spareDominoes)
                {
                    if (dominoNums[0] == numToMatch || dominoNums[1] == numToMatch)
                    {
                        GameManager.Instance.DoubleTrainPlayerScript.AddDominoToTrain(_dominoObjects[dominoNums]);
                        return true;
                    }
                }
                foreach(int[] dominoNums in _bestPathDominoes)
                {
                    if (dominoNums[0] == numToMatch || dominoNums[1] == numToMatch)
                    {
                        GameManager.Instance.DoubleTrainPlayerScript.AddDominoToTrain(_dominoObjects[dominoNums]);
                        return true;
                    }
                }
                return false;
            }
            //Are there any public trains the player can add to using spare dominoes
            foreach (GameObject train in GameManager.Instance.Trains)
            {
                Train trainScript = train.GetComponent<Train>();
                if (!trainScript.IsUsable || trainScript.PlayerNum == _playerNum){continue;}
                numToMatch = trainScript.LastPlayedDominoNum;
                foreach (int[] dominoNums in _spareDominoes)
                {
                    if (dominoNums[0] == numToMatch || dominoNums[1] == numToMatch)
                    {
                        trainScript.AddDominoToTrain(_dominoObjects[dominoNums]);
                        return true;
                    }
                }
            }
            //Can the player add to their own train
            foreach (int[] dominoNums in _bestPathDominoes)
            {
                if (dominoNums[0] == _lastPlayedDominoNum || dominoNums[1] == _lastPlayedDominoNum)
                {
                    AddDominoToTrain(_dominoObjects[dominoNums]);
                    return true;
                }
            }
            foreach (int[] dominoNums in _spareDominoes)
            {
                if (dominoNums[0] == _lastPlayedDominoNum || dominoNums[1] == _lastPlayedDominoNum)
                {
                    AddDominoToTrain(_dominoObjects[dominoNums]);
                    return true;
                }
            }
            return false;
        }
        //Human Players
        foreach(GameObject train in GameManager.Instance.Trains)
        {
            Train trainScript = train.GetComponent<Train>();
            if (!trainScript.IsUsable || !DoesPlayerHaveAMatchingDomino(trainScript.LastPlayedDominoNum)){continue;}
            _currentTurnPart = 0;
            return true;
        }
        return false;
    }

    bool DoesPlayerHaveAMatchingDomino(int numToMatch)
    {
        foreach(int[] dominoNums in _spareDominoes)
        {
            if (dominoNums[0] == numToMatch|| dominoNums[1] == numToMatch)
            {
                return true;
            }
        }
        foreach(int[] dominoNums in _bestPathDominoes)
        {
            if (dominoNums[0] == numToMatch|| dominoNums[1] == numToMatch)
            {
                return true;
            }
        }
        return false;
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Public

    public void GenerateStartingDominoes(int numOfPlayers)
    {
        int numOfDominoes;
        if (numOfPlayers >= 2 && numOfPlayers <= 4){numOfDominoes = 15;}
        else if (numOfPlayers == 5 || numOfPlayers == 6){numOfDominoes = 12;}
        else{numOfDominoes = 11;}
        do{AddDominoToDeck();} while (_spareDominoes.Count < numOfDominoes);
        if (!_isCPU)
        {
            float spawnX = _deckViewingPos.x;
            float spawnY = _deckViewingPos.y;
            foreach (int[] domino in _spareDominoes)
            {
                _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
                spawnX += 0.5f;
                if (spawnX > 5){spawnX = -5;spawnY -= 2;}
            }     
        }
        FindBestPath();
    }

    public void TakeTurn()
    {
        _currentTurnPart = 1; 
        _showBestPathButton.onClick.RemoveAllListeners();
        _showBestPathButton.onClick.AddListener(ShowBestPath);
        _canTakeTurn = true;
    }

    /// <summary>
    /// Show the best path of dominoes for the player's train, with the best ones on top and the spare ones below
    /// </summary>
    public void ShowBestPath() //Background of button text learned from here: https://youtu.be/DtYAfmsoCxg
    {
        if (GameManager.Instance.ClickedDomino != null)
        {
            GameManager.Instance.ClickedDomino.GetComponent<Domino>().DeselectDomino();
        }
        float spawnX = _deckViewingPos.x;
        float spawnY = _deckViewingPos.y;
        foreach (int[] domino in _bestPathDominoes)
        {
            _dominoObjects[domino].SetActive(true);
            _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
            spawnX += 0.5f;
            if (spawnX > 5){spawnX = -5;spawnY -= 2;}
        }
        spawnX = -5;
        spawnY -= 2;
        foreach (int[] domino in _spareDominoes)
        {
            _dominoObjects[domino].SetActive(true);
            _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
            spawnX += 0.5f;
            if (spawnX > 5){spawnX = -5;spawnY -= 2;}
        }
    }

    public void AddDominoToDeckButton()
    {
        AddDominoToDeck();
        FindBestPath();
    }

    public void AddDominoToTrain(GameObject dominoToAdd)
    {
        dominoToAdd.SetActive(true);
        Domino dominoScript = dominoToAdd.GetComponent<Domino>();
        Transform dominoTransform = dominoToAdd.transform;
        dominoTransform.parent = transform;
        dominoTransform.SetPositionAndRotation(transform.position,transform.rotation);
        if (_trainSize == 0)
        {
            dominoTransform.position = transform.position;
        }
        else
        {
            dominoTransform.localPosition = _trainSize % 2 == 0 ? _lastPlayedDominoPos + _leftPosVector : _lastPlayedDominoPos + _rightPosVector;
        }
        dominoScript.Placed(_playerNum == GameManager.Instance.MexicanTrainPlayerNum);
        int[] dominoNums = dominoScript.DominoNums;
        GameManager.Instance.ChangeDominoNumsAmounts(dominoNums[0]);
        if (dominoNums[0] != dominoNums[1])
        {
            //If it's not a double domino
            GameManager.Instance.ChangeDominoNumsAmounts(dominoNums[1]);
        }
        if(dominoNums[1] == _lastPlayedDominoNum)
        {
            //Need to rotate the domino if the right number is not on top
            dominoTransform.localRotation = new Quaternion(dominoTransform.localRotation.x,dominoTransform.localRotation.x,dominoTransform.localRotation.z + 180,dominoTransform.localRotation.w);
        }
        _trainSize++;
        _lastPlayedDominoPos = dominoTransform.localPosition;
        if (IsPublicTrain && GameManager.Instance.CurrentTurnPlayerNum == _playerNum)
        {
            //Player's train can be made private because they placed something on it 
            _isPublicTrain = false;
            foreach(GameObject train in GameManager.Instance.Trains)
            {
                Train trainScript = train.GetComponent<Train>();
                if (trainScript.IsPublicTrain){trainScript.ChangeUsability(true);}
            }
            print("Your train is no longer public");
        }
        if (dominoNums[0] == dominoNums[1])
        {
            //If it's a double domino
            GameManager.Instance.DoubleTrainPlayerNum = _playerNum;
            GameManager.Instance.DoubleTrainPlayerScript = this;
            _lastPlayedDominoNum = dominoNums[0];
            dominoScript.TrainScript.RemoveDominoFromDeck(dominoNums);
            dominoScript.TrainScript = this;
            StartCoroutine(GameManager.Instance.CurrentTurnTrainScript.DisplayTextTimer(
                $"Player {GameManager.Instance.CurrentTurnPlayerNum} has placed a double {dominoNums[0]} on " +
                $"{(_playerNum == GameManager.Instance.CurrentTurnPlayerNum ? "their own train" : _playerNum == GameManager.Instance.NumOfTrains ? "the Mexican Train" : $"Player {_playerNum}'s train")}," +
                "\n giving them another turn" +
                $"{(GameManager.Instance.IsCpuTurn && GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino ? ".\n There were no other proper dominoes available" : "")}"));
            if (GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino)
            {
                GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino = false;   
            };
            GameManager.Instance.CurrentTurnTrainScript.CurrentTurnPart = 1;
            _canInteract = false;
            GameManager.Instance.NumOfTurns++;
        }
        else
        {
            if (GameManager.Instance.DoubleTrainPlayerNum != 0)
            {
                GameManager.Instance.DoubleTrainPlayerNum = 0;
                print("Double completed");
                foreach(GameObject train in GameManager.Instance.Trains)
                {
                    Train trainScript = train.GetComponent<Train>();
                    if (trainScript.IsPublicTrain){trainScript.ChangeUsability(true);}
                }
            }
            if (GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino)
            {
                //Dominoes in this case are never flipped so the right value is always the bottom number
                _lastPlayedDominoNum = dominoNums[1]; 
            }
            else
            {
                _lastPlayedDominoNum = dominoNums[0] == _lastPlayedDominoNum ? dominoNums[1] : dominoNums[0]; 
            } 
            dominoScript.TrainScript.RemoveDominoFromDeck(dominoNums);
            dominoScript.TrainScript = this;
            StartCoroutine(GameManager.Instance.CurrentTurnTrainScript.DisplayTextTimer(
                $"Player {GameManager.Instance.CurrentTurnPlayerNum} has placed a ({dominoNums[0]},{dominoNums[1]}) on " + 
                $"{(_playerNum == GameManager.Instance.CurrentTurnPlayerNum ? "their own train" : _playerNum == GameManager.Instance.NumOfTrains ? "the Mexican train" : $"Player {_playerNum}'s train")}" +
                $"{(GameManager.Instance.IsCpuTurn && GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino ? ".\n There were no other proper dominoes available" : "")}"));
            if (GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino)
            {
                GameManager.Instance.CurrentTurnTrainScript.CanAddDiffDomino = false;   
            };
            GameManager.Instance.CurrentTurnTrainScript.CurrentTurnPart = 5;
            _canInteract = false;
        }
    }

    public void CheckIfDominoCanBeAdded()
    {
        GameObject clickedDomino = GameManager.Instance.ClickedDomino;
        if(clickedDomino != null)
        {
            Domino clickedDominoScript = clickedDomino.GetComponent<Domino>();
            if (clickedDominoScript.DominoNums[0] == _lastPlayedDominoNum || clickedDominoScript.DominoNums[1] == _lastPlayedDominoNum || _canAddDiffDomino)
            {
                Train currentTurnTrainScript = GameManager.Instance.CurrentTurnTrainScript;
                if (_isUsable)
                {
                    StopAllCoroutines();
                    _middleScreenTextObject.SetActive(false);
                    AddDominoToTrain(clickedDomino);   
                }
                // else if(GameManager.Instance.DoubleTrainPlayerNum != 0)
                // {
                //     StopAllCoroutines();
                //     StartCoroutine(DisplayTextTimer("You must complete the double first"));
                //     print("You must complete the double first");
                // }
                else if(currentTurnTrainScript.PlayerNum != _playerNum && currentTurnTrainScript.IsPublicTrain)
                {
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer("You must add to your train first"));
                    print("You must add to your train first");
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer("This domino cannot be placed here"));    
                    print("This domino cannot be placed here");
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(DisplayTextTimer("This domino cannot be placed here")); 
                print("This domino cannot be placed here");
            }
        }

    }

    public void RemoveDominoFromDeck(int[] dominoNums)
    {
        if (_bestPathDominoes.Contains(dominoNums))
        {
            if (dominoNums != _bestPathDominoes[0])
            {
                _bestPathDominoes.Remove(dominoNums);
                FindBestPath();
            }
            else //If you remove the first item in the best path, it does not need to be recalculated
            {
                _bestPathDominoes.Remove(dominoNums);
            }
        }
        else
        {
            _spareDominoes.Remove(dominoNums);
        }
        _dominoObjects[dominoNums].SetActive(true);
        _dominoObjects.Remove(dominoNums);
    }

    public void ChangeUsability(bool isUsable)
    {
        _isUsable = isUsable; 
        _trainIndicatorRenderer.color = isUsable ? Color.green : Color.grey;
    }

    public int CalculateFinalScore()
    {
        int total = 0;
        foreach(int[] dominoNums in _spareDominoes)
        {
            total += (dominoNums[0] == 0 && dominoNums[1] == 0) ? 50 : dominoNums[0] + dominoNums[1];
        }
        foreach(int[] dominoNums in _bestPathDominoes)
        {
            total += (dominoNums[0] == 0 && dominoNums[1] == 0) ? 50 : dominoNums[0] + dominoNums[1];
        }
        return total;
    }

    public IEnumerator DisplayTextTimer(string textToDisplay)
    {
        _canGoToPart1 = _canGoToPart2 = _canGoToPart3 = _canGoToPart4 = _canGoToPart5 = false;
        _middleScreenText.text = textToDisplay;
        _middleScreenTextObject.SetActive(true);
        yield return gameDelay;
        _middleScreenTextObject.SetActive(false);
        _canGoToPart1 = _canGoToPart2 = _canGoToPart3 = _canGoToPart4 = _canGoToPart5 =_canInteract = true;
    }
}
