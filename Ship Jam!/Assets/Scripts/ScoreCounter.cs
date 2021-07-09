using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreHumanText;

    public TextMeshProUGUI scoreAiText;

    // Misc
    private int scoreHuman;
    private int scoreAi;

    // Singleton of PointsCounter
    private static ScoreCounter instance = null;

    // Singleton of PointsCounter
    public static ScoreCounter Get()
    {
        if (instance == null)
            instance = (ScoreCounter) FindObjectOfType(typeof(ScoreCounter));

        return instance;
    }

    private void Awake()
    {
        scoreHuman = 0;
        scoreAi = 0;

        RefreshScoreUi();
    }

    public void AddScoreHuman()
    {
        scoreHuman++;
        RefreshScoreUi();
    }

    public void AddScoreAi()
    {
        scoreAi++;
        RefreshScoreUi();
    }

    private void RefreshScoreUi()
    {
        scoreHumanText.text = scoreHuman.ToString();
        scoreAiText.text = scoreAi.ToString();
    }
}