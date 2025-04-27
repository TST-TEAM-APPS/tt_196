using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Onboarding : MonoBehaviour
{
    [SerializeField] private List<GameObject> _steps;
    private int _currentIndex = 0;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Onboarding"))
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            ShowOnboarding();
        }
    }

    private void ShowOnboarding()
    {
        _currentIndex = 0;
        foreach (var item in _steps)
        {
            item.SetActive(false);
        }
        _steps[_currentIndex].SetActive(true);
    }

    public void ShowNextStep()
    {
        _currentIndex++;
        if (_currentIndex < _steps.Count)
        {
            foreach (var item in _steps)
            {
                item.SetActive(false);
            }
            _steps[_currentIndex].SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("Onboarding", 1);
            gameObject.SetActive(false);
        }
    }
    
}
