using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField]private Text text;
    private SpearHandler spearHandler;
    private bool terminateTutorial;//If we need to terminate the tutorial

    void Awake()
    {
        terminateTutorial = true;
        //If a high score file exists, it means the player already knows how to play the game
        ScoreFileHandler scoreFileHandler = ScoreFileHandler.GetScoreFileHandler();
        if (File.Exists(scoreFileHandler.filePath))
        {
            terminateTutorial = false;
            Destroy(gameObject);
        }
        spearHandler = SpearHandler.GetSpearHandler();
    }


    private void OnEnable()
    {
        //Execute the following commands only after the player held the spear
        if (spearHandler.canLaunch)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            text.text = "RELEASE!!";
        }
    }


    private void OnDestroy()
    {
        if (terminateTutorial)
        {
            GameMaster gameMaster = GameMaster.GetGameMaster();
            GameObject[] orbs = gameMaster.GetOrbsArray();
            for (int i = 0; i < gameMaster.ballCount; i++)
                orbs[i].GetComponent<Ball>().TerminateTutorial();
        }
    }
}
