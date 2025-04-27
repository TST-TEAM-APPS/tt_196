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

public class SaveDataInstaller : MonoBehaviour
{
    [SerializeField] private bool _fromTheBeginning;

    public void InstallBindings()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        BindFileNames();
        BindRegistration();
        BindSettings();
    }

    private void BindRegistration()
    {
        {
            var reg = SaveSystem.LoadData<RegistrationSaveData>();

#if UNITY_EDITOR
            if (_fromTheBeginning)
            {
                reg = null;
            }
#endif

            if (reg == null)
            {
                reg = new RegistrationSaveData("", false);
                SaveSystem.SaveData(reg);
            }

        }
    }

    private void BindSettings()
    {
        {
            var settings = SaveSystem.LoadData<SettingSaveData>();

#if UNITY_EDITOR
            if (_fromTheBeginning)
            {
                settings = null;
            }
#endif

            if (settings == null)
            {
                settings = new SettingSaveData(new List<bool> { false, false, false, false, false, false, false, false, false });
                SaveSystem.SaveData(settings);
            }

        }
    }





    private void BindFileNames()
    {
        FileNamesContainer.Add(typeof(RegistrationSaveData), FileNames.RegData);
        FileNamesContainer.Add(typeof(SettingSaveData), FileNames.SettingsData);
    }
}