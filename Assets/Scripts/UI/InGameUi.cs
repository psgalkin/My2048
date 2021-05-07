using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUi : MonoBehaviour
{
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _debug;

    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _restartButton;

    [SerializeField] private GameObject _lossObj;

    private void Start()
    {
        _exitButton.onClick.AddListener(Exit);
        _restartButton.onClick.AddListener(Restart);

        Field.OnLoss += Loss;
    }

    public void Debug(string msg)
    {
        _debug.text = msg;
    }

    public void SetScore(int score)
    {
        _scoreText.text = $"Score: {score}";
    }

    public void SetHighScore(int score)
    {
        _highScoreText.text = $"Record: {score}";
    }

    private void Exit()
    {
        Application.Quit();
    }

    private void Restart()
    {
        Application.LoadLevel(0);
    }

    private void Loss()
    {
        _lossObj.SetActive(true);
    }

    private void OnDestroy()
    {
        Field.OnLoss -= Loss;
    }
}
