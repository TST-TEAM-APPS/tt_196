using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Navigator : MonoBehaviour
{
    [SerializeField] private List<GameObject> _windows;
    [SerializeField] private List<Toggle> _toggles;

    private void Awake()
    {
        foreach (var item in _toggles)
        {
            item.isOn = false;
            item.onValueChanged.AddListener(delegate { UpdateWindows(); });
        }
        _toggles[0].isOn = true;
        UpdateWindows();
    }


    public void UpdateWindows()
    {
        foreach (var item in _windows)
        {
            item.gameObject.SetActive(false);
        }
        _windows[GetSelectedToggleIndex(_toggles)].SetActive(true);
    }

    public int GetSelectedToggleIndex(List<Toggle> toggles)
    {
        int index = 0;
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
