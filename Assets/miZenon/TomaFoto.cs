using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.IO;

public class TomaFoto : MonoBehaviour
{
public RawImage[] imagenShot = new RawImage[3];
private WebCamTexture webCamTexture;
private string fileName = "foto";
private string savePath = "Application.DataPath";
private int captureCounter = 0;
private byte[] bytes;
private Texture2D snap;


 private Camera myCamera;
    // Start is called before the first frame update
    void Start()
    {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

    }

   public void ButtonSnapShot()
{

StartCoroutine("GuardaImagenCam");
}
private IEnumerator GuardaImagenCam()
{
yield return new WaitForEndOfFrame ();
snap = new Texture2D (webCamTexture.width, webCamTexture.height);
//snap = new Texture2D (myCamera.width, myCamera.height);

bytes = snap.EncodeToPNG();

snap.SetPixels (webCamTexture.GetPixels());
//snap.SetPixels (myCamera.GetPixels());
snap.Apply ();
imagenShot[captureCounter].texture = snap as Texture;
System.IO.File.WriteAllBytes(savePath + fileName + captureCounter.ToString() +".png", snap.EncodeToPNG());

captureCounter++;
}

}
