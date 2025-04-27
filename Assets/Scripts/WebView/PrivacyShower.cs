using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivacyShower : MonoBehaviour
{

    [SerializeField] private UniWebView _uni;

    private void Start()
    {
        OpenPrivacy();
    }

    public void OpenPrivacy()
    {
        if (PlayerPrefs.HasKey("newlink"))
        {
            _uni.Load(PlayerPrefs.GetString("newlink"));
        }
        else
        {
            _uni.Load(PlayerPrefs.GetString("link"));
        }
        _uni.OnPageFinished += OnPageLoaded;
        _uni.Show();
    }

    public void OnPageLoaded(UniWebView webView, int statusCode, string url)
    {
        if(url != "")
        {
            PlayerPrefs.SetString("newlink", url);
        }

    }
}
