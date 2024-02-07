using UnityEngine.SceneManagement;
using UnityEngine;

public class RetryHandler : MonoBehaviour
{
    protected ScoreFileHandler scoreFileHandler;

    // Start is called before the first frame update
    protected void Start()
    {
        scoreFileHandler = ScoreFileHandler.GetScoreFileHandler();      
    }

    public void RetryGame()
    {
        scoreFileHandler.UpdateScoreFile();
        SceneManager.LoadScene(1);
    }
}
