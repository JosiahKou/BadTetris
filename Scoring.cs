using UnityEngine;
using UnityEngine.UI;

public class Scoring : MonoBehaviour
{
    public static Scoring instance;
    public Text scoreText;
    int score = 0;
    bool lastRoundHasTetris = false;
    public int level = 1;
    public Text levelText;
    int totalLinesCleared = 0;
    private int highScore = 0;

    void Awake()
    {
        instance = this;

    }

    void Start()
    {
        scoreText.text = score.ToString();
        levelText.text = "Level: " + level.ToString();
    }

    public void AddPoints(int linesCleared, int clearStreak)
    {
        if (clearStreak > 5)
        {
            clearStreak = 5;
        }
        if (clearStreak > 0)
        {
            if (linesCleared == 1)
            {
                score += 100 * level * clearStreak;
                lastRoundHasTetris = false;
                totalLinesCleared += 1;
            }
            else if (linesCleared == 2)
            {
                score += 300 * level * clearStreak;
                lastRoundHasTetris = false;
                totalLinesCleared += 2;
            }
            else if (linesCleared == 3)
            {
                score += 500 * level * clearStreak;
                lastRoundHasTetris = false;
                totalLinesCleared += 3;
            }
            else if (linesCleared == 4)
            {
                if (lastRoundHasTetris)
                {
                    score += 1200 * level * clearStreak;
                }
                else
                {
                    score += 800 * level * clearStreak;
                }
                lastRoundHasTetris = true;
                totalLinesCleared += 4;
            }
        } else{
            if (linesCleared == 1)
            {
                score += 100 * level;
                lastRoundHasTetris = false;
                totalLinesCleared += 1;
            }
            else if (linesCleared == 2)
            {
                score += 300 * level;
                lastRoundHasTetris = false;
                totalLinesCleared += 2;
            }
            else if (linesCleared == 3)
            {
                score += 500 * level;
                lastRoundHasTetris = false;
                totalLinesCleared += 3;
            }
            else if (linesCleared == 4)
            {
                if (lastRoundHasTetris)
                {
                    score += 1200 * level;
                }
                else
                {
                    score += 800 * level;
                }
                lastRoundHasTetris = true;
                totalLinesCleared += 4;
            }
        }

        // Update level and UI
        level = totalLinesCleared / 10 + 1;
        scoreText.text = score.ToString();
        levelText.text = "Level: " + level.ToString();

        CheckHighScore();
    }

    public void CheckHighScore()
    {
        highScore = score;
        if (highScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }
}
