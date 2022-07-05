using Com.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TcpHelper : MonoBehaviour
{
    public class OnReadEvent : UnityEvent<string> { }

    public OnReadEvent OnRead = new OnReadEvent();

    public GameObject icon;
    public bool isServer = false;
    TCPClient client;

    private void Awake()
    {
        Conectar();
    }
    public void Conectar()
    {
        if(isServer)
        {
            TCPServer.Instance.Initialize(5000);
            TCPServer.Instance.MessageReceivedEvent += Instance_MessageReceivedEvent;


            UnityEngine.Debug.Log("Conectando como servidor:("+ TCPServer.Instance.port + ")");
        }
        else
        {
            client = new TCPClient(PlayerPrefs.GetString("ipserver", "127.0.0.1"), 5000);
            client.MessageReceivedEvent += Instance_MessageReceivedEvent;
            GameManager.Instance.OnStateChanged += Instance_OnStateChanged;
            //GameManager.Instance.OnStateChanged += Instance_OnStateChanged;

            UnityEngine.Debug.Log("Conectando como cliente:(" + PlayerPrefs.GetString("ipserver", "127.0.0.1") + ",5000)");
        }
    }

    private void Instance_OnStateChanged(string state)
    {
        //Enviar("change_state|" + state);
    }

    Queue<string> socketMsg = new Queue<string>();

    private void Instance_MessageReceivedEvent(object sender, MessageEventArgs e)
    {
        lock (socketMsg)
            socketMsg.Enqueue(e.message);
    }


    public void Enviar(string m)
    {
        if (isServer)
        {
            TCPServer.Instance.SendMessage(m);
        }
        else
        {
            client.SendMessage(m);
        }
    }

    private void Update()
    {
        if (isServer)
        {
            icon.SetActive(TCPServer.Instance.clients == 0);
        }
        else
        {
            if(client!=null)icon.SetActive(!client.Connected);
        }

        if(socketMsg.Count>0)
        {
            string m = "";

            lock (socketMsg)
                m = socketMsg.Dequeue();

            //string[] args = m.Split('|');

            OnRead.Invoke(m);
        }    
    }

    private void OnApplicationQuit()
    {
        GameManager.Instance.OnStateChanged -= Instance_OnStateChanged;
        if (isServer)
        {
            TCPServer.Instance.MessageReceivedEvent -= Instance_MessageReceivedEvent;
            TCPServer.Instance.Close();
            
        }
        else
        {
            if(client!=null)
            {
                client.MessageReceivedEvent -= Instance_MessageReceivedEvent;
                client.Close();  
            }
        }
    }
}
