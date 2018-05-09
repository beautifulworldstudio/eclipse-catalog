using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
 {
  private const int RETRY = -1;
  private const int STEP0 = 1;
  private const int STEP1 = 10;
  private const int STEP2 = 20;
  private const int STEP3 = 30;
  private const int FINISHED = 40;

  private const int verticalLine = 150;
  private const int horizontalLine = 90;
  private const int character1 = 190;
  private const int character2 = 224;
  private const int character3 = 258;
  private const int character4 = 292;
  private const int character5 = 326;
  private const int characterwidth = 30;
  private const int characterheight = 40;
  //画像関連の定数
  private readonly string[] imagenames = new string[] { "calibration/targetbox.png", "calibration/approvebutton.png",
  "calibration/retrybutton.png", "calibration/retry_xhdpi.png", "calibration/step0_xhdpi.png",
  "calibration/step1_xhdpi.png", "calibration/step2_xhdpi.png", "calibration/step3_xhdpi.png",
  "calibration/step4_xhdpi.png", "calibration/comma.png" };

  private GUIStyle ButtonLabelStyle;
  private const int TARGET = 0;
  private const int APPROVEBUTTON = 1;
  private const int RETRYBUTTON = 2;
  private const int RETRYMESSAGE = 3;
  private const int STEP0MESSAGE = 4;
  private const int STEP1MESSAGE = 5;
  private const int STEP2MESSAGE = 6;
  private const int STEP3MESSAGE = 7;
  private const int STEP4MESSAGE = 8;
  private const int COMMA = 9;

  private Texture2D[] images;
  private Texture2D[] numbers;
  private Texture2D minus;

  public Camera maincamera;
  WebCamTexture webcamTexture;
  const int FPS = 60;
  private GUIStyle labelStyle;
  private int state; //現在の状態
  private DeviceOrientation lastorientation; //現在の方向
  private DeviceOrientation orientation;

  private Rect displayposition; //目印のセット位置
  private Rect targetposition; //目印のセット位置
  private Rect messageposition;
  private Rect messagebox; //メッセージ表示位置  
  private string text;

  private Quaternion center;
  private Quaternion right;
  private Quaternion top;

  public static DeviceOrientation deviceOrientation;
  public static float horizontal;
  public static float vertical;
  public static bool calibrated = false;
  public static string movetoscene;
  //フラグ
  //public bool touched;
  //public bool menu;
  //public bool shutter;
  private bool ready;


  void Start()
   {
    ready = false;
    //フォント生成
    labelStyle = new GUIStyle();
    labelStyle.fontSize = Screen.height / 22;
    labelStyle.normal.textColor = Color.white;

    //カメラを取得
    maincamera = Camera.main;

    //状態を初期値にセット
    state = STEP0;
    displayposition = new Rect(Screen.width / 2 - 32, Screen.height / 2 - 32, 64, 64);
    messagebox = new Rect(50, 50, 400, 300);
    calibrated = false;

    //現在の向きに向き不明の値を入れる
    lastorientation = DeviceOrientation.Unknown;

    //タッチのフラグをリセット
    //touched = false;
    //イメージ読み込み
    images = new Texture2D[imagenames.Length];
    foreach(string path in imagenames)
     {
      StartCoroutine("getTextureAsset", path);
     }

    //数字読み込み
    numbers = new Texture2D[10];
    for (int i = 0; i < 10; i++)
     {
      //numbers[i] = (Texture2D)Resources.Load(i.ToString(), typeof(Texture2D));
      StartCoroutine("getTextureAsset", "numero/" + i.ToString() + ".png");
     }
    //comma = (Texture2D)Resources.Load("comma", typeof(Texture2D));
    StartCoroutine("getTextureAsset" ,"calibration/comma.png");
    //センサーを有効にする
    Input.compass.enabled = true;
    Input.gyro.enabled = true;

    //Quadを画面いっぱいに広げる
    float _h = maincamera.orthographicSize * 2;
    float _w = _h * maincamera.aspect;

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
   }

  void Update()
   {
    if(!ready)
     {
      foreach(Texture2D tex in images) if (tex == null) return;
      foreach (Texture2D tex in numbers) if (tex == null) return;
      ready = true;
     }

    bool shutterpressed = Input.GetKeyDown(KeyCode.F5);

    if (Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.Unknown)
     {
     }
    else//Lnadscape,Portraitの場合
     {
      if (shutterpressed)//シャッターボタン
       {
        switch (state)
         {
          case RETRY:
           state = STEP0;
           break;
          case STEP0:
           state = STEP1;
          // targetposition = new Rect(Screen.width / 2 - 32, Screen.height / 2 - 32, 64, 64);
           //getMessagePosition(caption1);
/*
           if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
             messageposition = new Rect((Screen.width - caption1.width) / 2, (Screen.height / 2 - caption1.height) / 2, caption1.width, caption1.height);
            }
           else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
             messageposition = new Rect((Screen.width / 2 - caption1.width) / 2, (Screen.height - caption1.height) / 2, caption1.width, caption1.height);
            }
*/
           break;
          case STEP1:
           state = STEP2;
           orientation = Input.deviceOrientation;
           //targetposition = new Rect(Screen.width - 64, Screen.height / 2 - 32, 64, 64);
           center = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
           //getMessagePosition(caption2);
/*
           if (orientation == DeviceOrientation.Portrait || orientation == DeviceOrientation.PortraitUpsideDown)
            {
             messageposition = new Rect((Screen.width - caption2.width) / 2, (Screen.height / 2 - caption2.height) / 2, caption1.width, caption2.height);
            }
           else if (orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight)
            {
             messageposition = new Rect((Screen.width / 2 - caption2.width) / 2, (Screen.height - caption2.height) / 2, caption1.width, caption2.height);
            }
*/
           break;
          case STEP2:
           state = STEP3;
          // targetposition = new Rect(Screen.width - 64, 0, 64, 64);
           right = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
           //getMessagePosition(caption3);
/*
           if (orientation == DeviceOrientation.Portrait || orientation == DeviceOrientation.PortraitUpsideDown)
            {
             messageposition = new Rect((Screen.width - caption3.width) / 2, (Screen.height / 2 - caption3.height) / 2, caption3.width, caption3.height);
            }
           else if (orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight)
            {
             messageposition = new Rect((Screen.width / 2 - caption3.width) / 2, (Screen.height - caption3.height) / 2, caption3.width, caption3.height);
            }
*/
           break;
          case STEP3:
           state = FINISHED;
           calibrated = true;
           //Quaternion gyro = new Quaternion(-Input.gyro.attitude.x, -Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
           //top = Quaternion.Euler(90.0f, 0.0f, 0.0f) * gyro; //カメラの向きでQuaternionを記録する
           top = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
           getScreenAngles();
           //getMessagePosition(caption4);
/*
           if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
             messageposition = new Rect((Screen.width - caption4.width) / 2, (Screen.height / 2 - caption4.height) / 2, caption1.width, caption4.height);
            }
           else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
             messageposition = new Rect((Screen.width / 2 - caption4.width) / 2, (Screen.height - caption4.height) / 2, caption1.width, caption4.height);
            }
*/
           break;
         }
       }
      //STEP1の最中に向きが変わったら、センター位置変更
      if( state == STEP1 & lastorientation != Input.deviceOrientation)
       {
        float horizontalsidelength = images[TARGET].width;
        float verticalsidelength = images[TARGET].height;
 
        targetposition = new Rect((Screen.height - horizontalsidelength)/ 2 , (Screen.width - verticalsidelength) / 2 - 32, horizontalsidelength, verticalsidelength);
       }
      lastorientation = Input.deviceOrientation;
     }

    //デバイスの向きが変わっている場合
    if (Input.deviceOrientation != orientation && (state > STEP1 & state < FINISHED))
     {
      //やり直し
      state = RETRY;
     }
/*
    //スマホ(Unity)が横ならそのまま
    if (Input.deviceOrientation != currentorientation)
     {
      if (Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.Unknown) return;
      currentorientation = Input.deviceOrientation;

      switch (currentorientation)
       {
        case DeviceOrientation.LandscapeLeft:
         if (state == STEP0)
          {
           displayposition = new Rect(Screen.height / 2 - 32, Screen.width / 2 - 32, 64, 64);
          }
         break;
        case DeviceOrientation.LandscapeRight:
         if (state == STEP0)
          {
           displayposition = new Rect(Screen.height / 2 - 32, Screen.width / 2 - 32, 64, 64);
          }
         break;
        case DeviceOrientation.Portrait:
         if (state == STEP0)
          {
           displayposition = new Rect(Screen.width / 2 - 32, Screen.height / 2 - 32, 64, 64);
          }
         break;
        case DeviceOrientation.PortraitUpsideDown:
         if (state == STEP0)
          {
           displayposition = new Rect(Screen.width / 2 - 32, Screen.height / 2 - 32, 64, 64);
          }
         break;
       }
     }
*/
    if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        webcamTexture.Stop();
        // アプリケーション終了
        Application.Quit();
        return;
       }
     }
   }

  void OnGUI()
   {
    switch(state)
     {
      case RETRY:
       getMessagePosition(Input.deviceOrientation, images[RETRYMESSAGE]);
       GUI.DrawTexture(messageposition, images[RETRYMESSAGE]);
       break;
      case STEP0:
       getMessagePosition(Input.deviceOrientation, images[STEP0MESSAGE]);
       GUI.DrawTexture(messageposition, images[STEP0MESSAGE]);
       break;
      case STEP1:
       targetposition = new Rect((Screen.width - images[TARGET].width)/ 2, Screen.height / 2 - 32, images[TARGET].width, images[TARGET].height);
       getMessagePosition(Input.deviceOrientation, images[STEP1MESSAGE]);
       GUI.DrawTexture(messageposition, images[STEP1MESSAGE]);
       break;
      case STEP2:
       targetposition = new Rect(Screen.width - images[TARGET].width, (Screen.height - images[TARGET].height)/ 2, images[TARGET].width, images[TARGET].height);
       getMessagePosition(orientation, images[STEP2MESSAGE]); GUI.DrawTexture(messageposition, images[STEP2MESSAGE]);
       break;
      case STEP3:
       targetposition = new Rect(Screen.width - images[TARGET].width, 0, images[TARGET].width, images[TARGET].height);
       getMessagePosition(orientation, images[STEP3MESSAGE]);
       GUI.DrawTexture(messageposition, images[STEP3MESSAGE]);
       break;
      case FINISHED:
       getMessagePosition(Input.deviceOrientation, images[STEP4MESSAGE]);
       GUI.DrawTexture(messageposition, images[STEP4MESSAGE]);
       float left = messageposition.xMin;
       float top = messageposition.yMin;
       //水平画角の書き込み
       int value = (int)horizontal;
       if(value >= 100) value -= ((value / 100) * 100);
       if ((value / 10) != 0) GUI.DrawTexture(new Rect(left + character1, top + horizontalLine, characterwidth, characterheight), numbers[value / 10]);
       GUI.DrawTexture(new Rect(left + character2, top + horizontalLine, characterwidth, characterheight), numbers[value % 10]);
       GUI.DrawTexture(new Rect(left + character3, top + horizontalLine, characterwidth, characterheight),images[COMMA]);
       value = (int)((horizontal - Mathf.Floor(horizontal)) * 100.0f);
       GUI.DrawTexture(new Rect(left + character4, top + horizontalLine, characterwidth, characterheight), numbers[value / 10]);
       GUI.DrawTexture(new Rect(left + character5, top + horizontalLine, characterwidth, characterheight), numbers[value % 10]);
       //垂直画角の書き込み
       value = (int)vertical;
       if (value >= 100) value -= ((value / 100) * 100);
       if ((value / 10) != 0) GUI.DrawTexture(new Rect(left + character1, top + verticalLine, characterwidth, characterheight), numbers[value / 10]);
       GUI.DrawTexture(new Rect(left + character2, top + verticalLine, characterwidth, characterheight), numbers[value % 10]);
       GUI.DrawTexture(new Rect(left + character3, top + verticalLine, characterwidth, characterheight), images[COMMA]);
       value = (int)((vertical - Mathf.Floor(vertical)) * 100.0f);
       GUI.DrawTexture(new Rect(left + character4, top + verticalLine, characterwidth, characterheight), numbers[value / 10]);
       GUI.DrawTexture(new Rect(left + character5, top + verticalLine, characterwidth, characterheight), numbers[value % 10]);
       if (Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.Unknown)
        {
         drawDecideButtons(lastorientation);
        }
       else drawDecideButtons(Input.deviceOrientation);
       break;
     }

    if (state > STEP0 && state < FINISHED)
     {
      GUI.DrawTexture(targetposition, images[TARGET]);
     }
 /*
    switch(state)
     {
      case STEP0:
       if (GUI.Button(new Rect(100, 100, 200, 150), startbutton))
        {
         state = STEP1;
         text = "水平の画角を計算します\n右端の四角に同じ目印が入るように動かしてください。";
         displayposition = new Rect(Screen.width - 64, Screen.height / 2 - 32, 64, 64);

     //Quaternion gyro = new Quaternion(-Input.gyro.attitude.x, -Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
     //center = Quaternion.Euler(90.0f, 0.0f, 0.0f) * gyro; //カメラの向きでQuaternionを記録する
         center = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
        }
       break;
      case STEP1:
       if (GUI.Button(new Rect(100, 100, 400, 150), "右端をセット"))
        {
         state = STEP2;
         text = "垂直の画角を計算します\n上端の四角に同じ目印が入るように動かしてください。";
         displayposition = new Rect(Screen.width - 64, 0, 64, 64);

     // Quaternion gyro = new Quaternion(-Input.gyro.attitude.x, -Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
     // right = Quaternion.Euler(90.0f, 0.0f, 0.0f) * gyro; //カメラの向きでQuaternionを記録する
         right = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
        }
    break;
      case STEP2:
       if (GUI.Button(new Rect(100, 100, 400, 150), "上端をセット"))
        {
         state = FINISHED;
         calibrated = true;
     //Quaternion gyro = new Quaternion(-Input.gyro.attitude.x, -Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
     //top = Quaternion.Euler(90.0f, 0.0f, 0.0f) * gyro; //カメラの向きでQuaternionを記録する
         top = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w);
         getScreenAngles();
        }
       break;
      case FINISHED:
       if (GUI.Button(new Rect(100, 100, 400, 200), "終了"))
        {
         webcamTexture.Stop();
         Object.DestroyImmediate(webcamTexture);
         SceneManager.LoadScene("landmark");
        }
       break;
     }
*/
   }


  private void drawDecideButtons(DeviceOrientation screenorientation)
   {
    if (screenorientation == DeviceOrientation.Portrait || screenorientation == DeviceOrientation.PortraitUpsideDown)
     {
      if (GUI.Button(new Rect((Screen.width - images[APPROVEBUTTON].width) / 2, Screen.height / 2 + 50, images[APPROVEBUTTON].width, images[APPROVEBUTTON].height), images[APPROVEBUTTON]))
       {
        finishCalibration();
       }
      if (GUI.Button(new Rect((Screen.width - images[RETRYBUTTON].width) / 2, Screen.height / 2 + 250, images[RETRYBUTTON].width, images[RETRYBUTTON].height), images[RETRYBUTTON]))
       {
        state = STEP0;
       }
     }
    else if (screenorientation == DeviceOrientation.LandscapeLeft || screenorientation == DeviceOrientation.LandscapeRight)
     {
      if (GUI.Button(new Rect(Screen.width / 2 + (Screen.width / 2 - images[APPROVEBUTTON].width) / 2, 50, images[APPROVEBUTTON].width, images[APPROVEBUTTON].height), images[APPROVEBUTTON]))
       {
        finishCalibration();
       }
      if (GUI.Button(new Rect(Screen.width / 2 + (Screen.width / 2 - images[RETRYBUTTON].width) / 2, 250, images[RETRYBUTTON].width, images[RETRYBUTTON].height), images[RETRYBUTTON]))
       {
        state = STEP0;
       }
     }
   }

  private void getMessagePosition(DeviceOrientation messageorientation,Texture2D message)
   {
    if (messageorientation == DeviceOrientation.Portrait || messageorientation == DeviceOrientation.PortraitUpsideDown)
     {
      messageInPortaritMode(message);
     } 
    else if (messageorientation == DeviceOrientation.LandscapeLeft || messageorientation == DeviceOrientation.LandscapeRight)
     {
      messageInLandscapeMode(message);
     }
    else 
     {
      if (lastorientation != DeviceOrientation.Unknown)
       {
        if (lastorientation == DeviceOrientation.Portrait || lastorientation == DeviceOrientation.PortraitUpsideDown)
         {
          messageInPortaritMode(message);
         }
        else if (messageorientation == DeviceOrientation.LandscapeLeft || messageorientation == DeviceOrientation.LandscapeRight)
         {
          messageInLandscapeMode(message);
         }
       }
      else
       {
        messageInPortaritMode(message);
       }
     }
   }

  private void messageInPortaritMode(Texture2D message)
   {
    messageposition = new Rect((Screen.width - message.width) / 2, (Screen.height / 2 - message.height) / 2, message.width, message.height);
   }

  private void messageInLandscapeMode(Texture2D message)
   {
    messageposition = new Rect((Screen.width / 2 - message.width) / 2, (Screen.height - message.height) / 2, message.width, message.height);
   }

 private void getScreenAngles()
   {
    horizontal = Quaternion.Angle(center, right);
    vertical = Quaternion.Angle(top, right);
    deviceOrientation = orientation;
   }

  private void finishCalibration()
   {
    //センサーを無効にする
    Input.compass.enabled = false;
    Input.gyro.enabled = false;

    webcamTexture.Stop();
    UnityEngine.Object.DestroyImmediate(webcamTexture);
    SceneManager.LoadScene(movetoscene);
   }

 //テクスチャ読み込み
  IEnumerator getTextureAsset(string datafilename)
   {
    if (Application.platform == RuntimePlatform.WindowsEditor)
     {
      string path = Application.dataPath + "/StreamingAssets/" + datafilename; ;
      yield return null;

      try
       {
        byte[] data = File.ReadAllBytes(path);
        if (data != null && data.Length != 0)
         {
          int pos = 16, width = 0, height = 0;
          for (int i = 0; i < 4; i++) width = width * 256 + data[pos++];
          for (int i = 0; i < 4; i++) height = height * 256 + data[pos++];
          Texture2D texture = new Texture2D(width, height);
          texture.LoadImage(data);
          setTexture(texture, datafilename);
         }
       }
      catch (Exception e) { }
     }
    else if (Application.platform == RuntimePlatform.Android)
     {
      string bundleUrl = Path.Combine("jar:file://" + Application.dataPath + "!/assets" + "/", datafilename);
      WWW www = new WWW(bundleUrl);
      yield return www;
      while (!www.isDone) { }
      setTexture(www.texture, datafilename);
     }
   }


  //読み込んだテクスチャを変数に格納する
  private void setTexture(Texture2D tex, string datafilename)
   {
    for (int i = 0; i < imagenames.Length; i++)
     {
      if (datafilename == imagenames[i]) {images[i] = tex; return; }
     }

    for (int i = 0; i < 10; i++)
     {
      if (datafilename == ("numero/" + i.ToString() + ".png")) { numbers[i] = tex; return; }
     }
   }
 }
