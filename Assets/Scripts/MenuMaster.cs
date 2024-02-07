using System.Collections;
using UnityEngine;

public class MenuMaster : MasterParent
{
    [SerializeField] private float minWaitTime, maxWaitTime;
    protected override void Awake()
    {
        base.Awake();
        ScoreFileHandler myRef = ScoreFileHandler.GetScoreFileHandler();
        myRef.ReadScoreFile();
        InvokeRepeating("CheckForOrbs", 0, 1f);
        
    }
    IEnumerator SpawnWave()
    {
        //Handling the first ball
        float t0 = Time.time;
        float x0 = generateSign() * spawnx;
        orbs[0].transform.position = new Vector3(x0, orbs[0].transform.position.y,
            orbs[0].transform.position.z);
        float speed = (-Mathf.Sign(x0)) * Random.Range(minSpeed, maxSpeed);
        float commonx = mainCam.transform.position.x + generateSign() * Random.Range(0, maxXDiff);
        float t = ((commonx - x0) / speed);//The moment the first orb will reach the selected common point
        orbs[0].GetComponent<Ball>().speed = speed;
        for (int i = 1; i < ballCount; i++)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            x0 = generateSign() * spawnx;
            orbs[i].transform.position = new Vector3(x0, orbs[i].transform.position.y,
                orbs[i].transform.position.z);
            /*Since all sequential balls will start moving after a certain delay, we substract from
             t the time that has passed since the first ball's launch to adjust their speed
             in accordance with the delay.*/
            orbs[i].GetComponent<Ball>().speed = (commonx - x0) / (t - (Time.time - t0));
        }
    }

    private void CheckForOrbs()
    {
        for (int i = 0; i < ballCount; i++)
            if (orbs[i].GetComponent<Ball>().speed != 0)
                return;
        StartCoroutine("SpawnWave");
    }

}
