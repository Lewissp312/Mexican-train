using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    bool _isViewingDeck;
    public bool IsViewingDeck{get => _isViewingDeck;}
    bool _isDragging;
    bool _isViewingGame;
    bool _isViewingMexicanTrain;
    float _gameViewYBound;
    float _gameViewXBound;
    float _mexicanTrainViewLowerYBound;
    readonly float _mexicanTrainViewUpperYBound = 60;
    readonly float _mexicanTrainViewXBound = 110;
    InputAction _click;
    InputAction _gameViewButton;
    InputAction _deckViewButton;
    InputAction _mexicanTrainViewButton;
    Vector3 _origin;
    Vector3 _difference;
    Vector3 _gameViewPos;
    Vector3 _mexicanTrainViewPos;
    readonly Vector3 _deckViewingPos = new(0,-55f,-1);
    [SerializeField] TextMeshProUGUI _deckText; 
    [SerializeField] GameObject _showBestPathButton; 
    [SerializeField] GameObject _addDominoToDeckButton;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Unity functions


    void Awake()
    {
        _gameViewYBound = 3.5f;
        _gameViewXBound = 7.5f;
        _mexicanTrainViewLowerYBound = 60;
        _click = InputSystem.actions.FindAction("Click");
        _gameViewButton = InputSystem.actions.FindAction("Game View");
        _deckViewButton = InputSystem.actions.FindAction("Deck View");
        _mexicanTrainViewButton = InputSystem.actions.FindAction("Mexican Train View");
        _gameViewPos = new(0,0,-1);
        _mexicanTrainViewPos = new(110,60,-1);
    }

    void Update() 
    { //Camera code apadted from here: https://youtu.be/H7pjj1K91HE
        if (!GameManager.Instance.IsGameActive){return;}
        if (_click.WasPressedThisFrame())
        {
            _isDragging = true;
            _origin = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue());
        } 
        else if (_click.WasReleasedThisFrame())
        {
            _isDragging = false;
        }    
        else if(_gameViewButton.WasPressedThisFrame() && !_isViewingGame)
        {
            ActivateGameView();
        }
        else if (_deckViewButton.WasPressedThisFrame() && !_isViewingDeck && !GameManager.Instance.IsCpuTurn)
        {
            ActivateDeckView();
        }
        else if(_mexicanTrainViewButton.WasPressedThisFrame() && !_isViewingMexicanTrain)
        {
            ActivateMexicanTrainView();
        }
        
    }

    void LateUpdate()
    {
        if ( !GameManager.Instance.IsGameActive || !_isDragging || _isViewingDeck){return;}
        _difference = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue()) - transform.position;
        Vector3 newPos = _origin - _difference; 
        UpdatePosition(newPos);
        
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Private

    void ActivateDeckView()
    {
        if(_isViewingGame)
        {
            _gameViewPos = transform.position;
            _isViewingGame = false;
        }
        else
        {
            _mexicanTrainViewPos = transform.position;
            _isViewingMexicanTrain = false;
        }
        transform.position = _deckViewingPos;
        _deckText.enabled = true;
        _showBestPathButton.SetActive(true);
        // _addDominoToDeckButton.SetActive(true);    
        _isViewingDeck = true; 
    }

    void ActivateMexicanTrainView()
    {
        if(_isViewingGame)
        {
            _gameViewPos = transform.position;
            _isViewingGame = false;
        }
        else
        {
            _deckText.enabled = false;
            _showBestPathButton.SetActive(false);
            _addDominoToDeckButton.SetActive(false);    
            _isViewingDeck = false; 
        }
        transform.position = _mexicanTrainViewPos;
        _isViewingMexicanTrain = true;
    }

    void UpdatePosition(Vector3 newPos)
    {
        float newX = newPos.x;
        float newY = newPos.y;
        if (_isViewingGame)
        {
            if(newX > _gameViewXBound)
            {
                newX = _gameViewXBound;
            }
            else if(newX < -_gameViewXBound)
            {
                newX = -_gameViewXBound;
            }
            if(newY > _gameViewYBound)
            {
                newY = _gameViewYBound;
            }
            else if(newY < -_gameViewYBound)
            {
                newY = -_gameViewYBound;
            }
        }
        else
        {
            newX = _mexicanTrainViewXBound;
            if(newY > _mexicanTrainViewUpperYBound)
            {
                newY = _mexicanTrainViewUpperYBound;
            }
            else if(newY < _mexicanTrainViewLowerYBound)
            {
                newY = _mexicanTrainViewLowerYBound;
            }
            
        }
        transform.position = new(newX,newY,-1); 
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Public

    public void ActivateGameView()
    {
        //Public because it is used by gameManager at the start of the game
        if(_isViewingDeck)
        {
            _deckText.enabled = false;
            _showBestPathButton.SetActive(false);
            _addDominoToDeckButton.SetActive(false);    
            _isViewingDeck = false; 
        }
        else if(_isViewingMexicanTrain)
        {
            _mexicanTrainViewPos = transform.position;
            _isViewingMexicanTrain = false;
        }
        transform.position = _gameViewPos;
        _isViewingGame = true;
    }

    /// <summary>
    /// Checks if a domino has gotten close to or exceeded the camera bounds so that the bounds can be updated
    /// </summary>
    public void CheckForBoundsUpdate(float XPos, float YPos, bool isOnMexicanTrain)
    {
        
        if (isOnMexicanTrain)
        {
            if(Math.Abs(_mexicanTrainViewLowerYBound - YPos) <= 2) 
            {
                _mexicanTrainViewLowerYBound -= 2;
            }
        } 
        else
        {
            if(Math.Abs(_gameViewXBound - XPos) <= 1 || Math.Abs(-_gameViewXBound - XPos) <= 1)
            {
                _gameViewXBound += 1;
            }
            if(Math.Abs(_gameViewYBound - YPos) <= 1 || Math.Abs(-_gameViewYBound - YPos) <= 1)
            {
                _gameViewYBound += 1;
            }
        }
    }
}
