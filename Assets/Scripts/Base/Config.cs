using System;
using System.Xml;
using System.IO;
using UnityEngine;

public class Config
{
    //----------------------------------------------------------------------------------------------------------------------------------------------------------

    //--------------------------------------------------------------------------------------------------------
    //Public Propieties
    //--------------------------------------------------------------------------------------------------------

    static public Config Instance
    {
        get
        {
            if (_instance == null) _instance = new Config();
            return _instance;
        }
    }

    public readonly string CONFIG_PATH = Path.GetFullPath("config.xml");

    //--------------------------------------------------------------------------------------------------------
    //Public Methods
    //--------------------------------------------------------------------------------------------------------

    public string GetString(string key, string defaultValue = "", string section = "main", string atributteName = "value")
    {
        LoadFromFile();

        XmlElement keyNode = GetElement(key, defaultValue, section, atributteName);

        Save();
        return keyNode.GetAttribute(atributteName);
    }
    public int GetInt(string key, int defaultValue = 0, string section = "main", string atributteName = "value")
    {
        string value = GetString(key, defaultValue.ToString(), section, atributteName);
        int convertedValue = 0;

        int.TryParse(value, out convertedValue);

        return convertedValue;
    }
    public float GetFloat(string key, float defaultValue = 0, string section = "main", string atributteName = "value")
    {
        string value = GetString(key, defaultValue.ToString(), section, atributteName);
        float convertedValue = 0;

        float.TryParse(value, out convertedValue);

        return convertedValue;
    }
    public bool GetBoolean(string key, bool defaultValue = false, string section = "main", string atributteName = "value")
    {
        string value = GetString(key, defaultValue.ToString(), section, atributteName);
        bool convertedValue = false;

        bool.TryParse(value, out convertedValue);

        return convertedValue;
    }
    public string GetValue(string key, object defaultValue = null, string section = "main", string atributteName = "value")
    {
        return GetString(key, Convert.ToString(defaultValue), section, atributteName);
    }

    public bool HasElement(string key, string section = "main")
    {
        try
        {
            XmlNode sectionNode = _xml.GetElementsByTagName(section)[0];

            if (sectionNode == null)
            {
                sectionNode = (XmlElement)_xml.CreateElement(section);
                return false;
            }

            XmlElement keyNode = sectionNode.SelectSingleNode(key) as XmlElement;
            if (keyNode == null)
            {
                keyNode = (XmlElement)_xml.CreateElement(key);
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void SetString(string key, string value = "", string section = "main", string atributteName = "value")
    {
        //LoadFromFile();

        XmlElement keyNode = GetElement(key, value, section, atributteName);

        keyNode.SetAttribute(atributteName, value);
        Save();
    }
    public void SetInt(string key, int value = 0, string section = "main", string atributteName = "value")
    {
        SetString(key, value.ToString(), section, atributteName);
    }
    public void SetFloat(string key, float value = 0, string section = "main", string atributteName = "value")
    {
        SetString(key, value.ToString(), section, atributteName);
    }
    public void SetValue(string key, object value = null, string section = "main", string atributteName = "value")
    {
        SetString(key, Convert.ToString(value), section, atributteName);
    }
    public void SetBoolean(string key, bool value = false, string section = "main", string atributteName = "value")
    {
        SetString(key, Convert.ToString(value), section, atributteName);
    }
    //--------------------------------------------------------------------------------------------------------
    //Private Propieties
    //--------------------------------------------------------------------------------------------------------

    static private Config _instance;
    private XmlDocument _xml;

    //--------------------------------------------------------------------------------------------------------
    //Private Methods
    //--------------------------------------------------------------------------------------------------------

    private Config()
    {
        //steAll();

        _xml = new XmlDocument();

        if (Application.isMobilePlatform)
        {
            CONFIG_PATH = Path.Combine(Application.persistentDataPath, "config.xml");
        }

        LoadFromFile();
    }

    private void LoadFromFile()
    {
        try
        {
            if (PlayerPrefs.HasKey("config"))
            {
                _xml.LoadXml(PlayerPrefs.GetString("config"));
            }
            else
            {
                try
                {
                    _xml.LoadXml(Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(CONFIG_PATH)).text);
                    Save();
                }
                catch (Exception e)
                {
                    Debug.LogError("Config LoadFromFile() ERROR! : " + e.Message);
                    _xml.LoadXml("<config></config>");
                    Save();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);

            _xml.LoadXml("<config></config>");
            Save();
        }
    }

    void Save()
    {
        string x = "";

        using (var stringWriter = new StringWriter())
        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
        {
            _xml.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            x = stringWriter.GetStringBuilder().ToString();
        }

        Debug.Log(x);

        PlayerPrefs.SetString("config", x);
    }
    private XmlElement GetElement(string key, string defaultValue = "", string section = "main", string atributteName = "value")
    {
        XmlNode sectionNode = _xml.GetElementsByTagName(section)[0];

        if (sectionNode == null)
        {
            sectionNode = (XmlElement)_xml.CreateElement(section);
            _xml.FirstChild.AppendChild(sectionNode);
        }

        XmlElement keyNode = sectionNode.SelectSingleNode(key) as XmlElement;
        if (keyNode == null)
        {
            keyNode = (XmlElement)_xml.CreateElement(key);
            sectionNode.AppendChild(keyNode);
        }

        if (!keyNode.HasAttribute(atributteName))
            keyNode.SetAttribute(atributteName, defaultValue);

        return keyNode;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------
}

