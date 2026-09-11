using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    InputAction _click;
    InputAction _changeView;
    Vector3 _origin;
    Vector3 _difference;
    Vector3 _gameViewingPos;
    readonly Vector3 _dominoViewingPos = new(0,-55f,-1);
    bool _isDragging;
    bool isViewingDominoes;
    [SerializeField] TextMeshProUGUI _deckText; 
    [SerializeField] GameObject _showBestPathButton; 


    void Awake()
    {
        _click = InputSystem.actions.FindAction("Click");
        _changeView = InputSystem.actions.FindAction("Change View");
        _gameViewingPos = new(0,0,-1);
    }

    void Update()
    { //Camera code apadted from here: https://youtu.be/H7pjj1K91HE
        if (_click.WasPressedThisFrame())
        {
            _isDragging = true;
            _origin = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue());
        } else if (_click.WasReleasedThisFrame())
        {
            _isDragging = false;
        } else if (_changeView.WasPressedThisFrame())
        {
            if (!isViewingDominoes){_gameViewingPos = transform.position;}
            transform.position = isViewingDominoes ? _gameViewingPos : _dominoViewingPos;
            _deckText.enabled = !_deckText.enabled;
            _showBestPathButton.SetActive(!_showBestPathButton.activeSelf);  
            isViewingDominoes = !isViewingDominoes; 
        }
    }

    void LateUpdate()
    {
        if (!_isDragging || isViewingDominoes){return;}
        _difference = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue()) - transform.position; 
        transform.position = _origin - _difference;
    }
}
