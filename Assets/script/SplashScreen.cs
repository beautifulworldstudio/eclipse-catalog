using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SplashScreen : MonoBehaviour
 {
  private const string iconfilename = "appicon.png";
  private const string titlefilename = "apptitle.png";
  private const string datafileprefix = "jsons/";
  private const string msgID = "SplashScreen_NOTICE";
  private const string aboutthisapp = "　このアプリは2030年までの日食の起こる位置、太陽の方位を表示するアプリです。\n　太陽の位置を表示するために、カメラから取得した画像を使用します。画像の保存、並びに送信は行いません。又、方位と高度を計算するために現在位置をデバイスから取得します。位置情報の保存、並びに送信は行いません。\n　太陽と月の天球上の位置は略算式を使用して計算しており、実際の位置とはずれが生じる可能性があります。日食の観測の前には、天文台等が発表する予測を確認してください。";

  private Texture2D iconimage;
  private Texture2D titleimage;
  private Rect iconrect;
  private Rect titlerect;
  private float interval;
  //フラグ
  private bool ready;
  private bool noticevisible;

  // Use this for initialization
  void Start ()
   {
    StartCoroutine("getTextureAsset", iconfilename);
    StartCoroutine("getTextureAsset", titlefilename);
    float shortside = 0;
    if (Screen.width < Screen.height) shortside = Screen.width;
    else shortside = Screen.height;

    if (shortside < 720)
     {
      iconrect = new Rect(0, 0, 500, 500);
      titlerect = new Rect(0, 0, 500, 500);
     }
    else if (shortside < 1080)
     {
      iconrect = new Rect(0, 0, 600, 600);
      titlerect = new Rect(0, 0, 600, 600);
     }

    //初期化
    int indexnumber = -1;
    DateTime current = DateTime.UtcNow;

    for (int i = 0; i < EclipseCalendar.schedule.Length; i++)
     {
      int year = EclipseCalendar.schedule[i][0];
      int month = EclipseCalendar.schedule[i][1];
      int day = EclipseCalendar.schedule[i][2];
      int hour = EclipseCalendar.schedule[i][3];
      int minute = EclipseCalendar.schedule[i][4];

      DateTime date = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
      if (date.CompareTo(current) < 0) continue;
 
      indexnumber = i;
      break;
     }
    if (indexnumber == -1) indexnumber = EclipseCalendar.schedule.Length - 1;

    string filename = EclipseCalendar.schedule[indexnumber][0].ToString();
    if (EclipseCalendar.schedule[indexnumber][1] < 10) filename += "0";
    filename += EclipseCalendar.schedule[indexnumber][1].ToString();
    if (EclipseCalendar.schedule[indexnumber][2] < 10) filename += "0";
    filename += EclipseCalendar.schedule[indexnumber][2].ToString();
    filename += ".json";

    //jsonファイルを読み込む
    StartCoroutine("initEclipseDataFromAssetBundle", filename);
   }

  // Update is called once per frame
  void Update ()
   {
    if (Input.deviceOrientation == DeviceOrientation.Portrait | Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
     {
      float horizontalmargin = (Screen.width - iconrect.width) / 2;
      float verticalmargin = (Screen.height - iconrect.height - titlerect.height) / 2;
 
      iconrect.Set(horizontalmargin, verticalmargin, iconrect.width, iconrect.height);
      titlerect.Set(horizontalmargin, verticalmargin + iconrect.height, titlerect.width, titlerect.height);
     }
    else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft | Input.deviceOrientation == DeviceOrientation.LandscapeRight)
     {
      float horizontalmargin = (Screen.width - iconrect.width - titlerect.width) / 2;
      float verticalmargin = (Screen.height - iconrect.height) / 2;

      iconrect.Set(horizontalmargin, verticalmargin, iconrect.width, iconrect.height);
      titlerect.Set(horizontalmargin + iconrect.width, verticalmargin, titlerect.width, titlerect.height);
     }
    else 
     {
      float horizontalmargin = (Screen.width - iconrect.width) / 2;
      float verticalmargin = (Screen.height - iconrect.height - titlerect.height) / 2;

      iconrect.Set(horizontalmargin, verticalmargin, iconrect.width, iconrect.height);
      titlerect.Set(horizontalmargin, verticalmargin + iconrect.height, titlerect.width, titlerect.height);
     }

    interval += Time.deltaTime;
    if (interval > 4.0f & ready & !noticevisible)
     {
      showNotice();
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
    if(!noticevisible)
     {
      if (iconimage != null) GUI.DrawTexture(iconrect, iconimage);
      if (titleimage != null) GUI.DrawTexture(titlerect, titleimage);
     }
   }


  private void setTexture(string datafilename, Texture2D tex)
   {
    switch(datafilename)
     {
      case iconfilename: iconimage = tex; break;
      case titlefilename: titleimage = tex; break;
     }
   }


  private void setTextAsset(string text , string datafilename)
   {
    if (datafilename.IndexOf(".json") != -1)
     {
      try
       {
        EclipseData data = JsonUtility.FromJson<EclipseData>(text);
        if (data != null && data.initDateTime())
         {
          EclipseDataHolder.setEclipseData(data);
          ready = true;
         }
       //reader.Close();
      }
     catch (Exception e) { /*Debug.Log("readJson " + e.ToString());*/ }
    }
  } 


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
          setTexture(datafilename, texture);
         }
        else
         {
          //container.iconimage = unknownicon;
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
      setTexture(datafilename, www.texture);
   // text = "count =" + www.text.Length.ToString();
     }
   }

  //日食データをアセットバンドルから初期化する
  IEnumerator initEclipseDataFromAssetBundle(string datafilename)
   {
    string jsonstring = null;

    if (Application.platform == RuntimePlatform.WindowsEditor | Application.platform == RuntimePlatform.WindowsPlayer)
     {
      string path = Application.dataPath + "/StreamingAssets/" + datafileprefix + datafilename;
      StreamReader reader = new StreamReader(path);

      jsonstring = reader.ReadToEnd();
      reader.Close();
      setTextAsset(jsonstring, datafilename);
     }
    else if (Application.platform == RuntimePlatform.Android)
     {
      string bundleUrl = Path.Combine("jar:file://" + Application.dataPath + "!/assets" + "/", datafileprefix + datafilename);
      WWW www = new WWW(bundleUrl);
   //   if (www == null) text = "WWW NULL";
      yield return www;

      while (!www.isDone) { }
      jsonstring = www.text;
      setTextAsset(jsonstring, datafilename);
   // text = "count =" + www.text.Length.ToString();
     }
   }

  //Androidのメッセージダイアログを閉じるイベントを受け取る
  public void messageDialogClosed(string identifier)
   {
    if (identifier== msgID)
     {
      SceneManager.LoadScene("SolarEclipse");
     }
   }

  //メッセージ表示
  private void showNotice()
   {
    if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
     {
      SceneManager.LoadScene("SolarEclipse");
     }
    if (Application.platform == RuntimePlatform.Android)
     {
#if UNITY_ANDROID
    noticevisible = true;

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
     "プライバシーポリシーと注意",//タイトル
     aboutthisapp,
     msgID);
  }));
#endif
     }
   }
 }
