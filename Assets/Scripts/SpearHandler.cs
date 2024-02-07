using System.Collections;
using UnityEngine;

public class SpearHandler : MonoBehaviour
{
    private Transform spearPos;
    private bool canCharge;//If the player can start holding the spear
    public bool canLaunch { get; private set; }//If the player can release the spear
    public bool launched { get; private set; }
    private Rigidbody2D rb;
    public static Vector2 force;
    private Vector3 spearLaunchPos, chargedPos;
    private GameObject GameOverUI;
    private GameMaster master;
    private float speedMultiplier;
    private RectTransform pauseButtonTransform;
    private float ignoreTouchZoneSize;
    private PauseHandler pauseHandler;
    public GameObject tutorialHandler { get; private set; }
    private static SpearHandler spearHandler;
    private bool lookForTutorialObject;
    private delegate IEnumerator SpawnFunc();
    SpawnFunc spawnFunc;

    // Use this for initialization
    void Awake()
    {
        master = GameMaster.GetGameMaster();
        pauseHandler = PauseHandler.GetPauseHandler();
        spearHandler = this;
        ResetBools();
        speedMultiplier = 9f;
        spearPos = transform;
        rb = GetComponent<Rigidbody2D>();
        force = new Vector2(0, 158);
        InitPositions((master._spearInitPos.y + 5));
        GameObject pauseButton = GameObject.Find("PauseButton");
        pauseButtonTransform = pauseButton.GetComponent<RectTransform>();
        ignoreTouchZoneSize = pauseButtonTransform.sizeDelta.y / 2f + 200;
        GameOverUI = GameObject.Find("UIoverlay").transform.Find("GameOver").gameObject;
        lookForTutorialObject = true;
        spawnFunc = master.SpawnWave;
    }

    public static SpearHandler GetSpearHandler()
    {
       if(spearHandler == null)
        {
            Debug.LogError("Panic! No SpearHandler object was found!");
            Application.Quit();
        }
        return spearHandler;
    }

    private void ResetBools()
    {
        canLaunch = false;
        launched = false;
        canCharge = false;
    }

    private void InitPositions(float launchY)
    {
        spearLaunchPos = new Vector3(0f, launchY, 0f);
        chargedPos = new Vector3(0f, launchY - 1f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canCharge && transform.position.y < spearLaunchPos.y)
            transform.position = Vector3.MoveTowards(transform.position,
                spearLaunchPos, 10f * Time.deltaTime);
        else if (!canCharge)//To make sure this code block is only executed once
            canCharge = true;

#if UNITY_EDITOR//If we are playing the game in the editor
        if (canCharge && !launched && Input.GetMouseButton(0)
            && Input.mousePosition.y < (pauseButtonTransform.position.y - ignoreTouchZoneSize)
            && !pauseHandler.paused && GameOverUI != null && !GameOverUI.activeSelf)
        {
            if (!canLaunch && !master.spawningOrbs)
            {
                if (lookForTutorialObject)
                {
                    tutorialHandler = GameObject.Find("TutorialObject");
                    if (tutorialHandler != null)
                    {
                        tutorialHandler.SetActive(false);
                        master.inTutorial = true;
                    }
                    else
                    {
                        master.inTutorial = false;
                        lookForTutorialObject = false;/*If it the tutorial object wasn't found, there is no
                        point in trying to find it in the next frame.*/
                    }
                }
                canLaunch = true;
                StartCoroutine(spawnFunc());
            }
            ChragingEffect();
            spearPos.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                spearPos.position.y, spearPos.position.z);
        }
        else if (canLaunch)
        {
            //Release spear upwards
            launched = true;
            canLaunch = false;
            rb.AddForce(force, ForceMode2D.Impulse);
            StartCoroutine(MyDestroy());
        }
#else
        if (Input.touchCount > 0 && canCharge && !launched 
            && GameOverUI != null && !GameOverUI.activeSelf && !pauseHandler.paused 
            && Input.GetTouch(0).position.y < (pauseButtonTransform.position.y - ignoreTouchZoneSize))
        {
            if (!canLaunch && !master.spawningOrbs)
            {
                if (lookForTutorialObject)
                {
                    tutorialHandler = GameObject.Find("TutorialObject");
                    if (tutorialHandler != null)
                    {
                        tutorialHandler.SetActive(false);
                        master.inTutorial = true;
                    }
                    else
                    {
                        master.inTutorial = false;
                        lookForTutorialObject = false;/*If it the tutorial object wasn't found, there is no
                        point in trying to find it in the next frame.*/
                    }
                }
                canLaunch = true;
                StartCoroutine(spawnFunc());
            }
            ChragingEffect();
            spearPos.position = new Vector3( Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x, 
                spearPos.position.y, spearPos.position.z);
        }
        else if (canLaunch)
        {
            //Release spear upwards
            launched = true;
            canLaunch = false;
            rb.AddForce(force, ForceMode2D.Impulse);
            StartCoroutine(MyDestroy());
        }
#endif
    }
    private IEnumerator MyDestroy()
    { 
        float timeToWait = 1.25f;
        if (master.inTutorial)
        {
            float timeWaited = 0.15f;
            yield return new WaitForSeconds(timeWaited);
            master.inTutorial = false;
            Destroy(tutorialHandler);
            timeToWait -= timeWaited;
        }
        yield return new WaitForSeconds(timeToWait);
        rb.velocity = Vector2.zero;
        yield return StartCoroutine(master.spawnSpear());//Waiting for the function to end
        /*Quality of life improvement: Increase the speed of the orbs the player missed
         in order to speed up the gameplay experience*/
        int i;
        GameObject[] orbs = master.GetOrbsArray();
        for ( i = 0; i < orbs.Length; i++)
            orbs[i].GetComponent<Ball>().speed *= speedMultiplier;
        orbs = master.GetOrbBadOrbs();
        for(i=0;i<orbs.Length;i++)
            orbs[i].GetComponent<Ball>().speed *= speedMultiplier;
        master.getHealthOrb().GetComponent<Ball>().speed *= speedMultiplier;
        ResetBools();
        transform.position = master._spearInitPos;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        Ball ball = col.gameObject.GetComponent<Ball>();
        ball.speed = 0;
        ParticleSystem particles = ball.BallParticles().GetComponent<ParticleSystem>();
        Transform ballTransform = ball.transform;
        particles.transform.position = ballTransform.position;
        particles.Play();
        StartCoroutine(ClearParticles(particles));
        master.HandleDestroyedOrb(ballTransform);
    }

    private IEnumerator ClearParticles(ParticleSystem particles)
    {
        yield return new WaitForSeconds(1f);
        particles.Clear();
    }

    private void ChragingEffect()
    {
        if (transform.position.y > chargedPos.y)
            transform.Translate(0, -2f * Time.deltaTime, 0);
        else transform.Translate(0, 30 * Time.deltaTime, 0);
    }
}
