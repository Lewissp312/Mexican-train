using System.Collections;
using System.Collections.Generic;
using TMPro;

// using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Train : MonoBehaviour, IPointerClickHandler
{
    bool hasPlacedDouble;
    int trainSize;
    int _lastPlayedDominoNum;
    Vector3 _lastPlayedDominoPos;
    readonly Vector3 _rightPosVector = new(0.42f,-0.6f,0);
    readonly Vector3 _leftPosVector = new(-0.42f,-0.6f,0);
    readonly Vector3 _deckViewingPos = new(-5,-53,0);
    Dictionary<int[],GameObject> _dominoObjects;
    List<int[]> _bestPathDominoes;
    List<int[]> _spareDominoes;
    [SerializeField] bool isPublicTrain;
    [SerializeField] int _playerNum;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject _middleScreenTextObject;
    [SerializeField] TextMeshProUGUI _middleScreenText;

    void Awake()
    {
        _lastPlayedDominoNum = 12;
        _bestPathDominoes = new();
        _spareDominoes = new();
        _dominoObjects = new();
    } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateStartingDominoes()
    { //add the starting num here if you want to control it in game manager (currently always set to the double twelve)
      //TODO: Adjust the number of dominoes the player gets based on how many players there are
        List<int[]> drawPile = GameManager.Instance.DrawPile;
        do
        {
            AddDominoToDeck(drawPile);
            // int[] randDomino = drawPile[Random.Range(0,drawPile.Count)];
            // if (!_spareDominoes.Contains(randDomino))
            // {
            //     GameObject dominoCopy = Instantiate(_domino);
            //     GameObject firstNumber = Instantiate(numbers[randDomino[0]],dominoCopy.transform);
            //     GameObject secondNumber = Instantiate(numbers[randDomino[1]],dominoCopy.transform);
            //     Domino dominoScript = dominoCopy.GetComponent<Domino>();
            //     dominoScript.DominoNums = randDomino;
            //     dominoScript.TrainScript = gameObject.GetComponent<Train>();
            //     firstNumber.transform.localPosition = new(0,0.25f,0);
            //     secondNumber.transform.localPosition = new(0,-0.25f,0);
            //     // dominoCopy.SetActive(false);
            //     _dominoObjects.Add(randDomino,dominoCopy);
            //     _spareDominoes.Add(randDomino);                
            // }
        } while (_spareDominoes.Count < 15);
        // foreach(int[] domino in _spareDominoes){drawPile.Remove(domino);}
        if (_playerNum == 1)
        {
            float spawnX = _deckViewingPos.x;
            float spawnY = _deckViewingPos.y;
            foreach (int[] domino in _spareDominoes)
            {
                _dominoObjects[domino].SetActive(true);
                _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
                spawnX += 0.5f;
                if (spawnX > 5){spawnX = -5;spawnY -= 2;}
            }     
        }
        FindBestPath();
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
        // print("Starting");
        AddDominoToDeck(GameManager.Instance.DrawPile);
        FindBestPath();
        // foreach(int[] dominoes in _spareDominoes)
        // {
        //     _dominoObjects[dominoes].SetActive(true);
        // }
        // foreach(int[] dominoes in _bestPathDominoes)
        // {
        //     _dominoObjects[dominoes].SetActive(true);
        // }
        // print("End");
    }

    void AddDominoToDeck(List<int[]> drawPile)
    {
        if (drawPile.Count == 0)
        {
            print("No more dominoes");
            return;
        }
        GameObject[] numbers = GameManager.Instance.Numbers;
        bool isAlreadyInDeck = true;
        do
        {
            int[] randDomino = drawPile[Random.Range(0,drawPile.Count)];
            if (!_spareDominoes.Contains(randDomino) && !_bestPathDominoes.Contains(randDomino))
            {
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
                dominoCopy.transform.position = new(_deckViewingPos.x,_deckViewingPos.y - 4, 1);
                if (_playerNum == 1)
                {
                    dominoCopy.SetActive(true);
                }
                else
                {
                    dominoCopy.SetActive(false);
                }
                // dominoCopy.SetActive(false);
                isAlreadyInDeck = false;
                drawPile.Remove(randDomino);                
            } 
        } while (isAlreadyInDeck);
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
            foreach(int[] domino in currentDominoPath)
            {
                _bestPathDominoes.Add(domino);
            }
        } else if(currentDominoPath.Count == _bestPathDominoes.Count)
        {
            if (IsCurrentPathWorthMoreThanBest(currentDominoPath))
            {
                _bestPathDominoes.Clear();
                foreach(int[] domino in currentDominoPath)
                {
                    _bestPathDominoes.Add(domino);
                }
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
        Domino dominoScript = dominoToAdd.GetComponent<Domino>();
        dominoScript.Placed();
        Transform dominoTransform = dominoToAdd.transform;
        dominoTransform.parent = transform;
        dominoTransform.rotation = transform.rotation;
        dominoTransform.position = transform.position;
        //TODO: put check for doubles here
        if (trainSize == 0)
        {
            dominoTransform.position = transform.position;
        }
        else
        {
            dominoTransform.localPosition = trainSize % 2 != 0 ? _lastPlayedDominoPos + _rightPosVector : _lastPlayedDominoPos + _leftPosVector;
        }
        int[] dominoNums = dominoScript.DominoNums;
        if(dominoNums[1] == _lastPlayedDominoNum)
        {
            dominoTransform.localRotation = new Quaternion(dominoTransform.localRotation.x,dominoTransform.localRotation.x,dominoTransform.localRotation.z + 180,dominoTransform.localRotation.w);
        }
        trainSize++;
        _lastPlayedDominoPos = dominoTransform.localPosition;
        if (dominoNums[0] == dominoNums[1])
        {
            hasPlacedDouble = true;
            GameManager.Instance.DoubleTrain = _playerNum;
            _lastPlayedDominoNum = dominoNums[0];
        }
        else
        {
            hasPlacedDouble = false;
            GameManager.Instance.DoubleTrain = 0; 
            _lastPlayedDominoNum = dominoNums[0] == _lastPlayedDominoNum ? dominoNums[1] : dominoNums[0];   
        }
        dominoScript.TrainScript.RemoveDominoFromDeck(dominoNums);
        dominoScript.TrainScript = this;
    }

    public int GetLastPlayedDominoNum()
    {
        return _lastPlayedDominoNum;
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
                if ((isPublicTrain || _playerNum == GameManager.Instance.CurrentTurn) 
                && (GameManager.Instance.DoubleTrain == 0 || GameManager.Instance.DoubleTrain == _playerNum))
                {
                    StopAllCoroutines();
                    _middleScreenTextObject.SetActive(false);
                    // clickedDominoScript.Placed();
                    AddDominoToTrain(clickedDomino);   
                }
                else if(GameManager.Instance.DoubleTrain != 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer("You must complete the double first"));
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(DisplayTextTimer("This domino cannot be placed here"));    
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(DisplayTextTimer("This domino cannot be placed here")); 
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

    IEnumerator DisplayTextTimer(string textToDisplay)
    {
        //TODO: Remove this text when someone finishes their turn or the game
        // _cannotPlaceDominoText.transform.position = dominoPos;
        _middleScreenText.text = textToDisplay;
        _middleScreenTextObject.SetActive(true);
        yield return new WaitForSeconds(3);
        _middleScreenTextObject.SetActive(false);
    }
}
