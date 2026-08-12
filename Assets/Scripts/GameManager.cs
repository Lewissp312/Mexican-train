using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    List<int[]> _drawPile;
    InputAction _randomiserButton;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject[] _numbers;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _drawPile = new();
        _randomiserButton = InputSystem.actions.FindAction("Randomise");
        GenerateDominoes();
    }

    // Update is called once per frame
    void Update()
    {
        if (_randomiserButton.WasPressedThisFrame())
        {
            if (_domino.transform.childCount != 2)
            {
                Destroy(_domino.transform.GetChild(2).gameObject);
                Destroy(_domino.transform.GetChild(3).gameObject);
            }
            GameObject firstNumber = Instantiate(_numbers[Random.Range(0,_numbers.Length)],_domino.transform);
            GameObject secondNumber = Instantiate(_numbers[Random.Range(0,_numbers.Length)],_domino.transform);
            firstNumber.transform.localPosition = new(0,0.25f,0);
            secondNumber.transform.localPosition = new(0,-0.25f,0);
        }
    }

    void GenerateDominoes()
    {
        int xPos = 0;
        for(int i=0; i < _numbers.Length; i++)
        {
            for (int k=i; k < _numbers.Length; k++)
            {
                int[] numCombination = {i,k};
                _drawPile.Add(numCombination);
                print(numCombination[0]);
                print(numCombination[1]);
                GameObject dominoCopy = Instantiate(_domino);
                dominoCopy.transform.position = new(xPos,0,0);
                xPos += 2;
                GameObject firstNumber = Instantiate(_numbers[i],dominoCopy.transform);
                GameObject secondNumber = Instantiate(_numbers[k],dominoCopy.transform);
                firstNumber.transform.localPosition = new(0,0.25f,0);
                secondNumber.transform.localPosition = new(0,-0.25f,0);
            }
        }
    }
}
