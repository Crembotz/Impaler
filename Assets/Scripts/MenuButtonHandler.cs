using UnityEngine.SceneManagement;

public class MenuButtonHandler : RetryHandler
{
    public void Menu()
    {
        scoreFileHandler.UpdateScoreFile();
        SceneManager.LoadScene(0);
    }
}
