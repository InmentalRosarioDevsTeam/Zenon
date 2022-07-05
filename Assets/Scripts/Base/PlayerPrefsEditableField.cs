using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerPrefsEditableField : MonoBehaviour
{
    public string defaultValue;
    public UnityEvent OnChange;

    private void OnEnable()
    {
        GetComponent<InputField>().text = PlayerPrefs.GetString(name,defaultValue);
        PlayerPrefs.SetString(name, GetComponent<InputField>().text);

    }

    private void OnDisable()
    {
        if(PlayerPrefs.GetString(name, defaultValue) != GetComponent<InputField>().text)
        { 
            PlayerPrefs.SetString(name, GetComponent<InputField>().text);

            if (OnChange != null)
                OnChange.Invoke();
        }
    }
}
