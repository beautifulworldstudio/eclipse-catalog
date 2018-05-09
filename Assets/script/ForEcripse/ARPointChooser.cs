using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ARPointChooser : MonoBehaviour
 {
  private const string mapimagefile = "worldmap.jpg";
  private const string decideimagefile = "decidebutton.png";
  private const string leftupfile = "leftupwardarrow.png";
  private const string leftdownfile = "leftdownwardarrow.png";
  private const string rightupfile = "rightupwardarrow.png";
  private const string rightdownfile = "rightdownwardarrow.png";

  private GUIStyle labelStyle;
  private string text = "";
  private PointChangeReceiver pointreceiver;
  //指定地点 
  private float longitude;
  private float latitude;
  //地図表示関連
  private float offsetX;
  private float offsetY;
  private float width;
  private float height;
  private float minimumheight;
  private float maximumheight;
  //描画関連
  private Texture2D worldmap;
  private Texture2D currentarrow;
  private Texture2D decidebutton;
  private Texture2D leftuparrow;
  private Texture2D leftdownarrow;
  private Texture2D rightuparrow;
  private Texture2D rightdownarrow;
  private Rect mapposition;
  private Rect arrowposition;
  //マウス・タッチ操作
  private float lastTouchCount;
  private float lastRadius;
  private float lastx;
  private float lasty;
  //デバイスの向き
  private DeviceOrientation orientation;
  //フラグ
  private bool ready;
  private bool pinch;
  private bool pressed;
  private bool pointerdragged;

 
  void Start ()
   {
    ready = false;
    pinch = false;
    pointerdragged = false;

    labelStyle = new GUIStyle();
    labelStyle.fontSize = Screen.height / 22;
    labelStyle.normal.textColor = Color.black;

    float longside =  Screen.height > Screen.width ? Screen.height : Screen.width;
    if (longside <= 1280)
     {
      minimumheight = 1280;
     }
    else if (longside <= 1920)
     {
      minimumheight = 1920;
     }
    //
    mapposition = new Rect();
    text = "text";

    StartCoroutine("getTextureAsset", decideimagefile);
    StartCoroutine("getTextureAsset", mapimagefile);
    StartCoroutine("getTextureAsset", mapimagefile);
    StartCoroutine("getTextureAsset", leftupfile);
    StartCoroutine("getTextureAsset", leftdownfile);
    StartCoroutine("getTextureAsset", rightupfile);
    StartCoroutine("getTextureAsset", rightdownfile);
	  }
	

	 void Update ()
   {
    if(!ready)
     {
      if (worldmap != null & leftuparrow != null & leftdownarrow != null & rightuparrow != null & rightdownarrow != null)
       {
        initialize();
       }
     }
    else
     {
   //ピンチ操作
   //タッチによるスワイプを取得する
    //画面の縮小拡大を取得する
      if (Input.touchCount == 0)
       {
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
        if (pointerdragged) { pointerdragged = false; }
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
          //地図のサイズを変更する
          float ratio = largestRadius / lastRadius;
          magnifyMap(ratio, Xcenter, Ycenter);
          shiftMapPosition(0.0f, 0.0f);
         }
        lastTouchCount = Input.touchCount;
        lastRadius = largestRadius;
       }
   //ピンチ操作・終わり
   //マウス検知
      if (Input.GetMouseButtonDown(0))
       {
        Vector3 mousepos = Input.mousePosition;
        float x = mousepos.x;
        float y = Screen.height - mousepos.y; //画面は左上が原点、マウス（タッチ）は左下が原点

        if(!pointerdragged)
         {
          float left = arrowposition.xMin;
          float top = arrowposition.yMin;
          float right = left + arrowposition.width;
          float bottom = top + arrowposition.height;

          if (left <= x & x < right & top <= y & y < bottom) pointerdragged = true;
         }

        if (!pressed & !pointerdragged)
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
        if (pointerdragged) pointerdragged = false;
       }

      if (pressed)
       {
        float xdiff = Input.mousePosition.x - lastx;
        float ydiff = lasty - Input.mousePosition.y;//左下原点の値に換算する

        if (xdiff != 0.0f | ydiff != 0.0f) shiftMapPosition(xdiff, ydiff);

        lastx = Input.mousePosition.x;
        lasty = Input.mousePosition.y;
       }
      else if (pointerdragged)
       {
        float xdiff = Input.mousePosition.x - lastx;
        float ydiff = lasty - Input.mousePosition.y;
        shiftPointer(xdiff, ydiff);
        lastx = Input.mousePosition.x;
        lasty = Input.mousePosition.y;
       }
      //マウス操作・終わり
     }

    //offsetが適切でない場合、地図位置を補正する
    bool mapreposition = false;
    if ((Mathf.Abs(offsetX) + Screen.width) >= width)
     {
      offsetX = -(width - Screen.width);
      mapreposition = true;
     }
    if ((Mathf.Abs(offsetY) + Screen.height) >= height)
     {
      offsetY = -(height - Screen.height);
      mapreposition = true;
     }
    if (mapreposition) mapposition.Set(offsetX, offsetY, width, height);
  //デバイス補正おわり　

  //終了処理
  if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        // キャンセル処理
        enabled = false;
        return;
       }
     }
   }

  void OnGUI()
   {
    //地図表示
    GUI.DrawTexture(mapposition, worldmap);

    //ポインタ表示
    float currentx = width * longitude / 360.0f;
    if (currentx < 0) currentx += width;
    else if (currentx >= width) currentx -= width;

    float currenty = (height / 2.0f) * (1.0f - latitude / 90.0f);
    if (currenty < 0) currenty = 0;
    else if (currenty >= height) currenty = height;

    currentx += offsetX;
    currenty += offsetY;

    if (pointerdragged)
     {
      getPointerPosition(currentx, currenty);    
     }
    else { adjustPointer(currentx, currenty); }

 // text = "currentx=" + currentx + "\ncurrenty =" + currenty + "\nwidth= " + arrowposition.width + "\nheight =" + arrowposition.height;
    //ポインタ表示
    GUI.DrawTexture(arrowposition, currentarrow);

    //ボタン
    if (GUI.Button(new Rect((Screen.width - decidebutton.width)/ 2, Screen.height - decidebutton.height, decidebutton.width, decidebutton.height), decidebutton) )
     {
      pointreceiver.ObservationPointChange(longitude, latitude); 

      enabled = false;
     }
    //GUI.Label(new Rect(20, 20, 400, 400),text, labelStyle);
   }

  //矢印画像を変更せずに位置を得る
  private void getPointerPosition(float currentx, float currenty)
   {
    if (currentarrow == leftuparrow)
     {
      arrowposition.Set(currentx, currenty, leftuparrow.width, leftuparrow.height);
     }
    else if (currentarrow == leftdownarrow)
     {
      arrowposition.Set(currentx, currenty - rightuparrow.height, rightuparrow.width, rightuparrow.height);
     }
    else if (currentarrow == rightuparrow)
     {
      arrowposition.Set(currentx - rightuparrow.width, currenty, rightuparrow.width, rightuparrow.height);
     }
    else if(currentarrow == rightdownarrow)
     {
      arrowposition.Set(currentx - rightdownarrow.width, currenty - rightdownarrow.height, rightdownarrow.width, rightdownarrow.height);
     }
   }

  //表示ポインターを選ぶ
  private void adjustPointer(float currentx, float currenty)
   {
    float halfWidth = Screen.width / 2;
    float halfHeight = Screen.height / 2;

    if (0 <= currentx & halfWidth > currentx)
     {
      if (0 <= currenty & halfHeight > currenty)
       {
        arrowposition.Set(currentx, currenty, leftuparrow.width, leftuparrow.height);
        currentarrow = leftuparrow;
       }
      else if (halfHeight <= currenty & Screen.height > currenty)
       {
        arrowposition.Set(currentx, currenty - rightuparrow.height, rightuparrow.width, rightuparrow.height);
        currentarrow = leftdownarrow;
       }
      else
       {
        arrowposition.Set(0, 0, 0, 0);
       }
     }
    else if (halfWidth <= currentx & Screen.width > currentx)
     {
      if (0 < currenty & halfHeight > currenty)
       {
        arrowposition.Set(currentx - rightuparrow.width, currenty, rightuparrow.width, rightuparrow.height);
        currentarrow = rightuparrow;
       }
      else if (halfHeight <= currenty & Screen.height > currenty)
       {
        arrowposition.Set(currentx - rightdownarrow.width, currenty - rightdownarrow.height, rightdownarrow.width, rightdownarrow.height);
        currentarrow = rightdownarrow;
       }
      else
       {
        arrowposition.Set(0, 0, 0, 0);
       }
     }
    else
     {
      arrowposition.Set(0, 0, 0, 0);
     }
   }


  //ポインターを動かす
  private void shiftPointer(float xdiff, float ydiff)
   {
    //ポインタ表示
    float currentx = width * longitude / 360.0f;
    currentx += xdiff;
    if (currentx < 0) currentx += width;
    else if (currentx >= width) currentx -= width;

    float currenty = (height / 2.0f) * (1.0f - latitude / 90.0f);
    currenty += ydiff;
    if (currenty < 0) currenty = 0;
    else if (currenty >= height) currenty = height;


    longitude = currentx / width * 360.0f;
    if (longitude > 180.0) longitude -= 360.0f;
    float halfheight = height / 2.0f;
    //latitude = (halfheight - currenty) / halfheight * 90.0f;//この式を書き換えて下を得る

    latitude = (1.0f - (currenty / halfheight)) * 90.0f;
   }


  //ピンチ操作で地図を拡大縮小する
  private void magnifyMap(float ratio, float centerx, float centery)
   {
    float newheight = Mathf.Floor(height * ratio);
    //ピンチの中心点の地図上の比率を求める
    float allwidth = Mathf.Abs(offsetX) + centerx;
    float allheight = Mathf.Abs(offsetY) + centery;
    float ratiox = allwidth / width;
    float ratioy = allheight / height;

    if (newheight < minimumheight) newheight = minimumheight;
    else if (newheight > maximumheight) newheight = maximumheight;

    //サイズ変更後の値を代入
    width = newheight * 2.0f;
    height = newheight;

    //新たなオフセット値を計算
    allwidth = width * ratiox;
    allheight = height * ratioy;

    offsetX = centerx - allwidth;
    offsetY = centery - allheight;

    mapposition.Set(offsetX, offsetY, width, height);
   }


  //地図の表示位置を動かす
  private void shiftMapPosition(float xdiff, float ydiff)
   {
    offsetX += xdiff;
    offsetY += ydiff;

    if (offsetX > 0) offsetX = 0.0f;
    else if((Mathf.Abs(offsetX) + Screen.width) >= width)
     {
      offsetX = -(width - Screen.width);
     }

    if (offsetY > 0) offsetY = 0.0f;
    else if ((Mathf.Abs(offsetY) + Screen.height) >= height)
     {
      offsetY = -(height - Screen.height);
     }
  //text ="offsetX =" + offsetX + "\noffsetY = " + offsetY + "\nwidth=" + width + "\nheight = " + height;
    mapposition.Set(offsetX, offsetY, width, height);
   }

  //初期化
  private void initialize()
   {
    //地図の最大高さを指定
    maximumheight = minimumheight * 3.0f;

    //地図の大きさ
    width = minimumheight * 2.0f;
    height = minimumheight;

    //Rect初期化
    shiftMapPosition(0.0f, 0.0f);

    ready = true;
    enabled = false;
   }


  IEnumerator getTextureAsset(string datafilename)
   {
    if (Application.platform == RuntimePlatform.WindowsEditor)
     {
      string path = Application.dataPath + "/StreamingAssets/" + datafilename;
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
          //container.iconimage = texture;
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
    switch (datafilename)
     {
      case mapimagefile: worldmap = tex; break;
      case decideimagefile: decidebutton = tex; break;
      case leftupfile: leftuparrow = tex; break;
      case leftdownfile: leftdownarrow = tex; break;
      case rightupfile: rightuparrow = tex; break;
      case rightdownfile: rightdownarrow = tex; break;
     }
   }

  public void setPointReceiver(PointChangeReceiver caller, float lon, float lat)
   {
    pointreceiver = caller;
    longitude = lon;
    latitude = lat;
    height = minimumheight;
    width = height * 2;

    //初期位置を設定する
    float x = longitude < 0 ? (longitude + 360.0f) : longitude;
    float y = latitude / 90.0f;
    x = x / 360.0f * width;
    y = (1 + y) * (height / 2);
    offsetX = -(x - (Screen.width / 2));
    offsetY = -(y - (Screen.height / 2));
    //表示位置を補正
    shiftMapPosition(0.0f, 0.0f);
   }
 }
