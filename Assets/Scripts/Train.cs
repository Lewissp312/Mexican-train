using UnityEngine;

public class Train : MonoBehaviour
{
    int trainSize;
    int _lastPlayedDominoNum;
    Vector3 _lastPlayedDominoPos;
    Vector3 _rightPosVector = new(0.42f,-0.5f,0);
    Vector3 _leftPosVector = new(-0.42f,-0.5f,0);
    void Awake()
    {
        _lastPlayedDominoNum = Random.Range(0,13);
    } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddDominoToTrain(GameObject domino, int[] dominoNums)
    {
        Transform dominoTransform = domino.transform;
        dominoTransform.parent = transform;
        dominoTransform.rotation = transform.rotation;
        dominoTransform.position = transform.position;
        //TODO: put check for doubles here
        if (trainSize == 0)
        {
            dominoTransform.position = transform.position;
        }
        else
        {
            dominoTransform.localPosition = trainSize % 2 != 0 ? _lastPlayedDominoPos + _rightPosVector : _lastPlayedDominoPos + _leftPosVector;
            //left
        }
        if(dominoNums[1] == _lastPlayedDominoNum)
        {
            dominoTransform.localRotation = new Quaternion(dominoTransform.localRotation.x,dominoTransform.localRotation.x,dominoTransform.localRotation.z + 180,dominoTransform.localRotation.w);
        }
        trainSize++;
        _lastPlayedDominoPos = dominoTransform.localPosition;
        _lastPlayedDominoNum = dominoNums[0] == _lastPlayedDominoNum ? dominoNums[1] : dominoNums[0];
        //get lastplayeddominonum here
    }

    public int GetLastPlayedDominoNum()
    {
        return _lastPlayedDominoNum;
    }
}
