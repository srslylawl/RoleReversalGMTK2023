using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager I;

    [SerializeField] private TextMeshProUGUI GetReadyText;

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
    
}
