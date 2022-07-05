
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDb
{
    public class User
    {
        public bool isClear
        {
            get
            {
                return _isClear;
            }
        }


        public Dictionary<string, string> Fields
        {
            get
            {
                _isClear = false;
                return _fields;
            }
        }

        private Dictionary<string, string> _fields;
        private bool _isClear;

        public User()
        {
            Clear();
        }

        public void Clear()
        {
            _fields = new Dictionary<string, string>();
            _fields["Id"] = Guid.NewGuid().ToString() ;
            _fields["Enviado"] = "No";
            _isClear = true;
        }
    }

    public const string PATH = "users.csv";

    static public SimpleDb Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SimpleDb("Trivia.csv");

            return _instance;
        }
    }

    public User user = new User();

    static private SimpleDb _instance;
    private string _path;
    private string _persitentPath;

    public SimpleDb(string path)
    {
        _persitentPath = Path.Combine(Application.persistentDataPath, path);

        if (Application.isMobilePlatform)
            path = "/storage/emulated/0/" + path;
        else
            path = Path.GetFullPath(path);

        this._path = path;

        Debug.Log(_persitentPath);

        //Backup();
    }

    public string AddUser(User user)
    {
        try
        {

            string data = "";
            List<string> userList = new List<string>();
            foreach (KeyValuePair<string, string> item in user.Fields)
            {
                userList.Add(item.Value);
            }

            userList.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));


            if (!File.Exists(_persitentPath))
            {
                List<string> headers = new List<string>();
                foreach (KeyValuePair<string, string> item in user.Fields)
                {
                    headers.Add(item.Key);
                }
                headers.Add("Fecha");

                data = String.Join(";", headers.ToArray()) + "\r\n";
            }

            data += String.Join(";", userList.ToArray()) + "\r\n";

            File.AppendAllText(_path, data, new UTF8Encoding(true));
            File.AppendAllText(_persitentPath, data, new UTF8Encoding(true));

            return "";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public void Backup()
    {
        try
        {
            if (File.Exists(_path))
            {
                string backup = _path + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                File.Copy(_path, backup);
            }
        }
        catch (Exception e)
        {
        }
    }

    public User[] GetUsers()
    {
        string[] users_strings = GetUsersString();

        if (users_strings == null)
            return null;

        List<User> users = new List<User>();
        string[] keys = null;

        for (int i = 0; i < users_strings.Length; i++)
        {
            string[] tempString = users_strings[i].Split(';');

            if (i == 0)
            {
                keys = tempString;
                continue;
            }

            if (tempString.Length < keys.Length)
                continue;

            User temp = new User();
            for (int j = 0; j < keys.Length; j++)
            {
                temp.Fields[keys[j]] = tempString[j];
            }

            users.Add(temp);
        }

        return users.ToArray();
    }

    public string[] GetUsersString()
    {
        if (!File.Exists(_persitentPath))
            return null;

        string[] users_strings = null;

        try
        {
            users_strings = File.ReadAllLines(_persitentPath);
        }
        catch (Exception e)
        {
            return null;
        }

        return users_strings;
    }

    internal void Update(User[] users)
    {
        User[] newUsers = GetUsers();

        //Header
        List<string> headers = new List<string>();
        foreach (KeyValuePair<string, string> item in users[0].Fields)
        {
            headers.Add(item.Key);
        }

        string data = String.Join(";", headers.ToArray()) + "\r\n";

        int i = 0;
        for (i = 0; i < users.Length; i++)
        {
            try
            {
                List<string> user = new List<string>();
                foreach (KeyValuePair<string, string> item in users[i].Fields)
                {
                    user.Add(item.Value);
                }

                data += String.Join(";", user.ToArray()) + "\r\n";
            }
            catch (Exception e)
            {
            }

        }
        for (i = i; i < newUsers.Length; i++)
        {
            try
            {

                List<string> user = new List<string>();
                foreach (KeyValuePair<string, string> item in newUsers[i].Fields)
                {
                    user.Add(item.Value);
                }

                data += String.Join(";", user.ToArray()) + "\r\n";
            }
            catch (Exception e)
            {
            }
        }

        File.WriteAllText(_path, data, new UTF8Encoding(true));
        File.WriteAllText(_persitentPath, data, new UTF8Encoding(true));
    }
}