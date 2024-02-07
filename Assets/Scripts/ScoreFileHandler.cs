using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class ScoreFileHandler : MonoBehaviour
{
    private GameMaster gameMaster;
    public string filePath { get; private set; }
    [SerializeField] private Text scoreText;
    private class ScoreData
    {
        public int score;
        public ScoreData(int score)
        {
            this.score = score;
        }
    }
    private ScoreData scoreData;
    private static ScoreFileHandler scoreFileHandler;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        /*If there's another instance of this object that's different from this one, destroy it.
        This basically destroys the previous instance of this object(the instance that appears first 
        in the hierarchy from top to bottom), the one that no longer has a reference 
        to the HighScoreText object.*/
        if (GameObject.Find(gameObject.name) && GameObject.Find(gameObject.name) != gameObject)
            Destroy(GameObject.Find(gameObject.name));

        scoreFileHandler = this;
        filePath = Application.persistentDataPath + "/HighScore.json";
        scoreData = new ScoreData(0);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            scoreData = JsonUtility.FromJson<ScoreData>(json);
        }
        gameMaster = null;
    }


    public static ScoreFileHandler GetScoreFileHandler()
    {
        if (scoreFileHandler == null)
        {
            Debug.LogError("Panic! No score file handler was found!");
            Application.Quit();
        }
        return scoreFileHandler;
    }

    private void OnApplicationFocus(bool focus)
    {
        //If the game lost focus while the game scene is active
        if (!focus && SceneManager.GetActiveScene().buildIndex>0)
            UpdateScoreFile();
    }

    public void UpdateScoreFile()
    {
        if (gameMaster == null)//To prevent unnecessary calls for the GetGameMaster function
            gameMaster = GameMaster.GetGameMaster();

        if (gameMaster.score > scoreData.score)
        {
            scoreData.score = gameMaster.score;
            File.WriteAllText(filePath, JsonUtility.ToJson(scoreData));
        }
    }

    public void ReadScoreFile()
    {
        scoreText.text = "High score: " + scoreData.score;
    }
}



