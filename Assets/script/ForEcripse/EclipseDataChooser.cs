using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;


class EclipseIcon
 {
  public Texture2D iconimage;
  public Point position;
  public int distance;

  public EclipseIcon()
   {
    position = new Point(0, 0);
   }
 }

class HeadLine
 {
  public string text;
  public int fontsize;
  public int width;
  public int height;
  public Point position;
 }


public class EclipseDataChooser : MonoBehaviour
 {
  private const int VERTICAL_DIRECTION = 2;
  private const int HORIZONTAL_DIRECTION = 6;
  private const int vertical_margin = 20;
  private const int horizontal_margin = 10;
  private const int iconmargin = 20;
  private const float deaccelation = 2000.0f;
  private const float taplength = 10.0f;
  private const string icondirectory = "icons/";
  private const string windowsext = ".png";
  private const string androidext = ".jpg";
  private const string horizontal_handle_filename = "horizontalsliderhandle.png";
  private const string vertical_handle_filename = "verticalsliderhandle.png";
  private const string smokepixel_filename = "smoke.png";
  private const string unknownimage_filename = "unknown.png";

  //レイアウト関連
  private int iconwidth;
  private int iconheight;
  private int vertical_icon_baseline;
  private int horizontal_icon_baseline;
  private DeviceOrientation lastorientation;

  //スライダー関連
  private int handlewidth;
  private int handleheight;
  private int largestwidth;
  private float offset;
  private int handleleft;
  private int handletop;
  private int horizontal_slider_baseline;
  private int vertical_slider_baseline;
  private int sliderhandlelength;
  //画面操作関連
  private float startX;
  private float startY;
  private float lastX;
  private float lastY;
  private float swipespeed;
  private EclipseIcon[] eclipseicons;
  //読み込んだ画像を格納
  private Texture2D unknownicon;
  private Texture2D smoke;
  private Texture2D horizontalhandle;
  private Texture2D verticalhandle;
  //現在使用する画像を示す参照値
  private Texture2D sliderhandle;

  //フラグ
  public bool ready;
  private bool active;
  private bool dragged;
  private bool handlepicked;

  private DataChangeReceiver receiver;
 //debug
 private string text;
  private GUIStyle labelstyle;


  void Start ()
   {
    labelstyle = new GUIStyle();
    labelstyle.fontSize = Screen.height / 20;
    labelstyle.normal.textColor = Color.white;

    active = true;
    offset = 0;

    eclipseicons = new EclipseIcon[EclipseCalendar.schedule.Length];
    for (int i = 0; i < EclipseCalendar.schedule.Length; i++)
     {
      eclipseicons[i] = new EclipseIcon();
     }

    StartCoroutine("getTextureAsset", horizontal_handle_filename);
    StartCoroutine("getTextureAsset", vertical_handle_filename);
    StartCoroutine("getTextureAsset", smokepixel_filename);
    StartCoroutine("getTextureAsset", unknownimage_filename);

    initializeIconBoard();

   }

  // Update is called once per frame
  void Update ()
   {
    if(active)
     {
      if (ready)
       {
        switch (getOrientation())
         {
          case VERTICAL_DIRECTION:
           operationInPortrait();
           break;
          case HORIZONTAL_DIRECTION:
           operationInLandscape();
           break;
         }
       }
      else
       {
        if(horizontalhandle != null & verticalhandle != null & smoke != null & unknownicon != null)
         {
          //必要なテクスチャの読み込みが終わったら初期化
          initializeIconBoard();
         }
       }
     }

    //有効なデバイスの向きを記録する
    if (Input.deviceOrientation != DeviceOrientation.Unknown & Input.deviceOrientation != DeviceOrientation.FaceDown & Input.deviceOrientation != DeviceOrientation.FaceUp)
     {
      lastorientation = Input.deviceOrientation;
     }

    //終了処理
    if (Application.platform == RuntimePlatform.Android)
     {
      // エスケープキー取得
      if (Input.GetKeyDown(KeyCode.Escape))
       {
        // キャンセル処理
        active = false;
        enabled = false;
        return;
       }
     }
   }


  void OnGUI()
   {
    if(active & ready)
     {
 
      GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), smoke);
      int direction = getOrientation();
      switch (direction)
       {
        case VERTICAL_DIRECTION:
         drawIconsVerticalDirection();
         break;
        case HORIZONTAL_DIRECTION:
         drawIconsHorizontalDirection();
         break;
       }
      adjustSliderHandlePosition();
      GUI.DrawTexture(new Rect(handleleft, handletop, handlewidth, handleheight), sliderhandle);

     //Debug.Log("handletop =" + handletop + " handle left = " + handleleft);
     //GUI.Label(new Rect(20, 20, 500, 400), text, labelstyle);
     }
   }

  private int getOrientation()
   {
    if (Input.deviceOrientation == DeviceOrientation.Portrait | Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
     {
      return VERTICAL_DIRECTION;
     }
    else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft | Input.deviceOrientation == DeviceOrientation.LandscapeRight)
     {
      return HORIZONTAL_DIRECTION;
     }
    else
     {
      if (lastorientation == DeviceOrientation.Portrait | lastorientation == DeviceOrientation.PortraitUpsideDown)
       {
        return VERTICAL_DIRECTION;
       }
      else if (lastorientation == DeviceOrientation.LandscapeLeft | lastorientation == DeviceOrientation.LandscapeRight)
       {
        return HORIZONTAL_DIRECTION;
       }
     }
    //一度も直立していない場合はPortraitで表示する
    return VERTICAL_DIRECTION;//
    //return HORIZONTAL_DIRECTION;
   }


  //Portraitの時のマウス（タッチ）の処理
  private void operationInPortrait()
   {
    //マウス検知
    if (Input.GetMouseButtonDown(0))
     {
      Vector3 mousepos = Input.mousePosition;
      float X = mousepos.x;
      float Y = Screen.height - mousepos.y;
      if (!handlepicked)
       {
        if (handleleft <= X & (handleleft + handlewidth) >= X & handletop <= Y & (handletop + handleheight) >= Y)
         {
          handlepicked = true;
         }
       }
      lastX = mousepos.x;
      lastY = mousepos.y;

      if (!dragged & !handlepicked)
       {
        dragged = true;
    //text = "dragged";
        startX = mousepos.x;
        startY = mousepos.y;
       }
      return;
     }

    //ボタンが離された
    if (Input.GetMouseButtonUp(0))
     {
      Vector3 mousepos = Input.mousePosition;
      if (dragged)
       {
        //text = "startX =" + startX + "\nstartY =" + startY + "\nx=" + mousepos.x + "\ny= "+ mousepos.y;
        if (Mathf.Abs(startX - mousepos.x) < taplength & Mathf.Abs(startY - mousepos.y) < taplength)
         {
          int x = (int)(startX);
          int y = (int)(Screen.height - startY + Mathf.Abs(offset));//offsetはマイナス値
          for (int i = 0; i < eclipseicons.Length;i++)
           {
            int left = vertical_icon_baseline;
            int right = left + iconwidth;
            int top = eclipseicons[i].distance;
            int bottom = top + iconheight;

            if ((top + offset) > Screen.height | (bottom + offset) < 0) continue;

            if (left <= x & x <= right & top <= y & y <= bottom)
             {
              //Debug.Log("choosen " + i);
              receiver.EclipseDataChange(i);
              active = false;
              enabled = false;
              break;
             } 
           }
         }
       }
      dragged = false;
      handlepicked = false;
      return;
     }

    if (handlepicked)
     {
      //float ydiff = -(Input.mousePosition.y - lastY); //unityの座標系は左下が原点のため、この式を書き換えて下を得る
      float ydiff = lastY - Input.mousePosition.y;//unityの座標系は左下が原点のため、

      lastX = Input.mousePosition.x;
      lastY = Input.mousePosition.y;

      if (ydiff == 0.0f) return;

      offset += ydiff;
      moveSliderHandle(ydiff);

      if (offset > 0) { offset = 0; }

      if ((Math.Abs(offset) + Screen.height) >= largestwidth) { offset = Screen.height - largestwidth; }
     }
    else if (dragged)
     {
      float ydiff = lastY - Input.mousePosition.y;
      lastX = Input.mousePosition.x;
      lastY = Input.mousePosition.y;

      if (ydiff == 0.0f) return;
      offset += ydiff;

      swipespeed = ydiff / Time.deltaTime;
 
      if (offset > 0) { offset = 0; swipespeed = 0; }

      if ((Math.Abs(offset) + Screen.height) >= largestwidth) { offset = Screen.height - largestwidth; swipespeed = 0; }
     }
    else
     {
      if (swipespeed != 0)
       {
        float ydiff = swipespeed * Time.deltaTime;
        offset += ydiff;
        if (swipespeed < 0){ swipespeed += (deaccelation * Time.deltaTime); }
        else { swipespeed -= (deaccelation * Time.deltaTime); }

        if (offset > 0) { offset = 0; swipespeed = 0; }
        if ((Math.Abs(offset) + Screen.height) >= largestwidth) { offset = Screen.height - largestwidth; swipespeed = 0; }
        adjustSliderHandlePosition();
       }
     }
   }


  //Landscapeの時のマウス（タッチ）の処理
  private void operationInLandscape()
   {
    //マウス検知
    if (Input.GetMouseButtonDown(0))
     {
      Vector3 mousepos = Input.mousePosition;
      float X = mousepos.x;
      float Y = Screen.height - mousepos.y;
      if (!handlepicked)
       {
        if (handleleft <= X & (handleleft + handlewidth) >= X & handletop <= Y & (handletop + handleheight) >= Y)
         {
          handlepicked = true;
         }
       }
      lastX = mousepos.x;
      lastY = mousepos.y;

      if (!dragged & !handlepicked) 
       {
        dragged = true;
        startX = mousepos.x;
        startY = mousepos.y;
       }
      return;
     }

    //ボタンが離された
    if (Input.GetMouseButtonUp(0))
     {
      Vector3 mousepos = Input.mousePosition;
      if (dragged)
       {
        if (Mathf.Abs(startX - mousepos.x) < taplength & Mathf.Abs(startY - mousepos.y) < taplength)
         {
          int x = (int)(startX + Math.Abs(offset)) ;
          int y = (int)(Screen.height - startY);

          for (int i = 0; i < eclipseicons.Length; i++)
           {
            int left = eclipseicons[i].distance;
            int right = left + iconwidth;
            int top = horizontal_icon_baseline;
            int bottom = top + iconheight;

            if ((left + offset) > Screen.width | (right + offset) < 0) continue;
            if (left <= x & x <= right & top <= y & y <= bottom)
             {
              //Debug.Log("choosen " + i);
              receiver.EclipseDataChange(i);
              active = false;
              enabled = false;
              break;
             }
           }
         }
       }
      
      dragged = false;
      handlepicked = false;
      return;
     }

    if (handlepicked)
     {
      float xdiff = Input.mousePosition.x - lastX ;

      moveSliderHandle(xdiff);

      lastX = Input.mousePosition.x;
      lastY = Input.mousePosition.y;
     }
    else if (dragged)
     {
      float xdiff = Input.mousePosition.x - lastX;
      offset += xdiff;
      swipespeed = xdiff / Time.deltaTime;
      if (offset > 0) offset = 0;

      if ((Math.Abs(offset) + Screen.height) >= largestwidth) { offset = Screen.height - largestwidth; }
      adjustSliderHandlePosition();
      lastX = Input.mousePosition.x;
      lastY = Input.mousePosition.y;
     }
    else
     {
      if (swipespeed != 0)
       {
        float xdiff = swipespeed * Time.deltaTime;
        offset += xdiff;
        if (swipespeed < 0) { swipespeed += (deaccelation * Time.deltaTime); }
        else { swipespeed -= (deaccelation * Time.deltaTime); }

        if (offset > 0) offset = 0;
        if ((Math.Abs(offset) + Screen.height) >= largestwidth) { offset = Screen.height - largestwidth; }
        adjustSliderHandlePosition();
       }
     }
   }

  //Portraitの時の描画
  private void drawIconsVerticalDirection()
   {
    for (int i = 0; i < eclipseicons.Length; i++)
     {
      int top = (int)(offset + eclipseicons[i].distance);
      int bottom = top + iconheight;

      if ((top >= 0 & top < Screen.height) | (bottom >= 0 & bottom < Screen.height))
       {
        Texture2D image;
        if (eclipseicons[i].iconimage != null) image = eclipseicons[i].iconimage;
        else image = unknownicon;

        GUI.DrawTexture(new Rect(vertical_icon_baseline, top, iconwidth, iconheight), image);
       }
     }
   }

  //Landscapeの時の描画
  private void drawIconsHorizontalDirection()
   {
    for (int i = 0; i < eclipseicons.Length; i++)
     {
      int left = (int)(offset + eclipseicons[i].distance);
      int right = left + iconwidth;

      if ((left >= 0 & left < Screen.width) | (right >= 0 & right < Screen.width))
       {
        Texture2D image;
        if (eclipseicons[i].iconimage != null) image = eclipseicons[i].iconimage;
        else image = unknownicon;
        GUI.DrawTexture(new Rect(left, horizontal_icon_baseline, iconwidth, iconheight), image);
       }
     }
   }


  //マウス（タッチ）のイベントでスライダーを動かす
  private void moveSliderHandle(float movewidth)
   {
    switch(getOrientation())
     {
      case VERTICAL_DIRECTION:
       int maximumValue = largestwidth - Screen.height;
       float sliderwidth = Screen.height - sliderhandlelength;
       float widthperpixel = (float)(largestwidth - Screen.height) / sliderwidth;

       float top = handletop + movewidth;

       if (top < 0) top = 0;
       else if (top > sliderwidth) top = sliderwidth;

       offset = -(int)(top / sliderwidth * maximumValue);

       handletop = (int)top;
       handleleft = Screen.width - verticalhandle.width;// vertical_slider_baseline;

       break;
      case HORIZONTAL_DIRECTION:
       maximumValue = largestwidth - Screen.width;
       sliderwidth = Screen.width - sliderhandlelength;
       widthperpixel = maximumValue / sliderwidth;

       float left = handleleft + movewidth;

       if (left < 0) left = 0;
       else if (left > sliderwidth) left = sliderwidth;//スライダーの範囲を超えたら最大値にセット

       offset = -(int)(left / sliderwidth * maximumValue) ;

       handletop = Screen.height - horizontalhandle.height;// horizontal_slider_baseline;
       handleleft = (int)left;
       break;
     }
   }

  //スライダーハンドルの位置を決める
  private void adjustSliderHandlePosition()
   {
    if (!ready) return;
    switch (getOrientation())
     {
      case HORIZONTAL_DIRECTION:
       sliderhandlelength = 200;
       float sliderwidth = Screen.width - sliderhandlelength;
       float widthperpixel = (float)(largestwidth - Screen.width) / sliderwidth;
       handleleft = (int)(-offset / widthperpixel);
       handletop = Screen.height - horizontalhandle.height;// horizontal_slider_baseline;
       handlewidth = horizontalhandle.width;
       handleheight = horizontalhandle.height;
      sliderhandle = horizontalhandle;
       break;
      case VERTICAL_DIRECTION:
       sliderhandlelength = 200;
       sliderwidth = Screen.height - sliderhandlelength;
       widthperpixel = (float)(largestwidth - Screen.height) / sliderwidth;
       handleleft = Screen.width - verticalhandle.width;// vertical_slider_baseline;
       handletop = (int)(-offset / widthperpixel);
       handlewidth = verticalhandle.width;
       handleheight = verticalhandle.height;
       sliderhandle = verticalhandle;
       break;  
     }
   }

 public void setEnabled(bool val, DataChangeReceiver callback)
   {
    receiver = callback;
    offset = 0;

    active = val;
   }

  public bool getEnabled()
   {
    return active;
   }

  //画面の初期化
  private void initializeIconBoard() 
   {
  //Debug.Log("length = " + EclipseCalendar.schedule.Length);
    int shortside = 0;
    if (Screen.width < Screen.height) shortside = Screen.width;
    else shortside = Screen.height;

    if (shortside <= 720)
     {
      iconwidth = 500;
      iconheight = 500;
      vertical_icon_baseline = 50;
      horizontal_icon_baseline = 70;
     }
    else if (shortside <= 1080)
     {
      iconwidth = 600;
      iconheight = 600;
      vertical_icon_baseline = 80;
      horizontal_icon_baseline = 70;
     }
    largestwidth = iconwidth * EclipseCalendar.schedule.Length * iconmargin * (EclipseCalendar.schedule.Length - 1);

  //一番上にマージンを加算する。
    int length = 0;
    for (int i = 0; i < EclipseCalendar.schedule.Length; i++)
     {
      int[] date = EclipseCalendar.schedule[i];

      //ファイル名を生成
      string filename = date[EclipseCalendar.START_YEAR].ToString();
      if (date[EclipseCalendar.START_MONTH] < 10) filename += "0";
      filename += date[EclipseCalendar.START_MONTH].ToString();
      if (date[EclipseCalendar.START_DAY] < 10) filename += "0";
      filename += date[EclipseCalendar.START_DAY].ToString();

      eclipseicons[i] = new EclipseIcon();

      if (Application.platform == RuntimePlatform.WindowsEditor)
       {
        StartCoroutine("getTextureAsset", filename + windowsext);
       }
      else if (Application.platform == RuntimePlatform.Android)
       {
        StartCoroutine("getTextureAsset", icondirectory + filename + androidext);
       }

      eclipseicons[i].distance = length;
      length += (iconwidth + iconmargin);
     }
　  largestwidth = length - iconmargin;//最大の幅。最後の1回分のマージンを削除 
    ready = true;
    enabled = false;
    active = false;
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
/*
        else
         {
          container.iconimage = unknownicon;
         }
*/
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
      //container.iconimage = www.texture;
   // text = "count =" + www.text.Length.ToString();
     }
   }


  //読み込んだテクスチャを変数に格納する
  private void setTexture(Texture2D tex, string datafilename)
   {
    //日食の日付と比較する

    if (datafilename.IndexOf(icondirectory) != -1)
     {
      string namepart = datafilename.Substring(0, datafilename.LastIndexOf("."));

      for (int i = 0; i < EclipseCalendar.schedule.Length; i++)
       {
        string datestring = icondirectory + EclipseCalendar.schedule[i][0].ToString();
        if (EclipseCalendar.schedule[i][1] < 10) datestring += "0";
        datestring += EclipseCalendar.schedule[i][1].ToString();
        if (EclipseCalendar.schedule[i][2] < 10) datestring += "0";
        datestring += EclipseCalendar.schedule[i][2].ToString();

        if (namepart == datestring) { eclipseicons[i].iconimage = tex; return; }
       }
     }
    else
     {
      if (datafilename == vertical_handle_filename) { verticalhandle = tex; }
      else if (datafilename == horizontal_handle_filename) { horizontalhandle = tex; }
      else if (datafilename == smokepixel_filename) { smoke = tex; }
      else if (datafilename == unknownimage_filename) { unknownicon = tex; }
     }
   }
 }
