using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

using System.Runtime.Serialization.Formatters.Binary;
using TMPro;

using Com;
using Com.Sockets;






public class GameManager_s : MonoBehaviour
{
    public FileStream saveFile;
    public FileStream saveFile2;
    public SaveData saveData;
    public SaveCount saveCount;
    bool fileExists;
    bool countExists;
    string rutaCVS = "";
    public string rutaSaveData = "";
    int contador;
    string rutaListas;


    [SerializeField] private Text nombrePersonaje;
    [SerializeField] private GameObject canvas;
    [SerializeField] private InputField nombre;
    [SerializeField] private InputField mail;
    [SerializeField] private InputField edad;
    [SerializeField] private Toggle acepto;



    [SerializeField] private VideoPlayer video;
    [SerializeField] private VideoClip[] videos;
    [SerializeField] private GameObject camaraDeFotos;
    [SerializeField] private GameObject Animacion;
    [SerializeField] private GameObject Conteo;
    [SerializeField] private GameObject tres;
    [SerializeField] private GameObject dos;
    [SerializeField] private GameObject uno;

    [SerializeField] private GameObject revision;
    [SerializeField] private GameObject despedida;

   // public GameObject fondo;
  //  private Image imagen1;

    public int personajeElegido;

    public int cont=0;

  //  public TomaFoto tomaFoto;
    public RawImage[] imagenShot = new RawImage[3];
    private WebCamTexture webCamTexture;
    private string fileName = "foto";
    private string savePath = "Application.DataPath";
    private int captureCounter = 0;
    private byte[] bytes;
    private Texture2D snap;


    //-------->>>>>> BASE DE DATOS
    public static DbAccess db;

    public string[] datosAPasar;

    public List<Participante> participantes = new List<Participante>();

    public Participante p;

    private void Awake()
    {
       db = new DbAccess("Data Source=\"" + Application.streamingAssetsPath + "\\db.db" + "\";Cache=Shared;");
    }

    private void OnApplicationQuit()
    {
       db.CloseSqlConnection();
    }
    //<<<<<<<<<----------







    void Start()
    {
        participantes.Clear();

        rutaListas = Application.dataPath;

        CheckCount();

        if (countExists)
        {
            LoadCount();
            CheckSave();
            if (fileExists)
            {
                LoadGame(rutaSaveData);
            }
        }
        else
        {
            contador = 1;

            rutaSaveData = "/saveData_" + contador + ".bin";
            rutaCVS = rutaListas + " /Resources/Listas/Lista de participantes (" + contador + ").csv";

            SavesCount();
        }

        datosAPasar = new string[9];

        personajeElegido = -1;

        camaraDeFotos.SetActive(false);

        Animacion.SetActive(false);

        Conteo.SetActive(false);
        uno.SetActive(false);
        dos.SetActive(false);
        tres.SetActive(false);
    }

    void Update()
    {
       
    }


    public void SeleccionPersonaje(string s)
    {
        
        switch (s)
        {
            case "Zenon": personajeElegido = 0; break;
            case "Bartolito": personajeElegido = 1; break;
            case "El Lobo Beto": personajeElegido = 2; break;
            case "Pancha": personajeElegido = 3; break;
        }

        video.clip = videos[personajeElegido];
        

        nombrePersonaje.text = s.ToUpper();
        nombrePersonaje.fontSize = 100;

        canvas.GetComponent<Animator>().Play("Ir a Comenzar");
    }

    void NuevoParticipante()
    {
        p = new Participante();
        p.nombre = nombre.text;
        p.mail = mail.text;
        p.edad = edad.text;
        p.acepto = (acepto.isOn) ? "Acepto" : "No Acepto";
        p.fecha = DateTime.Now.ToString("dd/MM/yyyy");
        p.hora = DateTime.Now.ToString("HH:mm");
    }

    void CargarParticipante()
    {
        datosAPasar[0] = p.nombre;
        datosAPasar[1] = p.mail;
        datosAPasar[2] = p.edad;
        datosAPasar[3] = p.acepto;
        datosAPasar[4] = p.foto1;
        datosAPasar[5] = p.foto2;
        datosAPasar[6] = p.foto3;
        datosAPasar[7] = p.fecha;
        datosAPasar[8] = p.hora;

        db.InsertInto("participantes", datosAPasar);
    }

    public void Comenzar()
    {
        NuevoParticipante();

        canvas.GetComponent<Animator>().Play("Ir a Espera");

        camaraDeFotos.SetActive(true);

        Invoke("PlayVideo", 1f);

        StartCoroutine(Cronometro());

        print("Tomando fotos... ");
        nombre.text = "";
        mail.text = "";

    }

    void PlayVideo()
    {
        Animacion.SetActive(true);
        video.Play();
        
    }

    public void Reintentar()
    {
        cont = 0;

        canvas.GetComponent<Animator>().Play("Volver de Final");

        camaraDeFotos.SetActive(true);

        Invoke("PlayVideo", 1f);

        StartCoroutine(Cronometro());

        print("Tomando fotos... ");
    }

    public void Validar()
    {


        if (nombre.text != "" && mail.text != "" && edad.text != "" && ValidarMail(mail.text) && acepto.isOn) 
        {
            canvas.GetComponent<Animator>().Play("Ir a Seleccion");
        }
    }

    public bool ValidarMail(string s)
    {
        if (s.Contains("@") && (     s.Contains(".com.ar") || s.Contains(".com") || s.Contains(".es") || s.Contains(".net")    )         )
        {
            return true;
        }

        else return false;
    }



     IEnumerator Cronometro()
    {

        yield return new WaitForSeconds(3.01f);

        Conteo.SetActive(true);

        tres.SetActive(true);
        yield return new WaitForSeconds(1f);
        tres.SetActive(false);
        dos.SetActive(true);
        yield return new WaitForSeconds(1f);
        dos.SetActive(false);
        uno.SetActive(true);
        yield return new WaitForSeconds(1f);
        uno.SetActive(false);

        yield return new WaitForSeconds(1f);
        ScreenshotHandler.TakeScreenshot_Static(1080, 1920);
        cont++;


        yield return new WaitForSeconds(1f);
        tres.SetActive(true);
        yield return new WaitForSeconds(1f);
        tres.SetActive(false);
        dos.SetActive(true);
        yield return new WaitForSeconds(1f);
        dos.SetActive(false);
        uno.SetActive(true);
        yield return new WaitForSeconds(1f);
        uno.SetActive(false);

        yield return new WaitForSeconds(1f);
        ScreenshotHandler.TakeScreenshot_Static(1080, 1920);
        cont++;

        yield return new WaitForSeconds(1f);
        tres.SetActive(true);
        yield return new WaitForSeconds(1f);
        tres.SetActive(false);
        dos.SetActive(true);
        yield return new WaitForSeconds(1f);
        dos.SetActive(false);
        uno.SetActive(true);
        yield return new WaitForSeconds(1f);
        uno.SetActive(false);

        yield return new WaitForSeconds(1f);
        ScreenshotHandler.TakeScreenshot_Static(1080, 1920);
        cont++;

        Conteo.SetActive(false);
        cont = 0;

        yield return new WaitForSeconds(2f);

        
        
        canvas.GetComponent<Animator>().Play("Ir a Final");
        camaraDeFotos.SetActive(false);
        Animacion.SetActive(false);
    }

    public void Repetir()
    {
        revision.SetActive(false);
        Comenzar();
    }

    IEnumerator Termino()
    {
        SaveGame(rutaSaveData);

        participantes.Add(p);

        

        CargarParticipante();

        //GetComponent<MailSender>().EnviarMail();

        canvas.GetComponent<Animator>().Play("Ir a Agradecimientos");
        yield return new WaitForSeconds(5f);
        canvas.GetComponent<Animator>().Play("Reiniciar");
        
        WriteCVS();

    }
    public void Terminar()
    {
        StartCoroutine(Termino());
    }

    public void MostrarImagenes(string p, int size)
    {  
        string path = Application.persistentDataPath + "foto" + cont + ".png";

        Debug.Log(path);
    }

    public void ButtonSnapShot()
    {
        StartCoroutine("GuardaImagenCam");
    }

    private IEnumerator GuardaImagenCam()
    {
        yield return new WaitForEndOfFrame ();
        snap = new Texture2D (webCamTexture.width, webCamTexture.height);

        bytes = snap.EncodeToPNG();

        snap.SetPixels (webCamTexture.GetPixels());
        snap.Apply ();
        imagenShot[cont].texture = snap as Texture;
        System.IO.File.WriteAllBytes(savePath + fileName + cont.ToString() +".png", snap.EncodeToPNG());

        //captureCounter++;
    }

    public void WriteCVS()
    {
        if (participantes.Count > 0)
        {
            StreamWriter sw = new StreamWriter(rutaCVS, false);
            sw.WriteLine("Lista de Participantes (" + DateTime.Now + "):");
            sw.WriteLine();
            sw.Close();

            sw = new StreamWriter(rutaCVS, true);

            for (int i = 0; i < participantes.Count; i++)
            {
                sw.WriteLine((i + 1).ToString() + ") " + participantes[i].nombre + 
                    "  E-mail: " + participantes[i].mail + 
                    "  Edad: " + participantes[i].edad + 
                    " Acepto: " + participantes[i].acepto);
            }

            print("fin");

            sw.Close();
        }
    }


    public void SavesCount()
    {
        ReadCount();
        BinaryFormatter bf2 = new BinaryFormatter();
        saveFile2 = File.Create(rutaListas + "/saveCounts.bin");
        bf2.Serialize(saveFile2, saveCount);
        saveFile2.Close();
    }

    public void LoadCount()
    {
        BinaryFormatter bf2 = new BinaryFormatter();
        saveFile2 = File.Open(rutaListas + "/saveCounts.bin", FileMode.Open);
        saveCount = (SaveCount)bf2.Deserialize(saveFile2);
        saveFile2.Close();
        WriteCount();
    }

    public void ReadCount()
    {
        saveCount.contador = contador;
    }

    public void WriteCount()
    {
        contador = saveCount.contador;
        rutaSaveData = "/saveData_" + contador + ".bin";
        rutaCVS = rutaListas + " - Lista de participantes (" + contador + ").csv";
    }

    public void ReadPlayerData()
    {
        saveData.lista.Clear();
        for (int i = 0; i < participantes.Count; i++)
        {
            saveData.lista.Add(participantes[i]);
        }
    }

    public void SaveGame(string nombreDelArchivo)
    {
        ReadPlayerData();
        BinaryFormatter bf = new BinaryFormatter();
        saveFile = File.Create(rutaListas + nombreDelArchivo);
        bf.Serialize(saveFile, saveData);
        saveFile.Close();

        Debug.Log("Save Created :" + saveFile.Name);
    }

    public void LoadGame(string nombreDelArchivo)
    {
        BinaryFormatter bf = new BinaryFormatter();
        saveFile = File.Open(rutaListas + nombreDelArchivo, FileMode.Open);
        saveData = (SaveData)bf.Deserialize(saveFile);
        saveFile.Close();
        WritePlayerData();

        Debug.Log("Save Loaded :" + saveFile.Name);
    }

    public void WritePlayerData()
    {
        participantes.Clear();

        for (int i = 0; i < saveData.lista.Count; i++)
        {
            participantes.Add(saveData.lista[i]);
        }
    }

    void CheckSave()
    {
        //if (File.Exists(Application.persistentDataPath + "/saveData_" + contador + ".bin"))
        if (File.Exists(rutaListas + "/saveData_" + contador + ".bin"))
        {
            Debug.Log("Save File Found");
            fileExists = true;
        }
        else
        {
            fileExists = false;
            Debug.Log("No save file found");
        }
    }

    void CheckCount()
    {
        //if (File.Exists(Application.persistentDataPath + "/saveCounts.bin"))
        if (File.Exists(rutaListas + "/saveCounts.bin"))
        {
            Debug.Log("Count File Found");
            countExists = true;
        }
        else
        {
            countExists = false;
            Debug.Log("No count file found");
        }
    }

}



[System.Serializable]
public class Participante
{
    public string nombre;
    public string mail;
    public string edad;
    public string acepto;
    public string foto1;
    public string foto2;
    public string foto3;
    public string fecha;
    public string hora;
}

[System.Serializable]
public class RegistroParticipantes
{
    public List<Participante> jugadores = new List<Participante>();
}

[System.Serializable]
public class SaveData
{
    public List<Participante> lista = new List<Participante>();
    public string rutaFondo;
}

[System.Serializable]
public class SaveCount
{
    public int contador;

    public string nombreArchivo;
}
