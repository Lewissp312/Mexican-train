using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // List<int[]> _bestDominoPath;
    List<int[]> _drawPile;
    // List<int[]> _spareDominoes;
    // Dictionary<int[],GameObject> _dominoObjects;
    // Train _trainScript;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject[] _numbers;
    [SerializeField] GameObject[] trains;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _drawPile = new();
        GenerateDrawPile();
    } 
    void Start()
    {
        foreach(GameObject train in trains)
        {
            train.GetComponent<Train>().GeneratePlayerDominoes(_drawPile,_numbers);
        }

        // _trainScript = _train.GetComponent<Train>();
        // print("Player dominoes");
        // for(int i=0; i < 15; i++)
        // {
        //     print($"{_spareDominoes[i][0]},{_spareDominoes[i][1]}");
        // }
        // print("Best domino path");
        // foreach(int[] domino in _bestDominoPath)
        // {
        //     print($"{domino[0]},{domino[1]}");
        //     _spareDominoes.Remove(domino);
        //     _dominoObjects[domino].SetActive(true);
        //     _trainScript.AddDominoToTrain(_dominoObjects[domino],domino);
        //     _dominoObjects.Remove(domino);
        // }
        // print($"Spare domino total: {_spareDominoes.Count}");
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
        print($"Draw pile total: {_drawPile.Count}");
    }
}
