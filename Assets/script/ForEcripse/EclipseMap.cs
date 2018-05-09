using System.Collections;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EclipseMap : MonoBehaviour, DataChangeReceiver
 {
  private const float DegToRad = Mathf.PI / 180.0f;
  private const int footmargin = 40;
  private const string appmenu = "UI/appmenu.png";
  private const string datafileprefix = "jsons/";
  private const string messageimage = "UI/mapmodehelp.png";
  private const string msgID = "EclipseMap_Help";
  private const string aboutmapmode = "　地図上に地球に映る月の影を表示します。地図はスワイプ動作で上下左右に動き、ピンチ動作で拡大・縮小することができます。\n　始まりから終わりまでの1分毎の影の変化を表示し、終了時刻の後は開始時刻に戻ってループ表示します。\n　モードの切り替え、表示する日食の変更はメニューダイアログから選択することができます。";
  //カメラ位置の経度・緯度
  private float radius = 10.0f;
  private float longitude = 135.0f;
  private float latitude = 35.0f;

  private GameObject container;
  private Camera maincam;
  private Clock clock;
  private ApplicationData appdata;

  //ユーザー操作関連
  private readonly string[] menuitems = new string[] { "太陽の方位を見る", "3Dモードで見る" ,"日食を選ぶ", "ヘルプ" };
  private delegate void userAction(); //ダイアログで選択された機能のメソッドのデリゲート
  private userAction[] actions;

 //マウス(スワイプ)関連
  private bool pressed; //マウスが押下されているか
  private float lastx;
  private float lasty;
  //コンポーネント
  private EclipseDataChooser chooser;
  private MapControll mapcontroller;
  //描画関連
  private Texture2D earthshadow;
  private Texture2D umbralshadow;
  private Texture2D result;
  private Texture2D appmenubutton;
  private UmbralShadowRenderer shadowrenderer;
  private EclipseData currenteclipsedata;
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
  //太陽・月のデータ
  private double[] posdata;


  void Start()
   {
    ready = false;
    menuvisible = false;
    helpvisible = false;
    //カメラ
    maincam = Camera.main;
    //時計
    clock = gameObject.GetComponent<Clock>();

    //メソッドポインタ初期化
    actions = new userAction[] { moveToARMode, moveTo3DMode, changeEclipseData, showHelpMessage };
    container = GameObject.Find("CameraContainer");

    //データ格納用配列
    posdata = new double[7];
    //アプリデータ
    appdata = ApplicationData.getApplicationData();
    //影を描くクラス
    shadowrenderer = new UmbralShadowRenderer();
    //データを選ぶクラス
    chooser = gameObject.GetComponent<EclipseDataChooser>();
    mapcontroller = gameObject.GetComponent<MapControll>(); 

    //テクスチャ
    earthshadow = new Texture2D(512, 512);
    StartCoroutine("getTextureAsset", appmenu);

    //地球モデルにテクスチャを貼る
    GameObject earth = GameObject.Find("MapBoard");
    Material[] mats = earth.GetComponent<Renderer>().materials;
    mats[1].SetTexture("_MainTex", earthshadow);

    //データセット
    //StartCoroutine("initEclipseDataFromAssetBundle", "20211125.json");
    //initEmptyEclipseData();
    initFromEclipseDataHolder();

    //初回のヘルプ表示は終わっているか
    if (!appdata.hasMapHelpShown)
     {
      showHelpMessage();
      appdata.hasMapHelpShown= true;
      appdata.saveApplicationData();
     }
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
      longitude = data.getInitialMapLongitude();
      latitude = data.getInitialMapLatitude();

      shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, UmbralShadowRenderer.PLAYMODE);
      setCameraPosition();
      ready = true;
     }
   }

  //保存されている日食データを初期化する
  private void initPresetEclipseData(string datafilename)
   {
   // Assetsフォルダから読み込む
    string path = Application.dataPath + "/eclipsedata/" + datafilename;

    StreamReader reader = new StreamReader(path);
    string jsonstring = reader.ReadToEnd();
    EclipseData data = null;

    try
     {
      data = JsonUtility.FromJson<EclipseData>(jsonstring);
      if (data != null && data.initDateTime())
       {
        currenteclipsedata = data;

        start = currenteclipsedata.getStartTime();
        finish = currenteclipsedata.getFinishTime();
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
        longitude = data.getInitialMapLongitude();
        latitude = data.getInitialMapLatitude();

        shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, UmbralShadowRenderer.PLAYMODE);
        //setCameraPosition();
        ready = true;
       }
      reader.Close();
     }
    catch (Exception e) {/* Debug.Log("readJson " + e.ToString());*/ }
   }

   //日食データをアセットバンドルから初期化する
  IEnumerator initEclipseDataFromAssetBundle(string datafilename)
   {
    string jsonstring = null;

    if (Application.platform == RuntimePlatform.WindowsEditor)
     {
      string path = Application.dataPath + "/StreamingAssets/" + datafileprefix + datafilename;
      StreamReader reader = new StreamReader(path);

      jsonstring  = reader.ReadToEnd();
      reader.Close();
     }
    else if (Application.platform == RuntimePlatform.Android)
     {
      string bundleUrl = Path.Combine("jar:file://" + Application.dataPath + "!/assets" + "/" , datafileprefix + datafilename);
      WWW www = new WWW(bundleUrl);
      yield return www;
   
      while (!www.isDone) { }
      jsonstring = www.text;
     }
    try
     {
      EclipseData data = JsonUtility.FromJson<EclipseData>(jsonstring);
      if (data != null && data.initDateTime())
       {
        EclipseDataHolder.setEclipseData(data);
        currenteclipsedata = data;

        start = currenteclipsedata.getStartTime();
        finish = currenteclipsedata.getFinishTime();
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
 
        shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, UmbralShadowRenderer.PLAYMODE);
        longitude = data.getInitialMapLongitude();
        latitude = data.getInitialMapLatitude();
        setCameraPosition();
        ready = true;
       }
     }
    catch (Exception e) { /*Debug.Log("readJson " + e.ToString()); */}
   }

  //空のデータクラスを初期化する
  private void initEmptyEclipseData()
   {
    //時刻はUTCで設定する
    int[] date = EclipseCalendar.schedule[8];
    finish = new DateTime(date[5], date[6], date[7], date[8], date[9], 0, DateTimeKind.Utc);
    start = new DateTime(date[0], date[1], date[2], date[3], date[4], 0, DateTimeKind.Utc);
    current = new DateTime(date[0], date[1], date[2], date[3], date[4], 0, DateTimeKind.Utc);

    currenteclipsedata = new EclipseData(start, finish, 1);

    EclipseDataHolder.setEclipseData(currenteclipsedata);

    umbralshadow = new Texture2D(earthshadow.width, earthshadow.height);
    result = new Texture2D(earthshadow.width, earthshadow.height);
 
    clearTexture(earthshadow);
    clearTexture(umbralshadow);
    clearTexture(result);
    shadowrenderer.setEclipseData(currenteclipsedata, earthshadow, umbralshadow, UmbralShadowRenderer.RECORDMODE);

    ready = true;
   }


  // Update is called once per frame
  void Update()
   {
    if (menuvisible | helpvisible | chooser.enabled) return;

    //処理開始
    interval += Time.deltaTime;
    if (interval > 0.1f & ready & !chooser.getEnabled())
     {
     //終了時刻よりあとの時刻か
      if (current.CompareTo(finish) > 0)
       {
        if (shadowrenderer.getMode()== UmbralShadowRenderer.RECORDMODE)
         {
          String filename = EclipseCalendar.getDateString(start.Year, start.Month, start.Day);

          currenteclipsedata.writeJSON(filename+ ".json");
          saveTexture(filename, result);
          if (umbralshadow != null) saveTexture(filename + "_umbra", umbralshadow);
          return;
          ready = false;
          interval = 0.0f;
         }
        else if(shadowrenderer.getMode() == UmbralShadowRenderer.PLAYMODE)
         {
          current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
         }
       }

      clock.setTime(current);
      //影描画
      shadowrenderer.drawLines(current);

      //テクスチャに結果を描き込む
      if(shadowrenderer.getMode() == UmbralShadowRenderer.RECORDMODE)
       {
        writeResult(earthshadow, result);
       }

      //地球の昼夜を描く
      currenteclipsedata.getPositions(current, posdata);
      shadowrenderer.drawNightSide(posdata);
 
      earthshadow.Apply();
      //時間を一つ進める
      current = current.AddMinutes(1.0);

      interval = 0.0f;
     }

    if (Application.platform == RuntimePlatform.Android & !chooser.enabled & !helpvisible)
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
    if (!chooser.getEnabled() & !helpvisible)
     {
      //メニューボタン表示
      if (GUI.Button(new Rect((Screen.width - appmenubutton.width) / 2, (Screen.height - appmenubutton.height - footmargin), appmenubutton.width, appmenubutton.height), appmenubutton))
       {
#if UNITY_ANDROID
        menuvisible = true;
        clock.enabled = false;
        mapcontroller.setOperationEnabled(false);

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


  private void setCameraPosition()
   {
    if (longitude < 0) longitude += 360.0f;

    GameObject mapboard = GameObject.Find("MapBoard");
    float mapwidth = mapboard.GetComponent<Renderer>().bounds.size.x;
    float mapheight = mapboard.GetComponent<Renderer>().bounds.size.y;
    float x = longitude / 360.0f * mapwidth;
    // Debug.Log("longitude  =" + longitude + " mapwidth = " + mapwidth + " x=" + x);
    float equator = mapheight / 2;
    float y = (latitude / 90.0f) * mapheight;
    //Debug.Log("latitude  =" + latitude + " mapheight= " + mapheight + " y=" + y);
    float camX = x - (mapwidth / 2);
    float camY = y;// - (mapheight / 2);

    Vector3 campos =  maincam.transform.position;
    campos.Set(camX, camY, campos.z);
    maincam.transform.position = campos;
   }

  private void saveTexture(string datafilename, Texture2D tex)
   {
    byte[] imagedata = tex.EncodeToPNG();
    string path = Application.dataPath + "/eclipsedata/" + datafilename+ ".png";
    File.WriteAllBytes(path, imagedata);
  /*    StreamWriter writer = new StreamWriter(path);
      writer.Write(imagedata);
      writer.Flush();
      writer.Close();
  */
   }


  public void EclipseDataChange(int number)
   {
    mapcontroller.setOperationEnabled(true);
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

  //テクスチャ記録
  private void writeResult(Texture2D src, Texture2D dst)
   {
    if (src.width != dst.width | src.height != dst.height) return;
    Color transparent = new Color(0, 0, 0, 0);
    Color boundscolor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    for (int y = 0; y < src.height; y++)
     {
      for (int x = 0; x < src.width; x++)
       {
        Color c = src.GetPixel(x, y);
        if (c == transparent | c == boundscolor) continue;
        dst.SetPixel(x, y, Color.white);
       }
     }
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
    if (datafilename == appmenu){ appmenubutton = tex; }
   }

  //Androidのメッセージダイアログを閉じるイベントを受け取る
  public void messageDialogClosed(string identifier)
   {
    if (identifier== msgID)
     {
      clock.enabled = true; 
      helpvisible = false;
      mapcontroller.setOperationEnabled(true);
     }
   }

  //AndroidのAlertDialogからのイベントを受け取る
  public void receiveButtonEvent(string identifier)
   {
    menuvisible = false;
    if (identifier == "CANCEL") { mapcontroller.setOperationEnabled(true); clock.enabled = true; return; }

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
  private void moveTo3DMode()
   {
    SceneManager.LoadScene("SolarEclipse");
   }

  //日食を選ぶ
  private void changeEclipseData()
   {
    chooser.enabled = true;
    mapcontroller.setOperationEnabled(false);

    clock.enabled = false;
    chooser.setEnabled(true, this);
   }

  //ヘルプの表示
  private void showHelpMessage()
   {
#if UNITY_ANDROID
    helpvisible = true;
    clock.enabled = false;
    mapcontroller.setOperationEnabled(false);

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
     "マップモードについて",//タイトル
     aboutmapmode,
     msgID);
  }));
#endif

   }

  //テクスチャの消去
  private void clearTexture(Texture2D tex)
   {
    Color transparent = new Color(0, 0, 0, 0);

    for (int y = 0; y < tex.height; y++)
     {
      for (int x = 0; x < tex.width; x++)
       {
        tex.SetPixel(x, y, transparent);
       }
     }
   }
 }
