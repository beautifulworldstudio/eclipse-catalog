using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EclipseView : MonoBehaviour, DataChangeReceiver
 {
  private const float DegToRad = Mathf.PI / 180.0f;
  private const float addtion_lon = 0.1f;
  private const float addition_lat = 0.1f;
  private const float leastAngle = 7.0f;
  private const float largestAngle = 60.0f;
  private const string sliderimage = "slider.png";
  private const string mapmodeimage = "Mapmodebutton";
  private const string appmenu = "UI/appmenu.png";
  private const string datafileprefix = "jsons/";
  private const string about3Dmode = "　地球の３Ｄモデルに月の影を表示します。地球のモデルはスワイプ動作で上下左右に回転し、ピンチ動作で拡大・縮小することができます。\n　始まりから終わりまでの1分毎の影の変化を表示し、終了時刻の後は開始時刻に戻ってループ表示します。\n　モードの切り替え、表示する日食の変更はメニューダイアログから選択することができます。";
  private const string msgID = "Eclipse3D_Help";

 //ユーザー操作関連
  private readonly string[] menuitems = new string[] { "太陽の方位を見る", "マップモードで見る", "日食を選ぶ" , "ヘルプ" };
  private delegate void userAction(); //ダイアログで選択された機能のメソッドのデリゲート
  private userAction[] actions;

  private float radius = 10.0f;
  private float longitude = 135.0f;
  private float latitude = 35.0f;

  private GameObject sunlight; //太陽の役割のライト
  private EclipseDataChooser datachooser;
  private Clock clock;
  private ApplicationData appdata;
  //マウス(スワイプ)関連
  private bool pressed; //マウスが押下されているか
  private bool slidertapped;
  private float lastx;
  private float lasty;
  //ピンチ操作関連
  private float lastRadius;
  private int lastTouchCount;
  //描画関連
  private Texture2D earthshadow;
  private Texture2D appmenubutton;
  private UmbralShadowRenderer shadowrenderer;
  private EclipseData currenteclipsedata;
 //カメラ位置
  private Camera maincam;
  private GameObject camcontainer;
  private Vector3 camposition;
  private float initialLongitude;
  private float initialLatiude;
  //時間関連
  private DateTime start;
  private DateTime finish;
  private DateTime current;
  //ループ制御
  private float interval;
  //フラグ
  private bool ready;
  private bool menuvisible;
  private bool helpvisible;
  private bool pinch;

  private double[] posdata;
  public string datafile;

   // Use this for initialization
  void Start ()
   {
    ready = false;
    menuvisible = false;
 
    //メソッドポインタ初期化
    actions = new userAction[] { moveToARMode, moveToMapMode, changeEclipseData, showHelpMessage };

    //カメラ関連変数初期化
    maincam = Camera.main;
    camcontainer = GameObject.Find("CameraContainer");
    camposition = new Vector3();

    //アプリデータ
    appdata = ApplicationData.getApplicationData();
    //データ選択クラス
    datachooser = gameObject.GetComponent<EclipseDataChooser>();
    //時計
    clock = gameObject.GetComponent<Clock>();
    //データ格納用配列
    posdata = new double[7];
    //位置の初期設定を行う
    positionUpdated(0.0f, 0.0f);

    //影を描くクラス
    shadowrenderer = new UmbralShadowRenderer();

    //日光
    sunlight = GameObject.Find("SunLight");

    //地球の影を描くテクスチャ
    earthshadow = new Texture2D(512, 512);
    StartCoroutine("getTextureAsset", appmenu);

    //地球モデルのサイズ
    //Debug.Log("earthsize = "+ earth.GetComponent<Renderer>().bounds.size.x);
    //地球モデルにテクスチャを張る
    GameObject earth = GameObject.Find("perfectsphere");
    Material[] mats = earth.GetComponent<Renderer>().materials;
    mats[1].SetTexture("_MainTex", earthshadow);

    //データセット
    //StartCoroutine("initEclipseDataFromAssetBundle", "20301125.json");
    //initEclipseData(datafile);
    initFromEclipseDataHolder();

  //初回のヘルプ表示は終わっているか
    if (!appdata.has3DHelpShown)
     {
      showHelpMessage();
      appdata.has3DHelpShown = true;
      appdata.saveApplicationData();
     }
   }
  

  private void initEclipseData(string datafilename)
   {
  // Assetsフォルダから読み込む
    string path = Application.dataPath + "/eclipsedata/" + datafilename;
    //Debug.Log(path);

    StreamReader reader = new StreamReader(path);
    string jsonstring = reader.ReadToEnd();

    try
     {
      EclipseData data = JsonUtility.FromJson<EclipseData>(jsonstring);
      if (data != null && data.initDateTime())
       {
        currenteclipsedata = data;
        EclipseDataHolder.setEclipseData(data);

        start = currenteclipsedata.getStartTime();
        finish = currenteclipsedata.getFinishTime();
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);

        shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, UmbralShadowRenderer.PLAYMODE);
        longitude = data.getInitialCameraLongitude();
        latitude = data.getInitialCameraLatitude();
        positionUpdated(0.0f, 0.0f);
        ready = true;
       }
       //reader.Close();
     }
    catch (Exception e) { /*Debug.Log("readJson " + e.ToString()); */}
   }


  //日食データをアセットバンドルから初期化する
  IEnumerator initEclipseDataFromAssetBundle(string datafilename)
   {
    string jsonstring = null;
 
    if (Application.platform == RuntimePlatform.WindowsEditor)
     {

      string path = Application.dataPath + "/StreamingAssets/" + datafileprefix + datafilename;
      StreamReader reader = new StreamReader(path);

      jsonstring = reader.ReadToEnd();
      reader.Close();
     }
    else if (Application.platform == RuntimePlatform.Android)
     {
      string bundleUrl = Path.Combine("jar:file://" + Application.dataPath + "!/assets" + "/", datafileprefix + datafilename);
      WWW www = new WWW(bundleUrl);
      yield return www;

      while (!www.isDone) { }
      jsonstring = www.text;
      //text = "count =" + www.text.Length.ToString();
     }
    try
     {
      EclipseData data = JsonUtility.FromJson<EclipseData>(jsonstring);
      if (data != null && data.initDateTime())
       {
        currenteclipsedata = data;
        EclipseDataHolder.setEclipseData(currenteclipsedata);

        start = currenteclipsedata.getStartTime();
        finish = currenteclipsedata.getFinishTime();
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);

        shadowrenderer.setEclipseData(currenteclipsedata, earthshadow,UmbralShadowRenderer.PLAYMODE);
        longitude = data.getInitialCameraLongitude();
        latitude = data.getInitialCameraLatitude();
        positionUpdated(0.0f, 0.0f);
        ready = true;
       }
      //reader.Close();
     }
    catch (Exception e) { /*Debug.Log("readJson " + e.ToString());*/ }
   }


 //EclipseDataHolderからデータを読み込む
  private void initFromEclipseDataHolder()
   {
    EclipseData data = EclipseDataHolder.getEclipseData();

    if (data != null)
     {
      currenteclipsedata = data;

      start = currenteclipsedata.getStartTime();
      finish = currenteclipsedata.getFinishTime();
      current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
      longitude = data.getInitialCameraLongitude();
      latitude = data.getInitialCameraLatitude();

      shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, UmbralShadowRenderer.PLAYMODE);
      positionUpdated(0.0f, 0.0f);
      ready = true;
     }
   }

  // Update is called once per frame
  void Update()
   {
    if (datachooser.enabled | helpvisible) return;

    //処理開始
    interval += Time.deltaTime;  
    if (interval > 0.1f & ready)
     {
      currenteclipsedata.getPositions(current, posdata);

      //影描画
      shadowrenderer.drawLines(current);
      earthshadow.Apply();

      //時計に時間を通知
      clock.setTime(current);

     //太陽位置を計算してライトの位置と向きを変更する
      {
       //恒星時をもとに、背景の回転を行う（恒星時は春分点の時角)
       Material skybox = RenderSettings.skybox;
       skybox.SetFloat("_Rotation", (float)-posdata[EclipseData.PHAI]);//時角のマイナス方向に回転。skyboxのマテリアルは左右が逆

       //赤緯・赤経は北極から見て時計回り。時角、恒星時は反時計回り。時角に合わせて計算する
       //float ramda = -(float)((-asc + phai0) * DegToRad);これを書き換えて下の式になる
       float ramda = (float)((posdata[EclipseData.SUN_ASC] - posdata[EclipseData.PHAI]) * DegToRad);
       float psy = (float)(posdata[EclipseData.SUN_DEC] * DegToRad);
       float sundistance = 400;
       float x = Mathf.Cos(psy) * Mathf.Cos(ramda) * sundistance;
       float y = Mathf.Cos(psy) * Mathf.Sin(ramda) * sundistance;
       float z = Mathf.Sin(psy) * sundistance;

       Vector3 sunpos = sunlight.transform.position;
       sunpos.Set(x, z, y);
       sunlight.transform.position = sunpos;
       sunpos.Normalize();
       sunpos *= -1;
       sunlight.transform.forward = sunpos;
      }

      //時間を一つ進める
      current = current.AddMinutes(1.0);
      //終了時刻よりあとの時刻か
      if (current.CompareTo(finish) > 0)
       {
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
       }
      interval = 0.0f;
     }

    //ピンチ操作
    //タッチによるスワイプを取得する
    //画面の縮小拡大を取得する
    if(Application.platform == RuntimePlatform.Android)
     {
    if (Input.touchCount == 0)
     {
      //windows　の場合はコメントアウト
      if (pinch) pinch = false;
      if (pressed) pressed = false;
     }
    else if (Input.touchCount == 1 & lastTouchCount > 1)
     {
      if (pinch) pinch = false;
      Touch[] touches = Input.touches;
      Vector2 fingerpos = touches[0].position;
      lastx = fingerpos.x;
      lasty = fingerpos.y; //画面は左上が原点、マウスは左下が原点。なぜかタッチは左上原点のようだ。
      pressed = true;
      lastTouchCount = Input.touchCount;
     }
    else if (Input.touchCount > 1)
     {
      if (pressed) { pressed = false; }
      if (!pinch) { pinch = true; lastRadius = 0.0f; lastTouchCount = Input.touchCount; }

      float Xamount = 0.0f;
      float Yamount = 0.0f;

      Touch[] alltouch = Input.touches;

      foreach (Touch t in alltouch)
       {
        Vector2 pos = t.position;
        Xamount += pos.x;
        Yamount += pos.y;
       }
      float Xcenter = Xamount / Input.touchCount;
      float Ycenter = Yamount / Input.touchCount;
      float largestRadius = 0.0f;

      foreach (Touch t in alltouch)
       {
        Vector2 pos = t.position;
        float xdiff = pos.x - Xcenter;
        float ydiff = pos.y - Ycenter;
        float length = Mathf.Sqrt(xdiff * xdiff + ydiff * ydiff);
        if (length > largestRadius) largestRadius = length;
       }

      if (lastRadius != 0.0f & lastTouchCount == Input.touchCount)
       {
        //カメラ画角の変更
        float angleaddition = (lastRadius - largestRadius) * 0.05f;
        maincam.fieldOfView = maincam.fieldOfView + angleaddition;
        //画角の最小・最大角度はここでコントロールする
        if (maincam.fieldOfView <= leastAngle) { maincam.fieldOfView = leastAngle; }
        else if (maincam.fieldOfView >= largestAngle) { maincam.fieldOfView = largestAngle; }
       }
      lastTouchCount = Input.touchCount;
      lastRadius = largestRadius;
     }
     }
    //ピンチ操作・終わり

    //マウス検知
    if (Input.GetMouseButtonDown(0))
     {
      Vector3 mousepos = Input.mousePosition;
      float x = mousepos.x;
      float y = Screen.height - mousepos.y; //画面は左上が原点、マウス（タッチ）は左下が原点

      if (!pressed)
       {
        pressed = true;
       }
      lastx = mousepos.x;
      lasty = mousepos.y;
     }

    //左ボタン上がった
    if (Input.GetMouseButtonUp(0))
     {
      if (pressed) pressed = false;
     //if (slidertapped) slidertapped = false;
     }

    if (pressed)
     {
      float xdiff = Input.mousePosition.x - lastx;
      float ydiff = Input.mousePosition.y - lasty;

      if (xdiff != 0.0f | ydiff != 0.0f) positionUpdated(xdiff, ydiff);

      lastx = Input.mousePosition.x;
      lasty = Input.mousePosition.y;
     }

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

  void OnGUI()
   {
    if (datachooser.enabled) return;

    if(Application.platform == RuntimePlatform.Android)
     {
      //メニューボタン表示
      if(GUI.Button( new Rect((Screen.width - appmenubutton.width) /2, (Screen.height - appmenubutton.height), appmenubutton.width, appmenubutton.height), appmenubutton) )
       {
#if UNITY_ANDROID
     menuvisible = true;
     clock.enabled = false;

     // Javaのオブジェクトを作成
     AndroidJavaClass nativeDialog = new AndroidJavaClass("studio.beautifulworld.dialoglibrary.MenuDialog");

     // Context(Activity)オブジェクトを取得する
     AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
     AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

     // AndroidのUIスレッドで動かす
     context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
     // ダイアログ表示のstaticメソッドを呼び出す
     nativeDialog.CallStatic(
        "showButtons",
        context,
        "希望の動作を選んでください",
        menuitems);
      }));
#endif
       }
     } 
   }


 //EclipseDataChooserからの変更指示
  public void EclipseDataChange(int number)
   {
    clock.enabled = true;

    //日食データの変更を行う
    int year = EclipseCalendar.schedule[number][0];
    int month = EclipseCalendar.schedule[number][1];
    int day = EclipseCalendar.schedule[number][2];

    DateTime startdate = EclipseDataHolder.getEclipseData().getStartTime();
    if (year == startdate.Year & month == startdate.Month & day == startdate.Day) return;

    string datafilename = EclipseCalendar.getDateString(year, month, day) + ".json";
    
    StartCoroutine("initEclipseDataFromAssetBundle", datafilename);
   }


  //カメラ位置変更
  private void positionUpdated(float xdiff, float ydiff)
   {
    longitude -= (xdiff * addtion_lon);
    latitude -= (ydiff * addition_lat);

    camposition.Set(Mathf.Cos(latitude * DegToRad) * radius * Mathf.Cos(longitude * DegToRad), Mathf.Sin(latitude * DegToRad) * radius, Mathf.Cos(latitude * DegToRad) * radius * Mathf.Sin(longitude * DegToRad));
    camcontainer.transform.position = camposition;
    camcontainer.transform.forward = new Vector3(-camposition.x, -camposition.y, -camposition.z);
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
    switch(datafilename)
     {
     // case sliderimage: slider = tex; break;
     // case mapmodeimage: mapmodebutton = tex; break;
      case appmenu: appmenubutton = tex; break;
     }
   }


  //Androidのメッセージダイアログを閉じるイベントを受け取る
  public void messageDialogClosed(string identifier)
   {
    if (identifier== msgID)
     {
      clock.enabled = true; 
      helpvisible = false;
     }
   }

 　//AndroidのAlertDialogからのイベントを受け取る
  public void receiveButtonEvent(string identifier)
   {
    menuvisible = false;
    if (identifier == "CANCEL") { clock.enabled = true;  return; }

    for (int i = 0; i < menuitems.Length; i++)
     {
      if (menuitems[i] == identifier)
       {
        actions[i]();
        break;
       }
     }
   }

  //ユーザー操作の実行
  //ARモードへ
  private void moveToARMode()
   {
    SceneManager.LoadScene("EclipseAR");
   }

  //マップモードヘ
  private void moveToMapMode()
   {
    SceneManager.LoadScene("eclipsemap");
   }

  //日食を選ぶ
  private void changeEclipseData()
   {
    datachooser.enabled = true;
    clock.enabled = false;
    datachooser.setEnabled(true, this);
   }

 //ヘルプの表示
  private void showHelpMessage()
   {
    if (Application.platform == RuntimePlatform.Android)
     {
#if UNITY_ANDROID
    helpvisible = true;
    clock.enabled = false;

    // Javaのオブジェクトを作成
    AndroidJavaClass nativeDialog = new AndroidJavaClass("studio.beautifulworld.dialoglibrary.MenuDialog");

    // Context(Activity)オブジェクトを取得する
    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

    // AndroidのUIスレッドで動かす
    context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
     // ダイアログ表示のstaticメソッドを呼び出す
     nativeDialog.CallStatic(
     "showMessageEx",
     context,
     "3Dモードについて",//タイトル
     about3Dmode,
     msgID);
  }));
#endif
     }
   }
 }
