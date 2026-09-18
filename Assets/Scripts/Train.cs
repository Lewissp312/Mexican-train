using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Train : MonoBehaviour, IPointerClickHandler
{
    bool _isCPU;
    public bool IsCPU{get => _isCPU; set => _isCPU = value;}
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
    int _trainSize;
    int _lastPlayedDominoNum;
    public int LastPlayedDominoNum{get => _lastPlayedDominoNum;}
    SpriteRenderer _trainIndicatorRenderer;
    Vector3 _lastPlayedDominoPos;
    readonly Vector3 _rightPosVector = new(0.42f,-0.6f,0);
    readonly Vector3 _leftPosVector = new(-0.42f,-0.6f,0);
    readonly Vector3 _deckViewingPos = new(-5,-53,0);
    Dictionary<int[],GameObject> _dominoObjects;
    List<int[]> _bestPathDominoes;
    List<int[]> _spareDominoes;
    [SerializeField] bool _isPublicTrain;
    public bool IsPublicTrain{get => _isPublicTrain;}
    [SerializeField] int _playerNum;
    public int PlayerNum{get => _playerNum;}
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject _trainIndicator;
    [SerializeField] GameObject _middleScreenTextObject;
    [SerializeField] TextMeshProUGUI _middleScreenText;

    void Awake()
    {
        _currentTurnPart = 1;
        _canGoToPart1 = true;
        _lastPlayedDominoNum = 12;
        _bestPathDominoes = new();
        _spareDominoes = new();
        _dominoObjects = new();
        _trainIndicatorRenderer = _trainIndicator.GetComponent<SpriteRenderer>();
        if (_playerNum == 9){_isPublicTrain = true; ChangeUsability(true);}
    } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_canTakeTurn)
        {
            switch (_currentTurnPart)
            {
                case 1:
                    if (_canGoToPart1)
                    {
                        print($"I am {_playerNum} and CPU status is {_isCPU}");
                        StopAllCoroutines();
                        StartCoroutine(DisplayTextTimer($"Player {_playerNum}'s turn!")); 
                        _currentTurnPart = 2;
                        _canInteract = false;  
                    }
                    break;
                case 2:
                    if (_canGoToPart2)
                    {
                        FindBestPath();
                        if (GameManager.Instance.DoubleTrainPlayerNum == 0)
                        {
                            ChangeUsability(true);
                            if (_isPublicTrain)
                            {
                                //If you have something that can be placed on your public train, you must do it
                                if (_bestPathDominoes.Count > 0)
                                {
                                    GameObject[] trains = GameManager.Instance.Trains;
                                    foreach(GameObject train in trains)
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
                        if (!_isCPU && GameManager.Instance.DoubleTrainPlayerNum != _playerNum) //Do not need to do this if it's your second go as it's already been done
                        {
                            foreach(int[] dominoes in _spareDominoes){_dominoObjects[dominoes].SetActive(true);}
                            foreach(int[] dominoes in _bestPathDominoes){_dominoObjects[dominoes].SetActive(true);}
                        }
                        if (!CanPlayerActuallyDoAnything())
                        {
                            StopAllCoroutines();
                            _canInteract = false;
                            if (GameManager.Instance.DrawPile.Count == 0)
                            {
                                StartCoroutine(DisplayTextTimer($"Player {_playerNum}, there are no moves you can make and the domino pile is empty"));
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
                    }
                    break;
                case 3:
                    if (_canGoToPart3)
                    {
                        AddDominoToDeck();
                        StopAllCoroutines();
                        StartCoroutine(DisplayTextTimer($"Player {_playerNum}, ({_spareDominoes[^1][0]},{_spareDominoes[^1][1]}) was added to your deck")); 
                        _currentTurnPart = 4;  
                        _canInteract = false;
                    }
                    break;
                case 4:
                    if (_canGoToPart4)
                    {
                        FindBestPath();
                        if (_bestPathDominoes.Count > 0 && _isPublicTrain && GameManager.Instance.DoubleTrainPlayerNum == 0)
                        {
                            GameObject[] trains = GameManager.Instance.Trains;
                            foreach(GameObject train in trains)
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
                    }
                    break;
                case 5:
                    if (_canGoToPart5)
                    {
                        _canInteract = false;
                        if(!_isCPU)
                        {
                            foreach(int[] dominoes in _spareDominoes){_dominoObjects[dominoes].SetActive(false);}
                            foreach(int[] dominoes in _bestPathDominoes){_dominoObjects[dominoes].SetActive(false);}
                        }
                        if (GameManager.Instance.DoubleTrainPlayerNum == _playerNum)
                        {
                            GameObject[] trains = GameManager.Instance.Trains;
                            foreach(GameObject train in trains)
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
                        if (_spareDominoes.Count == 0 && _bestPathDominoes.Count == 0){GameManager.Instance.PlayerHasWon(_playerNum);}
                        else
                        {
                           _canTakeTurn = false;
                           GameManager.Instance.IsAtEndOfTurn = true; 
                        }   
                    }
                    break;
            }
        }
        
    }

    public void GenerateStartingDominoes()
    { //add the starting num here if you want to control it in game manager (currently always set to the double twelve)
      //TODO: Adjust the number of dominoes the player gets based on how many players there are
        do{AddDominoToDeck();} while (_spareDominoes.Count < 8);
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

    public void TakeTurn(){_currentTurnPart = 1; _canTakeTurn = true;}

    bool CanPlayerActuallyDoAnything()
    {
        GameObject[] trains = GameManager.Instance.Trains;
        //CPU Players
        if (_isCPU)
        {
            int numToMatch;
            //If there's a double train, you must add to it
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
            //Are there any public trains we can add to using spare dominoes
            foreach (GameObject train in trains)
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
            //Can we add to our own train
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
        foreach(GameObject train in trains)
        {
            Train trainScript = train.GetComponent<Train>();
            if (!trainScript.IsUsable || !DoDominoNumsMatch(trainScript.LastPlayedDominoNum)){continue;}
            _currentTurnPart = 0;
            return true;
        }
        return false;
    }

    bool DoDominoNumsMatch(int numToMatch)
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

    void AddDominoToDeck()
    {
        List<int[]> drawPile = GameManager.Instance.DrawPile;
        GameObject[] numbers = GameManager.Instance.Numbers;
        int[] randDomino = drawPile[Random.Range(0,drawPile.Count)];
        GameObject dominoCopy = Instantiate(_domino);
        GameObject firstNumber = Instantiate(numbers[randDomino[0]],dominoCopy.transform);
        GameObject secondNumber = Instantiate(numbers[randDomino[1]],dominoCopy.transform);
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
        drawPile.Remove(randDomino);                
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
        } else if(currentDominoPath.Count == _bestPathDominoes.Count)
        {
            if (IsCurrentPathWorthMoreThanBest(currentDominoPath))
            {
                _bestPathDominoes.Clear();
                foreach(int[] domino in currentDominoPath){_bestPathDominoes.Add(domino);}
            }
        }
    }

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

    public void AddDominoToTrain(GameObject dominoToAdd)
    {
        dominoToAdd.SetActive(true);
        Domino dominoScript = dominoToAdd.GetComponent<Domino>();
        Transform dominoTransform = dominoToAdd.transform;
        dominoTransform.parent = transform;
        dominoTransform.rotation = transform.rotation;
        dominoTransform.position = transform.position;
        if (_trainSize == 0)
        {
            dominoTransform.position = transform.position;
        }
        else
        {
            dominoTransform.localPosition = _trainSize % 2 != 0 ? _lastPlayedDominoPos + _rightPosVector : _lastPlayedDominoPos + _leftPosVector;
        }
        dominoScript.Placed(_playerNum == 9);
        int[] dominoNums = dominoScript.DominoNums;
        if(dominoNums[1] == _lastPlayedDominoNum)
        {
            dominoTransform.localRotation = new Quaternion(dominoTransform.localRotation.x,dominoTransform.localRotation.x,dominoTransform.localRotation.z + 180,dominoTransform.localRotation.w);
        }
        _trainSize++;
        _lastPlayedDominoPos = dominoTransform.localPosition;
        if (IsPublicTrain && GameManager.Instance.CurrentTurnPlayerNum == _playerNum)
        {
            _isPublicTrain = false;
            GameObject[] trains = GameManager.Instance.Trains;
            foreach(GameObject train in trains)
            {
                Train trainScript = train.GetComponent<Train>();
                if (trainScript.IsPublicTrain){trainScript.ChangeUsability(true);}
            }
            print("Your train is no longer public");
            // Thread.Sleep(3000);
        }
        if (dominoNums[0] == dominoNums[1])
        {
            GameManager.Instance.DoubleTrainPlayerNum = _playerNum;
            GameManager.Instance.DoubleTrainPlayerScript = this;
            _lastPlayedDominoNum = dominoNums[0];
            dominoScript.TrainScript.RemoveDominoFromDeck(dominoNums);
            dominoScript.TrainScript = this;
            StartCoroutine(GameManager.Instance.CurrentTurnTrainScript.DisplayTextTimer($"Player {GameManager.Instance.CurrentTurnPlayerNum} has placed a double, giving them a second turn"));
            GameManager.Instance.CurrentTurnTrainScript.CurrentTurnPart = 1;
            _canInteract = false;
        }
        else
        {
            if (GameManager.Instance.DoubleTrainPlayerNum != 0)
            {
                GameManager.Instance.DoubleTrainPlayerNum = 0;
                print("Double completed");
                GameObject[] trains = GameManager.Instance.Trains;
                foreach(GameObject train in trains)
                {
                    Train trainScript = train.GetComponent<Train>();
                    if (trainScript.IsPublicTrain){trainScript.ChangeUsability(true);}
                }
            }
            _lastPlayedDominoNum = dominoNums[0] == _lastPlayedDominoNum ? dominoNums[1] : dominoNums[0]; 
            dominoScript.TrainScript.RemoveDominoFromDeck(dominoNums);
            dominoScript.TrainScript = this;
            if (_playerNum == GameManager.Instance.MexicanTrainScript.PlayerNum)
            {
                StartCoroutine(GameManager.Instance.CurrentTurnTrainScript.DisplayTextTimer($"Player {GameManager.Instance.CurrentTurnPlayerNum} has placed a ({dominoNums[0]},{dominoNums[1]}) on the Mexican train"));
                GameManager.Instance.CurrentTurnTrainScript.CurrentTurnPart = 5;
                _canInteract = false;
            }
            else
            {
                StartCoroutine(GameManager.Instance.CurrentTurnTrainScript.DisplayTextTimer($"Player {GameManager.Instance.CurrentTurnPlayerNum} has placed a ({dominoNums[0]},{dominoNums[1]}) on train {_playerNum}"));
                GameManager.Instance.CurrentTurnTrainScript.CurrentTurnPart = 5;
                _canInteract = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CheckIfDominoCanBeAdded();
    }

    public void CheckIfDominoCanBeAdded()
    {
        //TODO: Add checks for who's turn it is and whether the train is open
        GameObject clickedDomino = GameManager.Instance.ClickedDomino;
        if(clickedDomino != null)
        {
            Domino clickedDominoScript = clickedDomino.GetComponent<Domino>();
            if (clickedDominoScript.DominoNums[0] == _lastPlayedDominoNum || clickedDominoScript.DominoNums[1] == _lastPlayedDominoNum)
            {
                Train currentTurnTrainScript = GameManager.Instance.CurrentTurnTrainScript;
                if (_isUsable)
                {
                    StopAllCoroutines();
                    _middleScreenTextObject.SetActive(false);
                    AddDominoToTrain(clickedDomino);   
                }
                else if(GameManager.Instance.DoubleTrainPlayerNum != 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer("You must complete the double first"));
                    print("You must complete the double first");
                }
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

    public IEnumerator DisplayTextTimer(string textToDisplay)
    {
        //TODO: Remove this text when someone finishes their turn or the game
        // _cannotPlaceDominoText.transform.position = dominoPos;
        // _isEndOfTurn = true;
        _canGoToPart1 = _canGoToPart2 = _canGoToPart3 = _canGoToPart4 = _canGoToPart5 = false;
        _middleScreenText.text = textToDisplay;
        _middleScreenTextObject.SetActive(true);
        yield return new WaitForSeconds(3);
        _middleScreenTextObject.SetActive(false);
        _canGoToPart1 = _canGoToPart2 = _canGoToPart3 = _canGoToPart4 = _canGoToPart5 =_canInteract = true;
        //Interact is true
        // if (_spareDominoes.Count == 0 && _bestPathDominoes.Count == 0){GameManager.Instance.HasPlayerWon = true; print($"{_playerNum}, I won");}
    }
}
