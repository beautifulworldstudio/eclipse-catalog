using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EclipseARprojecter : MonoBehaviour, DataChangeReceiver, PointChangeReceiver
 {
  private const float RadToDeg = 180.0f / Mathf.PI;
  private const int UNDER_HORIZON = 2;
  private const int INSIDE_SCREEN = 3;
  private const int OUTSIDE_SCREEN = 4; 
  private const int LOCATION_UNAVAILABLE = 5;
  //画像インデックス
  private const int MENUBUTTON = 0;
  private const int ALERT_UNDERHORIZON = 1;
  private const int SUNPOSITION = 2;
  private const int ALERT_LOCATION = 3;
  private const int SUNIMAGE = 4;
  private const int MINUS = 5;
  //観測地点モード
  private const int CURRENT_LOCATION = 10;
  private const int MAP_LOCATION = 20;
 
  private const int footmargin = 40;
  //数値表示の定数
  private const int direction_baseline = 22;
  private const int altitude_baseline = 84;
  private const int longitude_baseline = 137;
  private const int latitude_baseline = 195;
  private readonly int[] orders = new int[] { 243, 275, 301, 335, 388, 418 };
  private const int numberwidth = 30;
  private const int numberheight = 40;

  private const string datafileprefix = "jsons/";
  private const string aboutARmode = "　カメラ画像の上に太陽の位置を表示します。始まりから終わりまでの1分毎の位置の変化を表示し、終了時刻の後は開始時刻に戻ってループ表示します。\n　太陽を見る観測地点は位置情報を使用した「現在の地点から見る」と指定の位置から見る「地図で地点を選ぶ」をメニューから選んでください。\n　モードの切り替え、表示する日食の変更、画角の測定（キャリブレーション）はメニューダイアログから選択することができます。";
  private const string msgID = "EclipseAR_Help";

  //テクスチャのpath
  private readonly string[] imagepaths = new string[]
   { "UI/appmenu.png" , "UI/message_sun_is_under_horizon.png",
     "UI/message_sun_position.png", "UI/message_location_is_unavailable.png",
     "sunimage.png", "numero/minus.png" };
  private readonly string[] signboardpaths = new string[]
   { "signboard/up.png", "signboard/leftup.png",
     "signboard/left.png", "signboard/leftdown.png",
     "signboard/down.png", "signboard/rightdown.png",
     "signboard/right.png","signboard/rightup.png" };

  private Camera maincam;
  private CameraView camview;

  private Clock clock;
  private EclipseDataChooser chooser;
  private ARPointChooser pointchooser;
  private ApplicationData appdata;

  //ユーザー操作関連
  private readonly string[] menuitems = new string[] { "マップモードでみる", "3Dモードで見る", "日食を選ぶ", "現在地点から見る", "地図で地点を選ぶ", "キャリブレーション" , "ヘルプ" };
  private delegate void userAction(); //ダイアログで選択された機能のメソッドのデリゲート
  private userAction[] actions;
  //観測地点関連
  private int locationmode;
  private float longitude;
  private float latitude;
  //磁気偏角
  private Quaternion declination;
  //描画関連
  private Texture2D[] signboards;
  private Texture2D[] images;
  private Texture2D[] numbers;
  private Rect sunrect;
  private float horizontalFOV;
  private float verticalFOV;
  private int calibrated_orientation;
  private int signboardnumber;
  //デバイスの回転
  Quaternion camrotation;
  //日食のデータ
  private EclipseData currenteclipsedata;
  //方位と高度の数値
  private double sun_direction;
  private double sun_altitude;
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
  private bool calibrated;
  private bool magnetism;
  private double[] posdata;
  //状態変数
  private int state;
  //誘導イメージ点滅
  private bool blink;
  private float blinkinterval;

  void Start ()
   {
    ready = false;
    menuvisible = false;
    //メソッドポインタ初期化
    actions = new userAction[] { moveToMapMode, moveTo3DMode, changeEclipseData, setCurrentLocation, setMapPoint, moveToCalibration , showHelpMessage};

    //時計
    clock = gameObject.GetComponent<Clock>();
    //選択クラス
    chooser = gameObject.GetComponent<EclipseDataChooser>();
    //地点の選択
    pointchooser = gameObject.GetComponent<ARPointChooser>();
    //画像の読み込み
    images = new Texture2D[imagepaths.Length];
    foreach(string path in imagepaths){ StartCoroutine("getTextureAsset", path); }
    //数字の読み込み
    numbers = new Texture2D[10];
    for (int i = 0; i < 10; i++)
     {
      StartCoroutine("getTextureAsset", "numero/" + i.ToString()+ ".png" );
     }
    //誘導サインを読み込む
    signboards = new Texture2D[signboardpaths.Length];
    for (int i = 0; i < signboardpaths.Length; i++)
     {
      StartCoroutine("getTextureAsset", signboardpaths[i]);
     }

    //点滅
    blink = true;

    //表示位置格納用Rect
    sunrect = new Rect();
    //カメラ
    maincam = Camera.main;
    //データから読みだした天体の位置格納用配列
    posdata = new double[7];

    //デバイスの回転の保存用
    camrotation = Quaternion.identity;

    //カメラ画像を表示するクラス
    GameObject quad = GameObject.Find("Quad");
    camview = quad.GetComponent<CameraView>();
    //アプリケーションの保存してあるデータ
    appdata = ApplicationData.getApplicationData(); horizontalFOV = appdata.HorizontalAngle;


    //画角の読み込み
    //キャリブレーションが実行された後
    if (Calibration.calibrated)
     {
      horizontalFOV = Calibration.horizontal;
      verticalFOV = Calibration.vertical;
      calibrated_orientation = (int)Calibration.deviceOrientation;
      appdata.HorizontalAngle = horizontalFOV;
      appdata.VerticalAngle = verticalFOV;
      appdata.Orientation = calibrated_orientation;
      appdata.saveApplicationData();
      calibrated = true;
     }
    else
     {
      horizontalFOV = appdata.HorizontalAngle;
      verticalFOV = appdata.VerticalAngle;
      calibrated_orientation = appdata.Orientation;

      if (horizontalFOV == 0.0f | verticalFOV == 0.0f) calibrated = false;
      else calibrated = true;
     }

    //観測地点読み込み
    locationmode = MAP_LOCATION;
    longitude = appdata.ARLongitude;
    latitude = appdata.ARLatitude;

    Input.gyro.enabled = true;
    Input.location.Start();
    Input.compass.enabled = true;

    initFromEclipseDataHolder();
    //initEclipseData("20300601.json");
    //StartCoroutine("initEclipseDataFromAssetBundle", "20300601.json");

  //初回のヘルプ表示は終わっているか
    if (!appdata.hasARHelpShown)
     {
      showHelpMessage();
      appdata.hasARHelpShown= true;
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
      ready = true;
     }
   }

 //日食データを初期化する
  private void initEclipseData(string datafilename)
   {
    //Resourceから読み込む
    TextAsset jsondata = (TextAsset)Resources.Load("20300601", typeof(TextAsset));

   //StreamReader reader = new StreamReader(path);
    string jsonstring = jsondata.text;//reader.ReadToEnd();
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
        ready = true;
       }
     }
    catch (Exception e) {}
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
        ready = true;
       }
      //reader.Close();
     }
    catch (Exception e) { /*Debug.Log("readJson " + e.ToString());*/ }
   }

  private void showExplanationDialog()
   {
#if UNITY_ANDROID

    // Javaのオブジェクトを作成
    AndroidJavaClass nativeDialog = new AndroidJavaClass("studio.beautifulworld.dialoglibrary.MenuDialog");

    // Context(Activity)オブジェクトを取得する
    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

    // AndroidのUIスレッドで動かす
    context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
     // ダイアログ表示のstaticメソッドを呼び出す
     nativeDialog.CallStatic(
     "showMessages",
      context,
     "お知らせ",
     "カメラ画像に合わせて太陽の方向を表示します。");
     }));
#endif
   }

  void Update ()
   {
    //画角値の確認
    if (!calibrated)
     {
      if (Calibration.calibrated)
       {
        horizontalFOV = Calibration.horizontal;
        verticalFOV = Calibration.vertical;
        calibrated_orientation = (int)Calibration.deviceOrientation;
        appdata.HorizontalAngle = horizontalFOV;
        appdata.VerticalAngle = verticalFOV;
        appdata.saveApplicationData();
        calibrated = true;
       }
      else
       {
#if UNITY_ANDROID

        // Javaのオブジェクトを作成
        AndroidJavaClass nativeDialog = new AndroidJavaClass("studio.beautifulworld.dialoglibrary.MenuDialog");

        // Context(Activity)オブジェクトを取得する
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // AndroidのUIスレッドで動かす
        context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
        // ダイアログ表示のstaticメソッドを呼び出す
         nativeDialog.CallStatic(
         "showMessages",
         context,
         "お知らせ",
         "画角の値が記録されていません。\nキャリブレーションを実行します。");
         }));
#endif
        moveToCalibration();
       }
     }

    //データ選択・又は地点選択の場合、以後の処理はしない
    if (chooser.enabled | pointchooser.enabled | helpvisible) return;

    //処理開始
    interval += Time.deltaTime;
    if (interval > 0.1f & ready)
     {
      currenteclipsedata.getPositions(current, posdata);
      clock.setTime(current);

      if (!Input.location.isEnabledByUser)
       {
        state = LOCATION_UNAVAILABLE;
       }
      else
       {
        LocationInfo locinfo = Input.location.lastData;
        magnetism = false;
        if (locinfo.longitude > 122.5f & locinfo.longitude < 150.0f & locinfo.latitude > 23.0f & locinfo.latitude < 45.5f)
         {
          magnetism = true;
          //磁気偏角計算
          float declination_angle = TerrestrialMagnetism.getMagneticDeclination(locinfo.longitude, locinfo.latitude);
          declination = Quaternion.Euler(0.0f, declination_angle, 0.0f);
         }

        float height = 0;//高度
        if (locationmode == CURRENT_LOCATION)
         {
          longitude = locinfo.longitude;
          latitude = locinfo.latitude;
          height = locinfo.altitude;
         }
        float[,] matrix = caliculation.getMatrix(longitude, latitude, height);
        float[,] inverse = caliculation.getInverseMatrix(matrix);

   //debug
   /*
       //分、秒を含めて時間で表す
       double hour = current.Hour +9+ current.Minute / 60.0 + current.Second / 3600.0;
       double T = StarPosition.getTime(current.Year, current.Month, current.Day, hour);
      //太陽の黄経を計算するelon:ecliptic longitude
       double elon = StarPosition.getSunEclipticLongitude(T);
       //黄道傾角を計算する
       double e = StarPosition.getInclination(T);//
       //太陽の赤経に変換
       double asc = StarPosition.getRightAscension(elon, e);//
       //太陽の赤緯に変換
       double dec = StarPosition.getDeclination(elon, e);//   //恒星時を計算する
       double phai0 = StarPosition.getSidereal(T, hour / 24.0, longitude);//経過時間を引く必要があるので時間を別に与える
 */
       //食分計算
       //VesselElements ve = new VesselElements(posdata[EclipseData.SUN_ASC], posdata[EclipseData.SUN_DEC], posdata[EclipseData.SUN_DIST],
       //posdata[EclipseData.MOON_ASC], posdata[EclipseData.MOON_DEC], posdata[EclipseData.MOON_DIST], current);

        double phai = posdata[EclipseData.PHAI] + longitude;// longitude;//地方恒星時＝グリニッジ恒星時+経度。恒星時は反時計回り、経度は時計回り

        //太陽の方位・高度を計算する
        sun_direction = StarPosition.getSunDirection(posdata[EclipseData.SUN_ASC], posdata[EclipseData.SUN_DEC], latitude, phai);
        sun_altitude = StarPosition.getSunAltitude(posdata[EclipseData.SUN_ASC], posdata[EclipseData.SUN_DEC], latitude, phai);

        //コンパス空間に写像する
        //太陽の方向ベクトルを求める
        double sunz = Math.Sin(sun_altitude / RadToDeg);
        double suny = Math.Cos(sun_altitude / RadToDeg);
        double sunx = suny * Math.Cos((90.0 - sun_direction)/ RadToDeg);
        suny = suny * Math.Sin((90.0 - sun_direction)/ RadToDeg);

        double[] sun_vector = new double[] { sunx, suny, sunz };//角度から算出したベクトルなので、現在位置は引く必要はない

        //太陽への方向ベクトル単位化
        double norm = Math.Sqrt(sun_vector[0] * sun_vector[0] + sun_vector[1] * sun_vector[1] + sun_vector[2] * sun_vector[2]);
        sun_vector[0] /= norm;
        sun_vector[1] /= norm;
        sun_vector[2] /= norm;

        //太陽の方向が地平線の下
        if (sun_vector[2] < 0.0f) 
         {
          state = UNDER_HORIZON;
         }
        else
         {
          //デバイスの向きを検出する
          Quaternion gyro = Input.gyro.attitude;
          camrotation.Set(-gyro.x, -gyro.y, gyro.z, gyro.w);

          camrotation = Quaternion.Euler(90.0f, 0.0f, 0.0f) * camrotation;
          //向き検出・終わり

          //デバイス座標系に変換
          Quaternion inverseQ = Quaternion.Inverse(camrotation);
          Vector3[] sunandmoon = new Vector3[2];

          sunandmoon[0] = inverseQ * new Vector3((float)sun_vector[0], (float)sun_vector[2], (float)sun_vector[1]);
          //sunandmoon[1] = inverseQ * new Vector3((float)moon_vector[0], (float)moon_vector[2], (float)moon_vector[1]);

          //最後に有効になったデバイス向きを取得する
          DeviceOrientation orientation = camview.getLastOrienation();

          //画角とタンジェント値を得る
          float[] viewangles = new float[2];
          getViewAngle(orientation, viewangles);
          float verticalFOV = viewangles[1] / RadToDeg;
          float horizontalFOV = viewangles[0] / RadToDeg;
          float screen_tangent_horizontal = Mathf.Tan(horizontalFOV);
          float screen_tangent_vertical = Mathf.Tan(verticalFOV);
          float ratio = 0.5f / (screen_tangent_vertical * 2);

          for (int i = 0; i < 1; i++)
           {
            //磁気補正
            if (magnetism) sunandmoon[i] = declination * sunandmoon[i];

            //後ろの場合も見えないので、手順をスキップする。
            float x_angle = Mathf.Atan(sunandmoon[i].x / sunandmoon[i].z);
            float y_angle = Mathf.Atan(sunandmoon[i].y / sunandmoon[i].z);

            if (Mathf.Abs(x_angle) < horizontalFOV & Mathf.Abs(y_angle) < verticalFOV  & sunandmoon[i].z > 0)
             {
              float screenX = Mathf.Tan(x_angle) / screen_tangent_horizontal * Screen.width / 2 + Screen.width / 2 - images[SUNIMAGE].width / 2;
              float screenY = Screen.height / 2 - Mathf.Tan(y_angle) / screen_tangent_vertical * Screen.height / 2 - images[SUNIMAGE].height / 2;
              //datas.targetposition.Set(x, y, targetbox.width, targetbox.height);

              switch (i)
               {
                case 0: state = INSIDE_SCREEN; sunrect.Set(screenX, screenY, images[SUNIMAGE].width * ratio, images[SUNIMAGE].height * ratio); break;
                //case 1: moonSeeable = true; moonrect.Set(screenX, screenY, moonbox.width * ratio, moonbox.height * ratio); break;
               }

              blink = true;
              blinkinterval = 0.0f;
             }
            else
             {
              state = OUTSIDE_SCREEN;
              //太陽の位置を示すアイコンを決める
              getGuideImage(sunandmoon[0]);
             }
            }//for終わり
         }
        //デバイス座標系に変換・終わり
       }


      //時間を一つ進める
      current = current.AddMinutes(1.0);
      //終了時刻よりあとの時刻か
      if (current.CompareTo(finish) > 0)
       {
        current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, DateTimeKind.Utc);
       }
       //
      interval = 0.0f;
     }

    //アプリケーションの終了
    if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        Input.gyro.enabled = false;
        Input.location.Stop();
        Input.compass.enabled = false;
        // アプリケーション終了処理
        Application.Quit();
        return;
       }
     }
   }

 //
  void OnGUI()
   {
    if (!chooser.getEnabled() & !pointchooser.enabled)
     {
      switch(state)
       {
        case UNDER_HORIZON:
         GUI.DrawTexture(new Rect((Screen.width - images[ALERT_UNDERHORIZON].width)/2, (Screen.height - images[ALERT_UNDERHORIZON].height) / 2,
          images[ALERT_UNDERHORIZON].width, images[ALERT_UNDERHORIZON].height), images[ALERT_UNDERHORIZON]);
         break;
        case INSIDE_SCREEN:
         GUI.DrawTexture(sunrect, images[SUNIMAGE]);
         //太陽の位置を表示する
         float left = 0.0f;
         float top = 0.0f;

         if (sunrect.yMin < (Screen.height / 2))
          {
           left = (Screen.width - images[SUNPOSITION].width) / 2;
           top = (Screen.height / 2) + ((Screen.height / 2) - images[SUNPOSITION].height) / 2;

           GUI.DrawTexture(new Rect(left, top, images[SUNPOSITION].width, images[SUNPOSITION].height), images[SUNPOSITION]);
          }
         else if(sunrect.yMin >= (Screen.height / 2))
          {
           left = (Screen.width - images[SUNPOSITION].width) / 2;
           top = ((Screen.height / 2) - images[SUNPOSITION].height) / 2;

           GUI.DrawTexture(new Rect(left, top, images[SUNPOSITION].width, images[SUNPOSITION].height), images[SUNPOSITION]);
          }
         drawNumbers(left, top, direction_baseline, (float)sun_direction);
         drawNumbers(left, top, altitude_baseline,(float)sun_altitude );
         drawNumbers(left, top, longitude_baseline, longitude);
         drawNumbers(left, top, latitude_baseline, latitude);
         break;
        case OUTSIDE_SCREEN:
         blinkinterval += Time.deltaTime;
         if (blink)
          {
           GUI.DrawTexture(new Rect((Screen.width - signboards[signboardnumber].width) / 2, (Screen.height - signboards[signboardnumber].height) / 2,
            signboards[signboardnumber].width, signboards[signboardnumber].height), signboards[signboardnumber]);
           if (blinkinterval > 1.0f) { blink = false; blinkinterval = 0.0f; }
          }
         else
          {
           if (blinkinterval > 0.4f) { blink = true; blinkinterval = 0.0f; }
          }
         break;
        case LOCATION_UNAVAILABLE:
         GUI.DrawTexture(new Rect((Screen.width - images[ALERT_LOCATION].width) / 2, (Screen.height - images[ALERT_LOCATION].height) / 2,
             images[ALERT_LOCATION].width, images[ALERT_LOCATION].height), images[ALERT_LOCATION]);
         break;
       }

      //メニューボタン表示
      if (GUI.Button(new Rect((Screen.width - images[MENUBUTTON].width) / 2, (Screen.height - images[MENUBUTTON].height - footmargin), images[MENUBUTTON].width, images[MENUBUTTON].height), images[MENUBUTTON]))
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

  //数値を書く
  private void drawNumbers(float left, float top, float baseline, float number)
   {
    float value = Mathf.Abs(number);
    float denominator = 100.0f;
    int head = -1;
    for (int i = 1; i < 6; i++)
     {
      int quotient = (int)Mathf.Abs(value / denominator);
      if (quotient < 10)
       {
        if ((head == -1 & quotient != 0) | (head == -1 & i == 3))
         {
          GUI.DrawTexture(new Rect(left + orders[i], top + baseline, numbers[quotient].width, numbers[quotient].height), numbers[quotient]);
          head = i;
         }
        else if (head != -1)
         {
          GUI.DrawTexture(new Rect(left + orders[i], top + baseline, numbers[quotient].width, numbers[quotient].height), numbers[quotient]);
         }
       }
      value -= (quotient * denominator);
      denominator /= 10.0f;
     }
    if (number < 0.0)
     {
      int minuspos = head - 1;
      GUI.DrawTexture(new Rect(left + orders[minuspos], top + baseline, images[MINUS].width, images[MINUS].height), images[MINUS]);
     }
   }

  //画面外の太陽の方向を示す画像を得る
  private void getGuideImage(Vector3 sundirect)
   {
    Vector3 axis = Vector3.Cross(Vector3.forward, sundirect);
    Vector3 tangent = Vector3.Cross(axis, Vector3.forward);

    tangent.z = 0.0f;
    if (tangent.x == 0.0f)
     {
      if (tangent.y > 0.0f) { }//上
      else if (tangent.y < 0.0f) { } //下
     }
    else
     {
      float angle= Mathf.Atan(tangent.y / tangent.x) / Mathf.PI * 180.0f;
 
      if (tangent.x > 0.0f)
       {
        if (-90 <= angle & angle < -67.5) { signboardnumber = 4; } //下
        else if (-67.5 <= angle & angle < -22.5) { signboardnumber = 5; }//右下
        else if (-22.5 <= angle & angle < 22.5){ signboardnumber = 6; }
        else if (22.5 <= angle & angle < 67.5) { signboardnumber = 7; }
        else if (67.5 < angle ) { signboardnumber = 0; }
       }
      else if (tangent.x < 0.0f)
       {
        if (-90 <= angle & angle < -67.5) { signboardnumber = 0; } //上
        else if (-67.5 <= angle & angle < -22.5) { signboardnumber = 1; }//左上
        else if (-22.5 <= angle & angle < 22.5) { signboardnumber = 2; }
        else if (22.5 <= angle & angle < 67.5) { signboardnumber = 3; }
        else if (67.5 < angle) { signboardnumber = 4; }
       }
     }
   }


  //画角を得る
  private void getViewAngle(DeviceOrientation orientation, float[] container)
   {
    DeviceOrientation direction = (DeviceOrientation)calibrated_orientation;

    if (orientation == DeviceOrientation.Portrait | orientation == DeviceOrientation.PortraitUpsideDown)
     {
   // if (Calibration.deviceOrientation == DeviceOrientation.Portrait | Calibration.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
      if (direction == DeviceOrientation.Portrait | direction == DeviceOrientation.PortraitUpsideDown)
       {
        container[0] = horizontalFOV;
        container[1] = verticalFOV;
       }
      else
       {
        container[0] = verticalFOV;
        container[1] = horizontalFOV;
       }
     }
    else if (orientation == DeviceOrientation.LandscapeLeft | orientation == DeviceOrientation.LandscapeRight)
     {
   //      if (Calibration.deviceOrientation == DeviceOrientation.Portrait | Calibration.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
      if (direction == DeviceOrientation.Portrait | direction == DeviceOrientation.PortraitUpsideDown)
       {
        container[0] = verticalFOV;
        container[1] = horizontalFOV;
       }
      else
       {
        container[0] = horizontalFOV;
        container[1] = verticalFOV;
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

    //Debug.Log("data number = " + number);
    string datafilename = EclipseCalendar.getDateString(year, month, day) + ".json";

    StartCoroutine("initEclipseDataFromAssetBundle", datafilename);
   }


  //観察地点の変更を受け取る
  public void ObservationPointChange(float lon, float lat)
   {
    GameObject quad = GameObject.Find("Quad");
    CameraView camview = quad.GetComponent<CameraView>();
    camview.resumeCamera();
    clock.enabled = true;

    longitude = lon;
    latitude = lat;

    appdata.ARLongitude = lon;
    appdata.ARlatitude = lat;
    appdata.saveApplicationData();
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
    for (int i = 0; i < images.Length; i++)
     {
      if (datafilename == imagepaths[i]) { images[i] = tex; return; }
     }

    for (int i = 0; i < signboardpaths.Length; i++)
     {
      if (datafilename == signboardpaths[i]) { signboards[i] = tex;  return; }
     }

    for (int i = 0; i < 10; i++)
     {
      if (datafilename == "numero/" + i.ToString() + ".png") { numbers[i] = tex; return; }
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
    if (identifier == "CANCEL") { clock.enabled = true; return; }
  
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
  private void moveToMapMode()
   {
    Input.gyro.enabled = false;
    Input.location.Stop();
    Input.compass.enabled = false;

    SceneManager.LoadScene("eclipsemap");
   }

 //マップモードヘ
  private void moveTo3DMode()
   {
    Input.gyro.enabled = false;
    Input.location.Stop();
    Input.compass.enabled = false;

    SceneManager.LoadScene("SolarEclipse");
   }

  //日食を選ぶ
  private void changeEclipseData()
   {
    chooser.enabled = true;

    clock.enabled = false;
    chooser.setEnabled(true, this);
   }

  //現在地点から見る
  private void setCurrentLocation()
   {
    locationmode = CURRENT_LOCATION;
    clock.enabled = true;
   }

  //地図上の点から見る
  private void setMapPoint()
   {
    GameObject quad = GameObject.Find("Quad");
    CameraView camview = quad.GetComponent<CameraView>();
    camview.pauseCamera();

    pointchooser.enabled = true;
    locationmode = MAP_LOCATION;
    pointchooser.setPointReceiver(this, appdata.ARLongitude, appdata.ARLatitude);
    clock.enabled = false;
   }

  //キャリブレーション
  private void moveToCalibration()
   {
    Input.gyro.enabled = false;
    Input.location.Stop();
    Input.compass.enabled = false;

    Calibration.movetoscene = "EclipseAR";
    SceneManager.LoadScene("calibration");
   }

 //ヘルプの表示
  private void showHelpMessage()
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
     "方位モードについて",//タイトル
     aboutARmode,
     msgID);
  }));
#endif
   }
 } 
