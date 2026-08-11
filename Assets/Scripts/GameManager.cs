using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    InputAction _randomiserButton;
    [SerializeField] GameObject _domino;
    [SerializeField] GameObject[] _numbers;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _randomiserButton = InputSystem.actions.FindAction("Randomise");
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
}
