using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    List<int[]> _bestDominoPath;
    List<int[]> _drawPile;
    List<int[]> _spareDominoes;
    Dictionary<int[],GameObject> _dominoObjects;
    Train _trainScript;
    [SerializeField] GameObject _train;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject[] _numbers;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _drawPile = new();
        _bestDominoPath = new();
        _spareDominoes = new();
        _dominoObjects = new();
    } 
    void Start()
    {
        _trainScript = _train.GetComponent<Train>();
        GenerateDrawPile();
        GeneratePlayerDominoes();
        FindBestPath();
        print("Player dominoes");
        for(int i=0; i < 15; i++)
        {
            print($"{_spareDominoes[i][0]},{_spareDominoes[i][1]}");
        }
        print("Best domino path");
        foreach(int[] domino in _bestDominoPath)
        {
            print($"{domino[0]},{domino[1]}");
            _spareDominoes.Remove(domino);
            _trainScript.AddDominoToTrain(_dominoObjects[domino],domino);
            _dominoObjects.Remove(domino);
        }
        print($"Spare domino total: {_spareDominoes.Count}");
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateDrawPile()
    {
        for(int i=0; i < _numbers.Length; i++)
        {
            for (int k=i; k < _numbers.Length; k++)
            {
                int[] numCombination = {i,k};
                _drawPile.Add(numCombination);
            }
        }
    }

    void GeneratePlayerDominoes()
    {
        for(int i=0; i < 15; i++)
        {
            int[] randDomino = _drawPile[Random.Range(0,_drawPile.Count)];
            _drawPile.Remove(randDomino);
            _spareDominoes.Add(randDomino);
            GameObject dominoCopy = Instantiate(_domino);
            GameObject firstNumber = Instantiate(_numbers[randDomino[0]],dominoCopy.transform);
            GameObject secondNumber = Instantiate(_numbers[randDomino[1]],dominoCopy.transform);
            firstNumber.transform.localPosition = new(0,0.25f,0);
            secondNumber.transform.localPosition = new(0,-0.25f,0);
            _dominoObjects.Add(randDomino,dominoCopy);
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
            FindBestPathRecurLoop(currentDominoPath,_trainScript.GetLastPlayedDominoNum());
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
}
