using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldFixer : MonoBehaviour
{
    private TMP_InputField _inputField;
    private bool _keepOldTextInField = false;
    private string _oldEditText;
    private string _editText;

    private void Awake()
    {
        _inputField = gameObject.GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        _inputField.text = "";
        _inputField.onEndEdit.AddListener(EndEdit);
        _inputField.onValueChanged.AddListener(Editing);
        _inputField.onTouchScreenKeyboardStatusChanged.AddListener(ReportChangeStatus);
    }

    private void OnDisable()
    {
        _inputField.onEndEdit.RemoveListener(EndEdit);
        _inputField.onValueChanged.RemoveListener(Editing);
        _inputField.onValueChanged.RemoveListener(Editing);
        _inputField.onTouchScreenKeyboardStatusChanged.RemoveListener(ReportChangeStatus);
    }

    private void ReportChangeStatus(TouchScreenKeyboard.Status newStatus)
    {
        if (newStatus == TouchScreenKeyboard.Status.Canceled)
            _keepOldTextInField = true;
    }

    private void Editing(string currentText)
    {
        _oldEditText = _editText;
        _editText = currentText;
    }

    private void EndEdit(string currentText)
    {
        if (_keepOldTextInField && !string.IsNullOrEmpty(_oldEditText))
        {
            //IMPORTANT ORDER
            _editText = _oldEditText;
            _inputField.text = _oldEditText;

            _keepOldTextInField = false;
        }
    }
}
