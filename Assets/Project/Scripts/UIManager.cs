using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour {
    public static UIManager I;
    public static Coroutine coroutine;

    [SerializeField] private GameObject IngameUI;
    [SerializeField] private GameObject ScoreUI;
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject NextBTN;
    [SerializeField] private GameObject OutOfFrogUI;
    [SerializeField] private TextMeshProUGUI RemainingTime;
    [SerializeField] private TextMeshProUGUI GetReadyText;
    [SerializeField] private TextMeshProUGUI Score;

    private void Awake() {
        if (I && I != this) {
            Destroy(I.gameObject);
        }

        I = this;
    }

    private void Start() {
        // StartReadyCountDown("Get to safety!",null);
    }

    public static void StartReadyCountDown(string text, Action callBack) {
        I.StartCoroutine(I.ReadyCountDownRoutine(text, callBack));
    }

    public static void DisplayQuickText(string text, float duration, Action callBack) {
        I.StartCoroutine(I.QuickTextRoutine(text, duration, callBack));
    }

    private IEnumerator ReadyCountDownRoutine(string firstText, Action callBack) {
        GetReadyText.gameObject.SetActive(true);
        GetReadyText.SetText(firstText);
        float timer = 1f;
        var baseScale = new Vector3(1, 1, 1);
        GetReadyText.rectTransform.localScale = baseScale * 1.5f;

        while (timer > 0f) {
            timer -= Time.deltaTime;
            yield return null;
        }
        timer = 1f;
        GetReadyText.SetText("3");
        while (timer > 0f) {
            timer -= Time.deltaTime;
            var scale = timer*2 + 1;
            GetReadyText.rectTransform.localScale = baseScale * scale;
            yield return null;
        }
        GetReadyText.SetText("2");        
        timer = 1f;
        while (timer > 0f) {
            timer -= Time.deltaTime;
            var scale = timer*2 + 1;
            GetReadyText.rectTransform.localScale = baseScale * scale;
            yield return null;
        }
        GetReadyText.SetText("1");
        timer = 1f;
        while (timer > 0f) {
            timer -= Time.deltaTime;
            var scale = timer*2 + 1;
            GetReadyText.rectTransform.localScale = baseScale * scale;
            yield return null;
        }

        GetReadyText.gameObject.SetActive(false);
        
        callBack?.Invoke();
        yield return null;
    }
    
    private IEnumerator QuickTextRoutine(string firstText, float duration, Action callBack) {
        GetReadyText.gameObject.SetActive(true);
        GetReadyText.SetText(firstText);
        float timer = duration;
        var baseScale = new Vector3(1, 1, 1);
        GetReadyText.rectTransform.localScale = baseScale * 1.5f;

        while (timer > 0f) {
            timer -= Time.deltaTime;
            yield return null;
        }
        GetReadyText.gameObject.SetActive(false);
        callBack?.Invoke();
    }

    public static void CountdowmTimeStart(float maxTime)
    {
        coroutine = I.StartCoroutine(I.CountdowmTimeRountine(maxTime));
    }

    private IEnumerator CountdowmTimeRountine(float maxTime)
    {
        RemainingTime.gameObject.SetActive(true);
        float timer = maxTime;
        while(timer > 0) 
        { 
            timer -= Time.deltaTime;
            RemainingTime.SetText("Time Remaining: " + (int)timer);
            yield return null;
        }


        yield return null;
    }

    public static void CountdowmTimeStop()
    {
        I.StopCoroutine(coroutine);
    }

    public static void CloseGameUI()
    {
        I.IngameUI.SetActive(false);
    }

    public static void OpenGameOverUI()
    {
        I.GameOverUI.SetActive(true);
    }

    public static void OpenScoreUI(int score)
    {
        I.ScoreUI.SetActive(true);
        I.Score.text = ""+score;

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (SceneManager.sceneCount >= nextScene)
        {
            I.NextBTN.SetActive(true);
            I.OutOfFrogUI.SetActive(false);
        }
        else
        {
            I.NextBTN.SetActive(false);
            I.OutOfFrogUI.SetActive(true);
        }
    }

    public static void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
