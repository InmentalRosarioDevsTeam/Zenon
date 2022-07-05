using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.IO;

public class ScreenshotHandler : MonoBehaviour
{
    public static ScreenshotHandler instance;

    private Camera myCamera;
    private bool takeScreenshotOnNextFrame;

    public int contador = 0;



public RawImage[] imagenShot = new RawImage[3];
//private WebCamTexture webCamTexture;
private string fileName = "foto";
private string savePath = "Application.DataPath";
public int captureCounter = 0;
private byte[] bytes;
private Texture2D snap;




    private void Awake()
    {
        instance = this;
        myCamera = gameObject.GetComponent<Camera>();
        //contador = 0;
    }

    public void OnPostRender()
    {
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            /*ACA>>>>>*/ Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            //Sprite sp = Sprite.Create(renderResult, rect, new Vector2(0f, 0f), 100f);

            //imagen.GetComponent<Image>().sprite = sp;
           
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();

            int contador = GameObject.Find("GameManager_s").GetComponent<GameManager_s>().cont;

            string ruta = Application.dataPath + "/foto " + contador + ".png";

            System.IO.File.WriteAllBytes(ruta, byteArray);

            switch (contador)
            {
                case 0:
                    GameManager_s g = GameObject.Find("GameManager_s").GetComponent<GameManager_s>();
                    g.p.foto1 = ruta;
                    break;
                case 1:
                    GameManager_s g2 = GameObject.Find("GameManager_s").GetComponent<GameManager_s>();
                    g2.p.foto2 = ruta;
                    break;
                case 2:
                    GameManager_s g3 = GameObject.Find("GameManager_s").GetComponent<GameManager_s>();
                    g3.p.foto3 = ruta;
                    break;
            }
            
            // DESPUES CAMBIAR A DATA PATH
            //contador++; 


            snap = renderResult;
            bytes = snap.EncodeToPNG();

            //snap.SetPixels ();
            //snap.SetPixels(renderTexture.GetPixels());
            snap.Apply ();
            imagenShot[GameObject.Find("GameManager_s").GetComponent<GameManager_s>().cont].texture = snap as Texture;
            //System.IO.File.WriteAllBytes(savePath + fileName + captureCounter.ToString() +".png", snap.EncodeToPNG());

            //captureCounter++;

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;

        }
    }

    public void TakeScreenshot(int width, int height)
    {
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenshotOnNextFrame = true;
       
    }

    public static void TakeScreenshot_Static(int width, int height)
    {
        instance.TakeScreenshot(width, height);
    }

    
}
