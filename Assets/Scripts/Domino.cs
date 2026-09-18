using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

// [RequireComponent(typeof(SortingGroup))]
public class Domino : MonoBehaviour, IPointerClickHandler //Detecting mouse clicks: https://discussions.unity.com/t/solved-detecting-mouse-click-on-an-object-in-2d-game/668634
{
    bool isSelected;
    bool _isOnTrain;
    public bool IsOnTrain{set => _isOnTrain = value;}
    InputAction _click;
    SortingGroup sortingLayer;
    int[] _dominoNums;
    public int[] DominoNums
    {get => _dominoNums;set => _dominoNums = value; } 
    CameraScript cameraScript;
    Train _trainScript;
    public Train TrainScript{get => _trainScript; set => _trainScript = value;} 


    void Awake()
    {
        sortingLayer = GetComponent<SortingGroup>();
        cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
        _click = InputSystem.actions.FindAction("Click");
    }

    void Start()
    {
        // Camera.main.gameObject.GetComponent<CameraScript>();
        // cameraScript = Camera.main.gameObject.GetComponent<CameraScript>();
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
            DeselectDomino();
        }   
        
    }

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
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
}
