using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
 {
  public Camera maincamera;
  private WebCamTexture webcamTexture;
  private const int FPS = 60;
  private DeviceOrientation currentorientation; //現在の方向
  private DeviceOrientation lastOrientation;//最後にデバイスが立った状態の姿勢
  private GUIStyle labelStyle;

  private float ratio;
  private float aspect;
  private string text;

  public int getWidth()
   {
    return webcamTexture.width;
   }

  public int getHeight()
   {
    return webcamTexture.height;
   }

  public float getRatio()
   {
  return ratio;
   }

  public float getAspect()
   {
    return aspect;
   }

  public float getOrthographicSize()
   {
    return maincamera.orthographicSize;
   }

  public DeviceOrientation getLastOrienation()
   {
    return lastOrientation;
   }
 
  public void pauseCamera() { webcamTexture.Pause(); }
  public void resumeCamera() { webcamTexture.Play();  }

 // Use this for initialization
  void Start ()
   {
  //フォント生成
  labelStyle = new GUIStyle();
  labelStyle.fontSize = Screen.height / 22;
  labelStyle.normal.textColor = Color.white;
 
    //カメラを取得
    maincamera = Camera.main;
    //アスペクト比初期化
    aspect = 0.0f;

    //現在の向きに無効な値を入れる
    lastOrientation = DeviceOrientation.Unknown;

    //カメラのテクスチャをQuadに載せる
    Renderer rend = GetComponent<Renderer>();
    if (WebCamTexture.devices.Length > 0)
     {
      WebCamDevice cam = WebCamTexture.devices[0];
      WebCamTexture wcam = new WebCamTexture(cam.name);
      wcam.Play();
      int width = wcam.width, height = wcam.height;

      if (width < 1280 || height < 720) { width *= 2; height *= 2; }
      webcamTexture = new WebCamTexture(cam.name, width, height, FPS);
      wcam.Stop();

      rend.material.mainTexture = webcamTexture;
      webcamTexture.Play();
     }

    //向きを初期化
    setAspectRatio();  
   }

 // Update is called once per frame
  void Update ()
   {
  /*
      if (aspect == 0.0f)//アスペクト比が初期化されていなかったら、初期化する
       {
        setAspectRatio();
       }

      //デバイスが立った状態なら、記録する
      if (Input.deviceOrientation != DeviceOrientation.FaceUp & Input.deviceOrientation != DeviceOrientation.FaceDown & Input.deviceOrientation == DeviceOrientation.Unknown)
       {
        lastOrientation = Input.deviceOrientation;
       }

     //向きが変わった場合
     if (Input.deviceOrientation != currentorientation)
       {
        setOrientation();
       }
  */
    if (Input.deviceOrientation != DeviceOrientation.FaceUp & Input.deviceOrientation != DeviceOrientation.FaceDown & Input.deviceOrientation != DeviceOrientation.Unknown)
     {
      lastOrientation = Input.deviceOrientation;
     }
    setAspectRatio();
    //デバイスが立った状態なら、記録する
    setOrientation();

    if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        // アプリケーション終了処理
        Application.Quit();
        return;
       }
     }
   }

  private void setAspectRatio()
   {
    if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft | Input.deviceOrientation == DeviceOrientation.LandscapeRight)
     {
      aspect = 1.0f / maincamera.aspect;
     }
    else if (Input.deviceOrientation == DeviceOrientation.Portrait | Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
     {
      aspect = maincamera.aspect;
     }
    else if (Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.Unknown)
     {
      if (lastOrientation != DeviceOrientation.Unknown)
       {
        if (lastOrientation == DeviceOrientation.LandscapeLeft | lastOrientation == DeviceOrientation.LandscapeRight)
         {  
          aspect = 1.0f / maincamera.aspect;
         }
        else if (lastOrientation == DeviceOrientation.Portrait | lastOrientation == DeviceOrientation.PortraitUpsideDown)
         {
          aspect = maincamera.aspect;
         }
       }
      else { aspect = maincamera.aspect; }//不明の時はPortraitのアスペクト比を使用する
     }
   }

  private void setOrientation()
   {
    // text = "size = " + maincamera.orthographicSize + "\naspect =" + aspect;
    //Quadを画面いっぱいに広げる
    float screenaspect = aspect;
    if (screenaspect == 0.0f) screenaspect = maincamera.aspect; //初期化されていない場合暫定値を使う

    float _h = maincamera.orthographicSize * 2;
    float _w = _h * screenaspect;

    DeviceOrientation orientation;
    if (Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.Unknown)
     {
      if (lastOrientation != DeviceOrientation.Unknown) orientation = lastOrientation;
      else orientation = DeviceOrientation.Portrait;
     }
    else
     {
      orientation= Input.deviceOrientation;
     }

    switch (orientation)
     {
      case DeviceOrientation.LandscapeLeft:
       ratio = _h / screenaspect;
       _w = _h / screenaspect;
       transform.localScale = new Vector3(_w, _h, 1);
       transform.localRotation = Quaternion.Euler(0, 0, 0);
       break;
      case DeviceOrientation.LandscapeRight:
       ratio = _h / screenaspect;
       _w = _h / screenaspect;
       transform.localScale = new Vector3(_w, _h, 1);
       transform.localRotation = Quaternion.Euler(0, 0, 180);//反転させる
       break;
      case DeviceOrientation.Portrait:
       ratio = _h / screenaspect;
       transform.localScale = new Vector3(_h, _w, 1);
       transform.localRotation = Quaternion.Euler(0, 0, -90);
       break;
      case DeviceOrientation.PortraitUpsideDown:
       ratio = _h / screenaspect;
       transform.localScale = new Vector3(_h, _w, 1);
       transform.localRotation = Quaternion.Euler(0, 0, 90);//逆向きなら回転させる
       break;
     }
   }
/*
  private void OnGUI()
   {
    GUI.Label(new Rect(50, 50, 400, 300), text, labelStyle);
   }
*/
 }
