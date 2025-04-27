using Flagsmith;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.SceneManagement;

namespace RemoutConfig
{
    public class RemoutConfigLoader : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private ConfigData _allConfigData;
        [SerializeField] private SaveDataInstaller _saveDataInstaller;
        private bool _showTerms = true;
        private static FlagsmithClient _flagsmithClient;

        public Action<bool> ConfigLoadEnded;

        private void Start()
        {
#if UNITY_IOS
        if (!PlayerPrefs.HasKey("Onboarding"))
        {
        Device.RequestStoreReview();
        }
#endif
            //Перед получением данных из конфига инициализируйте свою систему сохранений, чтоб не было null при сохранении ссылки
            _saveDataInstaller.InstallBindings();
            _flagsmithClient = new(_key);
            StartLoading();
        }


        private void StartLoading()
        {
            string HtmlText = GetHtmlFromUri("http://google.com");

            Debug.Log("Google result: " + HtmlText);

            if (HtmlText != "")
            {
                if(_key != "")
                {
                    LoadRemoutConfig();
                }
                else
                {
                    Debug.Log("Missing Flagsmith key");
                    LoadScene();
                }
            }

            else
            {
                Debug.Log("No internet");
                LoadScene();
            }
        }

        public void LoadRemoutConfig()
        {
            _ = StartAsync();
        }

        async Task StartAsync()
        {
            var flags = await _flagsmithClient.GetEnvironmentFlags();
            if (flags == null)
            {
                Debug.Log("flags null");
                LoadScene();
                return;
            }
            string values = await flags.GetFeatureValue("feature");
            if (values == null || values == "")
            {
                Debug.Log("values null");
                LoadScene();
                return;
            }
            Debug.Log("Loaded");
            ProcessJsonResponse(values);
        }

        private void ProcessJsonResponse(string jsonResponse)
        {
            ConfigData feature = JsonConvert.DeserializeObject<ConfigData>(jsonResponse);
            _allConfigData.featureData = feature.featureData;
            _allConfigData.featureDisabled = feature.featureDisabled;

            Debug.Log("link's value from Config: " + _allConfigData.featureData);
            Debug.Log("useprivacy's value from Config: " + _allConfigData.featureDisabled);

            _showTerms = _allConfigData.featureDisabled;
            if (!_showTerms)
            {
                if (PlayerPrefs.HasKey("link"))
                {
                    if (PlayerPrefs.GetString("link") != _allConfigData.featureData)
                    {
                        PlayerPrefs.SetString("newlink", _allConfigData.featureData);
                    }
                }
                PlayerPrefs.SetString("link", _allConfigData.featureData);
            }
            LoadScene();

        }

        private void LoadScene()
        {
            ConfigLoadEnded?.Invoke(_showTerms);
        }

        public string GetHtmlFromUri(string resource)
        {
            string html = string.Empty;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to 
                            //adjust (probably stay under 300)
                            char[] cs = new char[80];
                            reader.Read(cs, 0, cs.Length);
                            foreach (char ch in cs)
                            {
                                html += ch;
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return html;
        }

    }

    [Serializable]
    public class ConfigData
    {
        public string featureData;
        public bool featureDisabled;
    }
}
