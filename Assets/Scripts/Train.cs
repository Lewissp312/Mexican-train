using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Train : MonoBehaviour, IPointerClickHandler
{
    int trainSize;
    int _lastPlayedDominoNum;
    Vector3 _lastPlayedDominoPos;
    readonly Vector3 _rightPosVector = new(0.42f,-0.6f,0);
    readonly Vector3 _leftPosVector = new(-0.42f,-0.6f,0);
    readonly Vector3 _deckViewingPos = new(-5,-53,0);
    Dictionary<int[],GameObject> _dominoObjects;
    List<int[]> _bestDominoPath;
    List<int[]> _spareDominoes;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject _cannotPlaceDominoText;

    void Awake()
    {
        //TODO: Add switch here for deciding deckviewingpos based on player number
        _lastPlayedDominoNum = 12;
        _bestDominoPath = new();
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

    public void GeneratePlayerDominoes(List<int[]> drawPile,GameObject[] numbers)
    { //add the starting num here if you want to control it in game manager (currently always set to the double twelve)
      //TODO: Adjust the number of dominoes the player gets based on how many players there are
        do
        {
            int[] randDomino = drawPile[Random.Range(0,drawPile.Count)];
            if (!_spareDominoes.Contains(randDomino))
            {
                GameObject dominoCopy = Instantiate(_domino);
                GameObject firstNumber = Instantiate(numbers[randDomino[0]],dominoCopy.transform);
                GameObject secondNumber = Instantiate(numbers[randDomino[1]],dominoCopy.transform);
                dominoCopy.GetComponent<Domino>().DominoNums = randDomino;
                firstNumber.transform.localPosition = new(0,0.25f,0);
                secondNumber.transform.localPosition = new(0,-0.25f,0);
                dominoCopy.SetActive(false);
                _dominoObjects.Add(randDomino,dominoCopy);
                _spareDominoes.Add(randDomino);                
            }
        } while (_spareDominoes.Count < 11);
        foreach(int[] domino in _spareDominoes){drawPile.Remove(domino);}
        float spawnX = _deckViewingPos.x;
        float spawnY = _deckViewingPos.y;
        foreach (int[] domino in _spareDominoes)
        {
            _dominoObjects[domino].SetActive(true);
            _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
            spawnX += 0.5f;
            if (spawnX > 5){spawnX = -5;spawnY -= 2;}
        }
        // foreach (int[] domino in _spareDominoes)
        // {
        //     _dominoObjects[domino].SetActive(true);
        //     _dominoObjects[domino].transform.position = new(spawnX, spawnY, 0);
        //     spawnX += 0.5f;
        // }   
        FindBestPath();
        print("Found best path");
        // foreach(int[] domino in _bestDominoPath){AddDominoToTrain(domino);}
        // _bestDominoPath.Clear();
    }

    public void ShowBestPath() //Background of button text learned from here: https://youtu.be/DtYAfmsoCxg
    {
        if (GameManager.Instance.ClickedDomino != null)
        {
            GameManager.Instance.ClickedDomino.GetComponent<Domino>().DeselectDomino();
        }
        float spawnX = _deckViewingPos.x;
        float spawnY = _deckViewingPos.y;
        foreach (int[] domino in _bestDominoPath)
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

    void FindBestPath()
    {
        if(_bestDominoPath.Count > 0)
        {
            foreach(int[] domino in _bestDominoPath)
            {
                _spareDominoes.Add(domino);
            }
            _bestDominoPath.Clear();
        }
        List<int[]> currentDominoPath = new();
        for (int i = 0; i < _spareDominoes.Count; i++)
        {
            FindBestPathRecurLoop(currentDominoPath,_lastPlayedDominoNum);
        }
        foreach(int[] domino in _bestDominoPath)
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
        if (currentDominoPath.Count > _bestDominoPath.Count)
        {
            _bestDominoPath.Clear();
            foreach(int[] domino in currentDominoPath)
            {
                _bestDominoPath.Add(domino);
            }
        } else if(currentDominoPath.Count == _bestDominoPath.Count)
        {
            if (IsCurrentPathWorthMoreThanBest(currentDominoPath))
            {
                _bestDominoPath.Clear();
                foreach(int[] domino in currentDominoPath)
                {
                    _bestDominoPath.Add(domino);
                }
            }
        }
    }

    bool IsCurrentPathWorthMoreThanBest(List<int[]> currentDominoPath)
    {
        int currentDominoPathTotal = 0;
        int bestDominoPathTotal = 0;
        for(int i = 0; i < _bestDominoPath.Count; i++)
        {
            if (currentDominoPath[i][0] == 0 && currentDominoPath[i][1] == 0)
            {
                currentDominoPathTotal += 50;
            }
            else
            {
                currentDominoPathTotal += currentDominoPath[i][0] + currentDominoPath[i][1];
            }
            if (_bestDominoPath[i][0] == 0 && _bestDominoPath[i][1] == 0)
            {
                bestDominoPathTotal += 50;
            }
            else
            {
                bestDominoPathTotal += _bestDominoPath[i][0] + _bestDominoPath[i][1];
            }
        }
        return currentDominoPathTotal > bestDominoPathTotal;
    }

    public void AddDominoToTrain(int[] dominoNums)
    {
        Transform dominoTransform = _dominoObjects[dominoNums].transform;
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
        if(dominoNums[1] == _lastPlayedDominoNum)
        {
            dominoTransform.localRotation = new Quaternion(dominoTransform.localRotation.x,dominoTransform.localRotation.x,dominoTransform.localRotation.z + 180,dominoTransform.localRotation.w);
        }
        trainSize++;
        _lastPlayedDominoPos = dominoTransform.localPosition;
        _lastPlayedDominoNum = dominoNums[0] == _lastPlayedDominoNum ? dominoNums[1] : dominoNums[0];
        if (_bestDominoPath.Contains(dominoNums))
        {
            _bestDominoPath.Remove(dominoNums);
        }
        else
        {
            _spareDominoes.Remove(dominoNums);
        }
        _dominoObjects[dominoNums].SetActive(true);
        _dominoObjects.Remove(dominoNums);
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
            if (clickedDominoScript.DominoNums[0] != _lastPlayedDominoNum && clickedDominoScript.DominoNums[1] != _lastPlayedDominoNum)
            {
                StopAllCoroutines();
                StartCoroutine(CannotPlaceDominoTextTimer(clickedDomino.transform.position));
            }
            else
            {
                clickedDominoScript.Placed(gameObject);
                AddDominoToTrain(clickedDominoScript.DominoNums);
            }
        }
    }

    IEnumerator CannotPlaceDominoTextTimer(Vector3 dominoPos)
    {
        //TODO: Remove this text when someone finishes their turn or the game
        // _cannotPlaceDominoText.transform.position = dominoPos;
        _cannotPlaceDominoText.SetActive(true);
        yield return new WaitForSeconds(3);
        _cannotPlaceDominoText.SetActive(false);
    }
}
