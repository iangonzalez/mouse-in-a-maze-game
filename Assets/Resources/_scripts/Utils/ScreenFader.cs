using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour {
    public Image fadeImage;
    public float rate = 2.0f;

    public bool doneFading = false;
    
    void Awake() {
        this.enabled = false;
        fadeImage.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
        fadeImage.color = Color.clear;
    }

    public void FadeScreenToBlack() {
        this.enabled = true;
    }

    void Update() {
        fadeImage.color = Color.Lerp(fadeImage.color, Color.black, Time.deltaTime * rate);

        if (fadeImage.color == Color.black) {
            doneFading = true;
        }
    }
}
