using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MasterParent
{
    [SerializeField] private GameObject[] badOrbs;
    [SerializeField] private GameObject healthOrb;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject[] missSprites;
    [SerializeField] private GameObject GameOverUI;
    private bool canSpawnBadOrb;
    private int misses;
    public int score { get; private set; }
    private Vector3 miniOrbSize;
    private bool updatedSpear = false;/*If we updated the spear's initial position and velocity after
    the orb count was increased to 4.*/
    public Vector3 _spearInitPos { get; private set; }
    private float orbSizeFactor;
    private static GameMaster gameMaster;//Making use of the singleton design pattern
    private int difficultyLevel;
    public bool inTutorial;
    public bool spawningOrbs { get; private set; }
    private float[] minWaitTimes;
    private float[] maxWaitTimes;
    private int maxOrbCount;
    private int prevScore;//Used to determine when it's possible to spawn a health orb.
    


    public static GameMaster GetGameMaster()
    {
        if (gameMaster == null)
        {
            Debug.LogError("Panic! No GameMaster object was found!");
            Application.Quit();
        }
        return gameMaster;
    }
    /*Every even index holds a minimal wait time value and every odd index
     holds a maximal wait time value.*/
    private void setWaitTimes(params float[] waitTimes)
    {
        if (waitTimes.Length == (ballCount - 1) * 2)
        {
            int numOfIterations = ballCount - 1;
            for (int i = 0; i < numOfIterations; i++)
            {
                minWaitTimes[i + 1] = waitTimes[2*i];
                maxWaitTimes[i + 1] = waitTimes[2*i + 1];
            }
        }
        else Debug.LogError("Invalid number of arguments was provided for the setWaitTimes method");
    }

    public GameObject[] GetOrbsArray()
    {
        return orbs;
    }

    public GameObject[] GetOrbBadOrbs()
    {
        return badOrbs;
    }

    public GameObject getHealthOrb()
    {
        return healthOrb;
    }

    override protected void Awake()
    {
        base.Awake();
        score = 0;
        prevScore = score;
        difficultyLevel = 0;
        gameMaster = this;
        spawningOrbs = false;
        _spearInitPos = new Vector3(0f, -22f, 0f);
        misses = missSprites.Length;
        maxOrbCount = 4;
        scoreText.text = "SCORE: " + score;
        orbSizeFactor = 1.5f;
        miniOrbSize = orbs[0].transform.localScale / orbSizeFactor;
        inTutorial = false;
        if (GameObject.Find("TutorialObject"))
            inTutorial = true;
        minWaitTimes = new float[ballCount + 1];
        maxWaitTimes = new float[ballCount + 1];
        minWaitTimes[0] = maxWaitTimes[0] = 0;
        setWaitTimes(0.1f, 0.175f, 0.1f, 0.175f);
    }

    public IEnumerator spawnSpear()
    {
        if (difficultyLevel < 4)
            DifficultyHandler();
        if (!updatedSpear && ballCount == maxOrbCount)
        {
            Animator mainCamAnim = Camera.main.GetComponent<Animator>();
            mainCamAnim.SetBool("changeColor", true);
            BackgroundColorHandler myRef = Camera.main.GetComponent<BackgroundColorHandler>();
            while (!myRef.colorChanged)
                yield return null;
            SpearHandler.force = new Vector2(0, 190);
            updatedSpear = true;
        }
    }


    public IEnumerator SpawnWave()
    {
        spawningOrbs = true;
        canSpawnBadOrb = true;
        bool canSpawnHealthOrb = true;
        int badOrbCount = 0;
        int maxBadOrbChance = 3;
        //Handling the first ball
        float t0 = Time.time;
        float x0 = generateSign() * spawnx;
        orbs[0].transform.position = new Vector3(x0, orbs[0].transform.position.y,
            orbs[0].transform.position.z);
        float speed = (-Mathf.Sign(x0)) * Random.Range(minSpeed, maxSpeed);
        float commonx = mainCam.transform.position.x + generateSign() * Random.Range(0, maxXDiff);
        float t = ((commonx - x0) / speed);/*The moment the first orb will reach the selected common point*/
        if (inTutorial)
        {
            orbs[0].GetComponent<Ball>().SetTutorial(commonx);
            StartCoroutine(EnableTutorialObject(t));
        }
        orbs[0].GetComponent<Ball>().speed = speed;
        for (int i = 1; i < ballCount; i++)//Handling the rest of the orbs
        {
            yield return new WaitForSeconds(Random.Range(minWaitTimes[i], maxWaitTimes[i]));
            x0 = generateSign() * spawnx;
            orbs[i].transform.position = new Vector3(x0, orbs[i].transform.position.y,
                orbs[i].transform.position.z);
            if (inTutorial)
                orbs[i].GetComponent<Ball>().SetTutorial(commonx);
            else if (canSpawnHealthOrb && score > 17 && score-prevScore > ballCount*4 &&
                misses < missSprites.Length && Random.Range(0, 4 - (missSprites.Length-misses)) == 0)
            {
                canSpawnHealthOrb = false;
                prevScore = score;
                spawnHealthOrb(t, commonx, t0, orbs[i].transform.position);
                continue;//Move on to the next iteration
            }
            else if (canSpawnBadOrb && score > 24 && Random.Range(0, maxBadOrbChance) == 0)
            {
                SpawnBadOrb(t, commonx, t0, orbs[i].transform.position, badOrbCount);
                if (ballCount < maxOrbCount || (ballCount == maxOrbCount && badOrbCount == 1))
                    canSpawnBadOrb = false;
                else//If only one bad orb can be spawned, there's no point in incrementing badOrbCount.
                {
                    badOrbCount++;
                    maxBadOrbChance += (missSprites.Length - misses + badOrbCount);/*The more orbs the player 
                    missed, the smaller the chance is of another bad orb appearing.*/
                }
                continue;//Move on to the next iteration
            }
            else if (score > 17 && Random.Range(0, 9) < 4)
                orbs[i].transform.localScale = miniOrbSize;
            else if (orbs[i].transform.localScale == miniOrbSize)
                orbs[i].transform.localScale *= orbSizeFactor;//Restoring the orb to its original size
            /*Since all sequential balls will start moving after a certain delay, we substract from
              t the time that has passed since the first ball's launch to adjust their speed
              in accordance with the delay.*/
            orbs[i].GetComponent<Ball>().speed = (commonx - x0) / (t - (Time.time - t0));

        }
        spawningOrbs = false;
    }

    private void spawnHealthOrb(float t, float commonx, float t0, Vector3 pos)
    {
        healthOrb.transform.position = pos;
        healthOrb.GetComponent<Ball>().speed = (commonx - pos.x) / (t - (Time.time - t0));
    }

    private IEnumerator EnableTutorialObject(float t)
    {
        yield return new WaitForSeconds(t);
        SpearHandler spearHandler = SpearHandler.GetSpearHandler();
        if(spearHandler.tutorialHandler!=null && !spearHandler.launched)
            spearHandler.tutorialHandler.SetActive(true);
    }

    public IEnumerator HardSpawnWave()
    {
        Debug.Log("HARD");
        spawningOrbs = true;
        canSpawnBadOrb = true;
        bool canSpawnHealthOrb = true;
        int badOrbCount = 0;
        int maxBadOrbChance = 3;
        //Handling the first ball
        float t0 = Time.time;
        float x0 = generateSign() * spawnx;//Must be a seperate variable
        orbs[0].transform.position = new Vector3(x0, orbs[0].transform.position.y,
            orbs[0].transform.position.z);
        float speed = (-Mathf.Sign(x0)) * maxSpeed;
        float commonx = mainCam.transform.position.x + Mathf.Sign(x0) * maxXDiff;
        float t = ((commonx - x0) / speed);/*The moment the first orb will reach 
        the selected common point*/
        orbs[0].GetComponent<Ball>().speed = speed;
        //TESTING
        float temp = ((maxXDiff - spawnx) / -maxSpeed);//When the first orb will reach commonx
        //Debug.Log("MAXSPEED:" + ((maxXDiff - (-spawnx)) / (temp - maxWaitTime * (ballCount - 1))));
        //TESTING
        x0 *= -1;
        for (int i = 1; i < ballCount; i++)
        {
            yield return new WaitForSeconds(maxWaitTimes[i]);
            orbs[i].transform.position = new Vector3(x0, orbs[i].transform.position.y,
                orbs[i].transform.position.z);
            if (canSpawnHealthOrb && score > 17 && score - prevScore > ballCount * 4 &&
                misses < missSprites.Length && Random.Range(0, 4 - (missSprites.Length - misses)) == 0)
            {
                canSpawnHealthOrb = false;
                prevScore = score;
                spawnHealthOrb(t, commonx, t0, orbs[i].transform.position);
                continue;//Move on to the next iteration
            }
            if (canSpawnBadOrb && score > 24 && Random.Range(0, maxBadOrbChance) == 0)
            {
                SpawnBadOrb(t, commonx, t0, orbs[i].transform.position, badOrbCount);
                if (ballCount < maxOrbCount || (ballCount == maxOrbCount && badOrbCount == 1))
                    canSpawnBadOrb = false;
                else//If only one bad orb can be spawned, there's no point in incrementing badOrbCount.
                {
                    badOrbCount++;
                    maxBadOrbChance += (missSprites.Length - misses + badOrbCount);/*The more orbs the player 
                    missed, the smaller the chance is of another bad orb appearing.*/
                }
                continue;//Move on to the next iteration
            }
            if (score > 17 && Random.Range(0, 9) < 4)
                orbs[i].transform.localScale = miniOrbSize;
            else if (orbs[i].transform.localScale == miniOrbSize)
                orbs[i].transform.localScale *= orbSizeFactor;//Restoring the orb to its original size
            /*Since all subsequent balls will start moving after a certain delay, we substract from
             t the time that has passed since the first ball's launch to adjust their speed
             in accordance with the delay.*/
            orbs[i].GetComponent<Ball>().speed = (commonx - x0) / (t - (Time.time - t0));
        }
        spawningOrbs = false;
    }

    public IEnumerator EZSpawnWave()
    {
        Debug.Log("EZ");
        spawningOrbs = true;
        canSpawnBadOrb = true;
        bool canSpawnHealthOrb = true;
        int badOrbCount = 0;
        int maxBadOrbChance = 3;
        //Handling the first ball
        float x0 = generateSign() * spawnx;//Must be a seperate variable
        orbs[0].transform.position = new Vector3(x0, orbs[0].transform.position.y,
                orbs[0].transform.position.z);
        float t0 = Time.time;
        float commonx = mainCam.transform.position.x - Mathf.Sign(x0) * maxXDiff;
        float speed = (-Mathf.Sign(x0)) * minSpeed;
        float t = ((commonx - x0) / speed);/*The moment the first ball will reach the
        selected common point*/
        orbs[0].GetComponent<Ball>().speed = speed;

        //TESTING
        float temp = ((-maxXDiff - spawnx) / -minSpeed);//When the first orb will reach commonx
        /*Debug.Log("MINSPEED:" + ((-maxXDiff - (-spawnx)) / (temp - minWaitTime)));When the
        second orb will reach commonx*/
        //TESTING
        x0 *= -1;
        for (int i = 1; i < ballCount; i++)
        {
            yield return new WaitForSeconds(minWaitTimes[i]);
            orbs[i].transform.position = new Vector3(x0, orbs[i].transform.position.y,
                orbs[i].transform.position.z);
            if (canSpawnHealthOrb && score > 17 && score - prevScore > ballCount * 4 &&
                misses < missSprites.Length && Random.Range(0, 4 - (missSprites.Length - misses)) == 0)
            {
                canSpawnHealthOrb = false;
                prevScore = score;
                spawnHealthOrb(t, commonx, t0, orbs[i].transform.position);
                continue;//Move on to the next iteration
            }
            if (canSpawnBadOrb && score > 24 && Random.Range(0, maxBadOrbChance) == 0)
            {
                SpawnBadOrb(t, commonx, t0, orbs[i].transform.position, badOrbCount);
                if (ballCount < maxOrbCount || (ballCount == maxOrbCount && badOrbCount == 1))
                    canSpawnBadOrb = false;
                else//If only one bad orb can be spawned, there's no point in incrementing badOrbCount.
                {
                    badOrbCount++;
                    maxBadOrbChance += (missSprites.Length - misses + badOrbCount);/*The more orbs the player 
                    missed, the smaller the chance is of another bad orb appearing.*/
                }
                continue;//Move on to the next iteration
            }
            if (score > 17 && Random.Range(0, 9) < 4)
                orbs[i].transform.localScale = miniOrbSize;
            else if (orbs[i].transform.localScale == miniOrbSize)
                orbs[i].transform.localScale *= orbSizeFactor;//Restoring the orb to its original size

            /*Since all sequential balls will start moving after a certain delay, we substract from
             t the time that has passed since the first ball's launch to adjust their speed
             in accordance with the delay.*/
            orbs[i].GetComponent<Ball>().speed = (commonx - x0) / (t - (Time.time - t0));
        }
        spawningOrbs = false;
    }
    private void SpawnBadOrb(float t, float commonx, float t0, Vector3 pos, int index)
    {
        float time = t - (Time.time - t0) + generateSign() * Random.Range(0.24f, 0.26f);
        badOrbs[index].transform.position = pos;
        badOrbs[index].GetComponent<Ball>().speed = (commonx - pos.x) / time;
    }

    public void MissHandler()
    {
        if (--misses >= 0)
        {
            missSprites[misses].SetActive(false);
            if (misses == 0)
                GameOverUI.SetActive(true);
        }
    }

    //Will be invoked upon collision with a spear
    public void HandleDestroyedOrb(Transform orbTrans)
    {
        orbTrans.position = new Vector3(spawnx, orbTrans.position.y, orbTrans.position.z);
        if (orbTrans.CompareTag("Ball"))
        {
            if (orbTrans.localScale == miniOrbSize)
                score += 2;
            else score++;
            scoreText.text = "SCORE: " + score;
        }
        else if (orbTrans.CompareTag("HealthOrb"))
            missSprites[misses++].SetActive(true);
        else MissHandler();//If the player impaled a bad orb
    }

    /*Adjusts the common X value range, waiting time range, speed range 
     * and the number of orbs that can be spawned based on the score.*/
    public void DifficultyHandler()
    {
        //4 orbs
        if (difficultyLevel == 4 && score > 70)
        {
            minSpeed = 8.33f;
            maxSpeed = 8.4f;
            maxXDiff = 2.75f;
            setWaitTimes(0.285f, 0.32f, 0.145f, 0.155f, 0.15f, 0.13f);
            difficultyLevel++;

        }
        else if (difficultyLevel == 3 && score > 46)
        {
            minSpeed = 7.65f;
            maxSpeed = 7.75f;
            maxXDiff = 2.6f;
            y0 = 8f;
            ballCount = maxOrbCount;
            setWaitTimes(0.225f, 0.275f, 0.145f, 0.145f, 0.13f, 0.13f);
            StartCoroutine(UpdateOrbPos());
            difficultyLevel++;
        }

        //3 orbs
        else if (difficultyLevel == 2 && score > 26)
        {
            maxXDiff = 2.6f;
            setWaitTimes(0.34f, 0.35f, 0.225f, 0.225f);
            minSpeed = 8.475f;
            maxSpeed = 8.5f;
            difficultyLevel++;
        }
        else if (difficultyLevel == 1 && score > 17)
        {
            maxXDiff = 2.375f;
            setWaitTimes(0.315f, 0.3225f, 0.215f, 0.225f);
            minSpeed = 8.375f;
            maxSpeed = 8.45f;
            difficultyLevel++;
        }
        else if (difficultyLevel == 0 && score > 5)
        {
            maxXDiff = 2.2f;
            setWaitTimes(0.28f, 0.3f, 0.175f, 0.225f);
            minSpeed = 8.275f;
            maxSpeed = 8.325f;
            difficultyLevel++;
        }
    }

    private IEnumerator UpdateOrbPos()
    {
        StartCoroutine(UpdateBadOrbsPos());
        StartCoroutine(updateHealthOrbPos());
        Ball orb;
        for (int i = 0; i < ballCount; i++)
        {
            orb = orbs[i].GetComponent<Ball>();
            while (orb.speed != 0)//Wait for the orb to be outside of the viewable area.
                yield return null;
            orbs[i].transform.position = new Vector3(orbs[i].transform.position.x, y0 - diffy * i);
        }
    }
    private IEnumerator UpdateBadOrbsPos()
    {
        Ball badOrb;
        for (int i = 0; i < badOrbs.Length; i++)
        {
            badOrb = badOrbs[i].GetComponent<Ball>();
            while (badOrb.speed != 0)
                yield return null;
            badOrb.transform.position = new Vector3(badOrb.transform.position.x, y0);
        }
    }

    private IEnumerator updateHealthOrbPos()
    {
        Ball orb = healthOrb.GetComponent<Ball>();
        while (orb.speed != 0)
            yield return null;
        orb.transform.position = new Vector3(orb.transform.position.x, y0);
    }
}
