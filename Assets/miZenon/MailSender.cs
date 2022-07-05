using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mime;
using System.IO;
using System.ComponentModel;
using UnityEngine.Networking;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Mono.Data.Sqlite;
using UnityEngine.UI;
using Com;

public class MailSender : MonoBehaviour
{
    public GameObject sending_img;
    public Text sending_txt;
    bool sending;
    int sendcount = 0;

    public class CertificateWhore : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    private string serverUrl = "https://127.0.0.1:80/nivea360/upload.php";
  
    void Update()
    {
        sending_img.SetActive(sending);
        if (sending)
            sending_txt.text = sendcount.ToString();
        else
            sending_txt.text = "";
    }

    IEnumerator Loop()
    {
        bool run = true;

        while (run)
        {
            Debug.Log("Mail Sender run");
            sending = false;

            SqliteDataReader reader = GameManager_s.db.ExecuteQuery("SELECT * from participantes where enviado=0");

            sendcount = GameManager_s.db.ExecuteQueryCount("SELECT count(id) from participantes where enviado=0");

            while (reader.Read())
            {

                Debug.Log("Enviando a jugador id=" + reader["id"].ToString());


                Debug.Log("El video Existe");

                WWWForm form = new WWWForm();
                byte[] bytes = null;

                try
                {
                    bytes = File.ReadAllBytes(reader["FotoA"].ToString());
                }
                catch (Exception)
                {
                    Debug.Log("No se puede acceder al archivo:" + Path.GetFileName(reader["FotoA"].ToString()));
                }

                try
                {
                    bytes = File.ReadAllBytes(reader["FotoB"].ToString());
                }
                catch (Exception)
                {
                    Debug.Log("No se puede acceder al archivo:" + Path.GetFileName(reader["FotoB"].ToString()));
                }

                try
                {
                    bytes = File.ReadAllBytes(reader["FotoC"].ToString());
                }
                catch (Exception)
                {
                    Debug.Log("No se puede acceder al archivo:" + Path.GetFileName(reader["FotoC"].ToString()));
                }

                //--------->>> Aca 
                if (bytes != null)
                {
                    Debug.Log("Sending..." + reader["Mail"].ToString() );

                    //NombreYApellido,Mail,Dni,FechaDeNacimiento,TenesManchas,CausaDeLasManchas,Foto,enviado,fecha
                    form.AddBinaryData("fileToUpload", bytes, Path.GetFileName( reader["FotoA"].ToString() ), "image/jpg");
                    form.AddBinaryData("fileToUpload", bytes, Path.GetFileName( reader["FotoB"].ToString() ), "image/jpg");
                    form.AddBinaryData("fileToUpload", bytes, Path.GetFileName( reader["FotoC"].ToString() ), "image/jpg");
                    form.AddField("id", reader["id"].ToString());
                    form.AddField("NombreYApellido", reader["NombreYApellido"].ToString());
                    form.AddField("Mail", reader["Mail"].ToString());
                    form.AddField("Edad", reader["Edad"].ToString());
                    form.AddField("Acepto", reader["Acepto"].ToString());
                    form.AddField("FotoA", reader["FotoA"].ToString());
                    form.AddField("FotoB", reader["FotoB"].ToString());
                    form.AddField("FotoC", reader["FotoC"].ToString());
                    form.AddField("Fecha", reader["Fecha"].ToString());
                    form.AddField("Hora", reader["Hora"].ToString());
       
                    UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
                    sending = true;

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                        Debug.Log("Error enviando");
                    }
                    else
                    {
                        string text = www.downloadHandler.text;

                        sendcount--;

                        if (!text.Contains("OK"))
                        {
                            Debug.Log("no ok!");
                            Debug.Log(text);

                            GameManager_s.db.ExecuteQuery("update participantes set enviado=2 where id=" + reader["id"].ToString());
                        }
                        else
                        {
                            GameManager_s.db.ExecuteQuery("update participantes set enviado=1 where id=" + reader["id"].ToString());
                            Debug.Log("Enviado! " + reader["Mail"].ToString() );

                        }
                    }
                    sending = false;

                }
                yield return new WaitForSeconds(0.5f);
            }

            reader.Close();

            yield return new WaitForSeconds(5);
        }
    }

    public void EnviarMail()
    {
        SimpleDb.Instance.Backup();
        StartCoroutine(Loop());
    }
}
