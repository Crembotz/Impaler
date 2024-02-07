using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseHandler : MonoBehaviour
{
    public bool paused { get; private set; }
    [SerializeField] private Sprite pause, play;
    [SerializeField] private Image currPauseButtonImage;
    private float playSpeed;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform restartTransform;
    private Vector2 initRestartPos, desiredRestartPos;
    public static PauseHandler pauseHandler { get; private set; }

    private void Awake()
    {
        pauseHandler = this;
        paused = false;
        playSpeed = 1.0f;
        initRestartPos = restartTransform.position;
        desiredRestartPos = new Vector2(transform.position.x, transform.position.y + 205);
        //anim = GetComponent<Animator>();
    }

    public static PauseHandler GetPauseHandler()
    {
        if (pauseHandler == null)
        {
            Debug.LogError("Panic! No pause handler was found!");
            Application.Quit();
        }
        return pauseHandler;
    }

    public void Pause()
    {
        //anim.SetFloat("moveRestartSpeed", -playSpeed);
        //anim.Play("moveRestart", 0, 0);
        if (playSpeed > 0)
            restartTransform.position = desiredRestartPos;
        else restartTransform.position = initRestartPos;
        
        playSpeed *= (-1);
        if (!paused)
        {
            currPauseButtonImage.sprite = play;
            Time.timeScale = 0;
        }
        else
        {
            currPauseButtonImage.sprite = pause;
            Time.timeScale = 1;
        }
        paused = !paused;
        restartTransform.position = desiredRestartPos;
    }

    public void EnableRestartAnimator()
    {
        //anim.Play("moveRestart",0,0);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("move");
    }

    public IEnumerator updateRestartPos()
    {
        if (playSpeed > 0)
            restartTransform.position = desiredRestartPos;
        else restartTransform.position = initRestartPos;
        playSpeed *= (-1);
        while (paused)
            yield return null;
        Debug.Log("HERE!!!!!!!");
    }
}
