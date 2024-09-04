using UnityEngine;
using UnityEngine.UI;

public class HighScore : MonoBehaviour
{
    public Text highScoreText;
    private Scoring scoring; // Instance of Scoring

    void Start()
    {
        UpdateHighScore();
    }

    public void UpdateHighScore()
    {
        highScoreText.text = "HighScore: " + PlayerPrefs.GetInt("HighScore", 0);
    }
}
