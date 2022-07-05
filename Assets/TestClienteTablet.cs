using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestClienteTablet : MonoBehaviour
{
    public TcpHelper tcpHelper;

    private void Start()
    {
        tcpHelper.OnRead.AddListener(s =>
        {
            transform.Find("Recibido").GetComponentInChildren<Text>().text += s + "\n\r";
        });

        transform.Find("EnviarBtn").GetComponent<Button>().onClick.AddListener(() =>
        {

            //envia
            tcpHelper.Enviar(transform.Find("Datos").GetComponent<InputField>().text);
            
            //lo pone en nulo
            transform.Find("Datos").GetComponent<InputField>().text = "";

        });
    }
}
