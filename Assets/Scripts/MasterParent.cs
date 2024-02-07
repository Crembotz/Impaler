using UnityEngine;

public class MasterParent : MonoBehaviour
{
    public int ballCount{ get; protected set; }
    [SerializeField] protected float diffy;
    [SerializeField] protected float spawnx, y0;
    [SerializeField] protected float maxXDiff;
    [SerializeField] protected GameObject[] orbs;
    public float minSpeed, maxSpeed;
    protected Camera mainCam;


    //Called before every Start() method, used for initialization
    protected virtual void Awake()
    {
        mainCam = Camera.main;
        ballCount = 3;
    }

    protected int generateSign()
    {
        int sign = Mathf.RoundToInt(Random.value);
        if (sign == 0)
            sign--;
        return sign;
    }






}
