using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

// [RequireComponent(typeof(SortingGroup))]
public class Domino : MonoBehaviour, IPointerClickHandler //Detecting mouse clicks: https://discussions.unity.com/t/solved-detecting-mouse-click-on-an-object-in-2d-game/668634
{
    bool isSelected;
    InputAction _click;
    SortingGroup sortingLayer;
    int[] _dominoNums;
    public int[] DominoNums
    {
        get => _dominoNums;
        set => _dominoNums = value; 
    } 
    GameObject _train;
    // public GameObject Train
    // {
    //     get => _train;
    //     set
    //     {
    //         _train = value;
    //         _trainScript = _train.GetComponent<Train>();
    //     }
    // } 
    Train _trainScript;

    void Awake()
    {
        sortingLayer = GetComponent<SortingGroup>();
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.ClickedDomino == null)
        {
            print("Worked");
            print(_dominoNums);
            // transform.parent = null;
            isSelected = true;
            transform.rotation = new(0,0,0,0);
            sortingLayer.enabled = true;
            GameManager.Instance.ClickedDomino = gameObject;
        }
        else if (_train != null)
        {
            _trainScript.CheckIfDominoCanBeAdded();
        }
        else if(GameManager.Instance.ClickedDomino == gameObject && transform.position.y < -50)
        {
            // transform.parent = null;
            isSelected = false;
            sortingLayer.enabled = false;
            GameManager.Instance.ClickedDomino = null;
        }
    }

    public void DeselectDomino()
    {
        isSelected = false;
        sortingLayer.enabled = false;
        GameManager.Instance.ClickedDomino = null;
    }

    public void Placed(GameObject train)
    {
        isSelected = false;
        sortingLayer.enabled = false;
        GameManager.Instance.ClickedDomino = null;
        _train = train;
        _trainScript = _train.GetComponent<Train>();
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
