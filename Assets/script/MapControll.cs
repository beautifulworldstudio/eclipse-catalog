using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MapControll : MonoBehaviour
 {
  private const float RadToDeg = 180 / Mathf.PI;
  private const float screendistance = 10.0f;
  private const float TWOPI = Mathf.PI * 2;
  private const float HALFPI = Mathf.PI / 2;
  private const float leastAngle = 7.0f;

  private GUIStyle labelStyle;
  private GameObject mapboard;
  private Camera maincam;
  private DeviceOrientation lastorientation;
  //最大画角
  private float largestFOV;
  //カメラの最大可動範囲
  private float camposleft;
  private float campostop;
  private float camposright;
  private float camposbottom;
 //マップを表示するplaneオブジェクトの頂点位置
  private float mapleft;
  private float maptop;
  private float mapright;
  private float mapbottom;
  private float mapwidth;
  private float mapheight;
  //現在の地図上の表示範囲
  private float screenleft;
  private float screentop;
  private float screenright;
  private float screenbottom;
  //タッチ関係変数
  private float lastX;
  private float lastY;
  private float lastRadius;
  private int lastTouchCount;
  //フラグ
  private bool swipe;
  private bool pinch;
  private bool suspend;
  //表示テクスチャ
  //private Texture2D logo;

  private string text1;
  //計算感覚
  private float interval;

  // Use this for initialization
 	void Start ()
   {
  //
  //フォント生成
  labelStyle = new GUIStyle();
  labelStyle.fontSize = Screen.height / 22;
  labelStyle.normal.textColor = Color.white;
  //

    //地図を表示するPlaneを取得
    mapboard = GameObject.Find("MapBoard");
    //カメラ
    maincam = Camera.main;

    mapwidth = mapboard.GetComponent<Renderer>().bounds.size.x;
    mapheight = mapboard.GetComponent<Renderer>().bounds.size.y;
    float maphalfwidth = mapwidth / 2.0f;
    float maphalfheight = mapheight / 2.0f;
    //地図の頂点位置
    mapleft = -maphalfwidth;
    maptop = maphalfheight;
    mapright = maphalfwidth;
    mapbottom = -maphalfheight;

    //最大画角を決める。画面高さが地図の縦より大きくなければいいから、最大画角は縦の長さから算出される。
    //スクリーンまでの直線と直角三角形を作るため、縦の半分の長さから計算する
　  largestFOV = Mathf.Atan(maphalfheight / screendistance) * RadToDeg * 2.0f;

/*
    //テクスチャ読み込み
    logo = (Texture2D)Resources.Load("logo_black", typeof(Texture2D));
*/
    //カメラ可動範囲の初期化
    getCameraMovableArea();

    interval = 0.0f;
    swipe = false;
    pinch = false;
    suspend = false;
   }
	
	// Update is called once per frame
 	void Update ()
   {
　　//最大画角を超えていないかチェックする
    if (maincam.fieldOfView > largestFOV) maincam.fieldOfView = largestFOV;

    //画面の表示位置確認
    float currentVerticalFOV = maincam.fieldOfView / 2.0f / RadToDeg;

    //画面の端が外に出るようであれば、位置を変える
    float ylength = Mathf.Tan(currentVerticalFOV) * screendistance;
    float xlength = ylength * Screen.width / Screen.height;
    screenleft = maincam.transform.position.x - xlength;
    screentop = maincam.transform.position.y + ylength;
    screenright = maincam.transform.position.x + xlength;
    screenbottom = maincam.transform.position.y - ylength;

    if (screenleft < mapleft)
     {
      float difference = mapleft - screenleft;
      screenleft = mapleft;  
      screenright += difference; 
      Vector3 campos = maincam.transform.position;
      campos.x += difference;
      maincam.transform.position = campos;
     }
    if (screenright > mapright)
     {
      float difference = mapright - screenright;
      screenright = mapright;
      screenleft += difference;
      Vector3 campos = maincam.transform.position;
      campos.x += difference;
      maincam.transform.position = campos;
     }
    if (screentop > maptop)
     {
      float difference = maptop - screentop;
      screentop = maptop;
      screenbottom += difference;
      Vector3 campos = maincam.transform.position;
      campos.y += difference;
      maincam.transform.position = campos;
     }
    if (screenbottom < mapbottom)
     {
      float difference = mapbottom - screenbottom;
      screenbottom = mapbottom;
      screentop += difference;
      Vector3 campos = maincam.transform.position;
      campos.y += difference;
      maincam.transform.position = campos;
     }

    //操作が一時停止なら、以後の処理はしない
    if (suspend) return;

    //タッチによるスワイプを取得する
    //画面の縮小拡大を取得する
    if (Input.touchCount == 0)
     {
      if (swipe) swipe = false;
      if (pinch) pinch = false;
     }
    else if (Input.touchCount == 1)
     {
      if (pinch) pinch = false;

      Touch[] touches = Input.touches;
      Vector2 fingerpos = touches[0].position;
      if (!swipe)
       {
        swipe = true;
       }
      else
       {
        float Xdifference = fingerpos.x - lastX;
        float Ydifference = fingerpos.y - lastY;
        Vector3 campos = maincam.transform.position;
        float ratio = 0.01f * maincam.fieldOfView / largestFOV;
       
        float Xnewposition = campos.x - (Xdifference * ratio);
        float Ynewposition = campos.y - (Ydifference * ratio);

        if (Xnewposition >= camposleft & Xnewposition <= camposright) campos.x = Xnewposition;
        if (Ynewposition <= campostop & Ynewposition >= camposbottom) campos.y = Ynewposition;

        maincam.transform.position = campos;
       }
      lastX = fingerpos.x;
      lastY = fingerpos.y;
     }
    else if (Input.touchCount > 1)
     {
      if (swipe) { swipe = false; }
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
        float angleaddition = (lastRadius - largestRadius) * 0.05f;
        maincam.fieldOfView = maincam.fieldOfView + angleaddition;
        //画角の最小角度はここでコントロールする
        if (maincam.fieldOfView < leastAngle) { maincam.fieldOfView = leastAngle; }

        //カメラの可動範囲を計算する
        getCameraMovableArea();
       }
      lastTouchCount = Input.touchCount;
      lastRadius = largestRadius;
     }

/*
    //終了処理
    if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        // アプリケーション終了
        Application.Quit();
        return;
       }
     }
*/
   }

  private void getCameraMovableArea()
   {
    //現在の画角（vertical）の2分の1
    float currentVerticalFOV = maincam.fieldOfView / 2.0f / RadToDeg;

    //現時点の画角での表示幅の2分の1
    float screenheight = Mathf.Tan(currentVerticalFOV) * screendistance;
    float screenwidth = screenheight * Screen.width / Screen.height;

  //地図オブジェクトのそれぞれの辺の2分の1
　  float maphalfwidth = mapboard.GetComponent<Renderer>().bounds.size.x / 2.0f;
    float maphalfheight = mapboard.GetComponent<Renderer>().bounds.size.y / 2.0f;

    float Xmargin = maphalfwidth - screenwidth;
    float Ymargin = maphalfheight - screenheight;

    camposleft = -Xmargin;
    campostop =Ymargin;
    camposright = Xmargin;
    camposbottom = -Ymargin;

    text1 = camposleft + "\n" + campostop + "\n" + camposright + "\n" + camposbottom;
   }

/*
  private void OnGUI()
   {
//    GUI.Label(new Rect(50, 20, 400, 300), text1, labelStyle);
    //ロゴを地図上に描画する
    if(!suspend) GUI.DrawTexture(new Rect(Screen.width - logo.width, Screen.height - logo.height, logo.width, logo.height), logo);
   }
*/

  public void setOperationEnabled(bool val)
   {
    //Debug.Log("passed setOperationEnabled " + val);
    suspend = !val;
   }

  public bool getOperationEnabled()
   {
    return suspend;
   }

 private void getPositionOnMap()
   {
/*
    float x = (float)satpos.getX();
    float y = (float)satpos.getY();
    float z = (float)satpos.getZ();
    float x2 = x * x;
    float y2 = y * y;

    float longitude = Mathf.Acos(x / Mathf.Sqrt(x2 + y2));
    if (y < 0.0f) longitude = TWOPI - longitude;
    float latitude = Mathf.Asin(z / Mathf.Sqrt(x2 + y2 + z * z));

    container.x = longitude / TWOPI * mapwidth + mapleft;
    container.y = latitude / HALFPI * (mapheight / 2);
*/
   }
 }
