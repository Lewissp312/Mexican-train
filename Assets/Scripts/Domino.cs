using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Domino : MonoBehaviour, IPointerClickHandler 
//Detecting mouse clicks: https://discussions.unity.com/t/solved-detecting-mouse-click-on-an-object-in-2d-game/668634 
//Detecting mouse position: https://discussions.unity.com/t/mouse-position-with-new-input-system/776798/19
{
    bool _isOnTrain;
    public bool IsOnTrain{set => _isOnTrain = value;}
    bool isSelected;
    int[] _dominoNums;
    public int[] DominoNums{get => _dominoNums;set => _dominoNums = value;} 
    Train _trainScript;
    public Train TrainScript{get => _trainScript; set => _trainScript = value;} 
    SortingGroup sortingLayer;
    CameraScript cameraScript;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Unity functions

    void Awake()
    {
        sortingLayer = GetComponent<SortingGroup>();
        cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
    }

    void Update()
    {
        if (isSelected && transform.parent == null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 trueMousePos = new(mousePos.x,mousePos.y,1);
            transform.position = trueMousePos;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.CurrentTurnTrainScript.CanInteract){return;}
        if (!_isOnTrain && GameManager.Instance.ClickedDomino == null)
        {
            isSelected = true;
            transform.rotation = new(0,0,0,0);
            sortingLayer.enabled = true;
            GameManager.Instance.ClickedDomino = gameObject;
        }
        else if (_isOnTrain)
        {
            _trainScript.CheckIfDominoCanBeAdded();
        }
        else if(isSelected && transform.position.y < -50)
        {
            //Dominoes can only be deselected in the deck
            DeselectDomino();
        }   
        
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Public 

    public void DeselectDomino()
    {
        isSelected = false;
        sortingLayer.enabled = false;
        GameManager.Instance.ClickedDomino = null;
    }

    public void Placed(bool isOnMexicanTrain)
    {
        DeselectDomino();
        _isOnTrain = true;
        cameraScript.CheckForBoundsUpdate(transform.position.x,transform.position.y, isOnMexicanTrain);
    }
}
