using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagement : MonoBehaviour
{
    public GameObject loadingCanvas;
    public Image loadBar;

    private void Start()
    {
        GameManager.instance.sceneManagement = this;
    }

    public void SelectState(string stateName)
    {
        Time.timeScale = 1.0f;
        //TODO : Loading Screen
        OnLoadingScreen($"State 0{stateName}");
    }

    public void SelectTestState()
    {
        Time.timeScale = 1.0f;
        OnLoadingScreen("ForMidterm");
    }

    public void SelectTargetScene(string stateName)
    {
        Time.timeScale = 1.0f;

        OnLoadingScreen(stateName);
    }

    public void StartNewTargetLevel(int levelIndex)
    {
        Time.timeScale = 1.0f;
        RunStageManager.instance.StartNewRunStage();
        OnLoadingScreen(GameManager.instance.GetSceneLevel(levelIndex));
    }

    public void StartSceneTutorial()
    {
        Time.timeScale = 1.0f;
        RunStageManager.instance.StartNewRunStage();
        OnLoadingScreen("Tutorial");
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1.0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        OnLoadingScreen("Lobby");
    }

    public void NextWinStage(string sceneName)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }

    public void RestartScene()
    {
        Time.timeScale = 1.0f;
        RunStageManager.instance.StartNewRunStage();
        OnLoadingScreen(SceneManager.GetActiveScene().name);
    }

    public void NextWinStageTest(string sceneName)
    {
        Time.timeScale = 1.0f;
        RunStageManager.instance.StartNewRunStage();
        RunStageManager.instance.daysCount = 5;
        OnLoadingScreen(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnLoadingScreen(string sceneName)
    {
        if (loadingCanvas == null)
            SceneManager.LoadScene(sceneName);
        else
            StartCoroutine(LoadingScreenTimer(sceneName));
    }

    public IEnumerator LoadingScreenTimer(string sceneName)
    {
        if (StageManager.instance != null)
        {
            StageManager.instance.canPause = false;
        }

        if (LobbyManager.instance != null)
        {
            LobbyManager.instance.canPause = false;
        }

        AudioManager.instance.StopMusic();

        loadBar.fillAmount = 0;
        loadingCanvas.SetActive(true);

        Time.timeScale = 1;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        float progress = 0;
        while (!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            loadBar.fillAmount = progress;
            if (progress >= 0.9f)
            {
                loadBar.fillAmount = 1;
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}