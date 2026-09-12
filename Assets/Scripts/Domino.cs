using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

// [RequireComponent(typeof(SortingGroup))]
public class Domino : MonoBehaviour, IPointerClickHandler //Detecting mouse clicks: https://discussions.unity.com/t/solved-detecting-mouse-click-on-an-object-in-2d-game/668634
{
    bool isSelected;
    bool _isOnTrain;
    public bool IsOnTrain
    {
        set => _isOnTrain = value;
    }
    InputAction _click;
    SortingGroup sortingLayer;
    int[] _dominoNums;
    public int[] DominoNums
    {
        get => _dominoNums;
        set => _dominoNums = value; 
    } 
    GameObject _train;
    Train _trainScript;
    public Train TrainScript
    {
        get => _trainScript;
        set
        {
            _trainScript = value;    
        }
    } 

    void Awake()
    {
        sortingLayer = GetComponent<SortingGroup>();
    }



    public void OnPointerClick(PointerEventData eventData)
    {
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

    public void Placed()
    {
        DeselectDomino();
        _isOnTrain = true;
        //_trainScript.RemoveDomino(_dominoNums);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _click = InputSystem.actions.FindAction("Click");
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 trueMousePos = new(mousePos.x,mousePos.y,1);
            transform.position = trueMousePos;
        }
        // if (_click.WasPressedThisFrame())
        // {
        //     // print(_dominoNums);
        //     Vector2 mousePos = Mouse.current.position.ReadValue();
        //     // print(Camera.main.ScreenToWorldPoint(mousePos));
        //     // if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit)) 
        //     // {
        //     //     print("Worked");
        //     // }
        // }
    }
}
