using UnityEngine;
using UnityEngine.SceneManagement;

public class Ball : MonoBehaviour
{
    [SerializeField] private GameObject ballParticles;
    private GameMaster gameMaster;
    private Camera mainCam;
    private float rightBoundary, leftBoundary;
    private bool missable;//If the player missed this orb, should he lose a miss?
    private bool inGameScene;
    public float speed { set; get; }

    //Tutorial variables
    private bool inTutorial;
    private Vector2 targetPos;
    //Tutorial variables

    public GameObject BallParticles()
    {
        return ballParticles;
    }
    void Start()
    {   
        inGameScene = false;
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            gameMaster = GameMaster.GetGameMaster();
            inGameScene = true;
        }
        inTutorial = false;
        mainCam = Camera.main;
        rightBoundary = mainCam.transform.position.x + mainCam.orthographicSize;
        leftBoundary = mainCam.transform.position.x - mainCam.orthographicSize;
        missable = false;
        if (gameObject.CompareTag("Ball"))
            missable = true;
        InvokeRepeating("IsOutside", 0, 0.5f);
    }
    void Update()
    {
        if (inTutorial && Vector2.Distance(targetPos, transform.position) < 0.1f)
            transform.position = Vector2.Lerp(transform.position, targetPos, speed * Time.deltaTime);
        else transform.position += new Vector3(speed * Time.deltaTime, 0);
    }

    public void SetTutorial(float commonx)
    {
        targetPos = new Vector2(commonx, transform.position.y);
        inTutorial = true;
    }

    public void TerminateTutorial()
    {
        inTutorial = false;
    }

    private void IsOutside()
    {
        if ((speed > 0 && transform.position.x > rightBoundary) || 
            (speed < 0 && transform.position.x < leftBoundary))
        {
            speed = 0;
            if (missable && inGameScene)
                gameMaster.MissHandler();
        }
    }

}
