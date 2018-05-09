using UnityEngine;
using System;
using System.IO;

public class UmbralShadowRenderer
{
 private const float DegToRad = Mathf.PI / 180.0f;
 private const float RadToDeg = 180.0f / Mathf.PI;
 public const int PLAYMODE = 1;
 public const int RECORDMODE = 2;

 private int mode;
 //  private bool play = false;
 //  private double interval;
 private Texture2D shadow;
 private Texture2D umbra;

 //private bool painted;
 //private Material material;

 //private float distanceToMoon;
 private DayAndNight earthrenderer;
 private double[] dawnline;
 private int[] sunline;

 //JSON関連
 private EclipseData dataholder;

 //描画色
 private Color shadowcolor = new Color(0.0f, 0.1f, 0.4f, 0.4f);
 private Color boundscolor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
 private Color sunoutline = new Color(1.0f, 0.0f, 0.0f, 1.0f);
 //private Color sunoutline = new Color(0.0f, 0.1f, 0.4f, 0.4f);
 private Color centercolor = Color.red;

 //フラグ
 private bool writejson;
 private bool finished;
 private bool dataread;
 //外部からの設定
 public bool drawnightside;

 public void setEclipseData(EclipseData data, Texture2D tex, Texture2D tex2, int m)
  {
   if (m != PLAYMODE & m != RECORDMODE) return;
   mode = m;

   dataholder = data;
   dataread = true;
   umbra = tex2;
   init(tex);
  }

 public void setEclipseData(EclipseData data, Texture2D tex, int m)
  {
  if (m != PLAYMODE & m != RECORDMODE) return;
  mode = m;

  dataholder = data;
  dataread = true;
  init(tex);
 } 
 
 private void init(Texture2D tex)
  {
  writejson = false;
  finished = false;

  //Debug.Log(checkPCManufacturer());

  //テクスチャ生成
  shadow = tex;//new Texture2D(512, 512);
               //昼夜を描くクラスを初期化
  earthrenderer = new DayAndNight(shadow);
  //テクスチャ画像消去
  clearTexture();
  //同時線の配列
  sunline = new int[(shadow.width + shadow.height) * 4];

  //同時線の座標を格納する配列
  dawnline = new double[shadow.height * 4];
 }

 /*
   void Start()
    {
     //時刻はUTCで設定する
     int[] date = EclipseCalendar.schedule[5];
     end = new DateTime(date[5], date[6], date[7], date[8], date[9], 0, DateTimeKind.Utc);
     utc = new DateTime(date[0], date[1], date[2], date[3], date[4], 0, DateTimeKind.Utc);
     dataholder = new EclipseData(utc, end, 1);
     EclipseData archive = readJSONData("20300601.json");
   //    end = new DateTime(2016, 3, 9, 4, 30, 0, DateTimeKind.Utc);
   //    utc = new DateTime(2016, 3, 8, 23, 30, 0, DateTimeKind.Utc);
     if (archive != null) { Debug.Log("archive retreived"); dataholder = archive; dataread = true; }
     else dataread = false; 
     //dataread = false;
     writejson = false;
     finished = false;

     //Debug.Log(checkPCManufacturer());
     //テクスチャ生成
     shadow = new Texture2D(512, 512);
     //昼夜を描くクラスを初期化
     earthrenderer = new DayAndNight(shadow);
     //テクスチャ画像消去
     clearTexture();

     //同時線の配列
     sunline = new int[(shadow.width + shadow.height) * 4];

   //テクスチャをマテリアルにセット

     GameObject obj = GameObject.Find("perfectsphere");
     Material[] mats = obj.GetComponent<Renderer>().materials;
     mats[1].SetTexture("_MainTex", shadow);

   //同時線の座標を格納する配列
     dawnline = new double[shadow.height * 4];
     //計算開始
     interval = 0.0;
     play = true;
     //Debug.Log("persistentChachePath=" + Application.persistentDataPath);
    }
  /*
    private string checkPCManufacturer()
     {
      //キー（HKEY_CURRENT_USER\Software\test\sub）を読み取り専用で開く
      Microsoft.Win32.RegistryKey regkey =
          Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation",false);
      //キーが存在しないときは null が返される
      if (regkey == null)
       {
        return "fault";
       }
      string stringValue = (string)regkey.GetValue("SystemManufacturer");
      regkey.Close();

      return stringValue;
     }
  */
 /*
  // Update is called once per frame
   void Update()
    {
     if (play)
      {
       interval += Time.deltaTime;

       if (interval > 0.1)
        {
         utc = utc.AddMinutes(1);
         //終了時刻よりあとの時刻か
         if (utc.CompareTo(end) > 0)
          {
           play = false;

           if(!finished)
            {
             if (!writejson) { dataholder.writeJSON(filename); writejson = true; }
             finished = true;
            }
           return;
          }

         painted = false;
         //updateScreen2();
         //テクスチャ画像消去
         clearTexture();
         drawLines();
         //地球の昼夜を更新
         earthrenderer.updateTime(utc);
         earthrenderer.updateScreen();
         //changeColor(sunoutline, shadowcolor);
         shadow.Apply();
        //        material.SetTexture("_MainTex", shadow);
         interval = 0.0;
        }
      }
    }
 */

  public int getMode()
   {
    return mode;
   }

  //UTCから地球の昼夜を描く
  public void drawNightSide(DateTime utc)
   {
    earthrenderer.updateTime(utc);
    //earthrenderer.updateScreen();
   }


  //UTCから地球の昼夜を描く 
  public void drawNightSide(double[] data)
   {
    earthrenderer.calculateSunPosition(data);
    earthrenderer.updateScreen();
   }


  //地球に写る月の影の輪郭を描く
  public void drawLines(DateTime utc)
   {
    EquatorialCoordinate sun = new EquatorialCoordinate();
    EquatorialCoordinate moon = new EquatorialCoordinate();

    double[] result = new double[3];
    double[] result2 = new double[3];
    double[] dataset = new double[7];
    double[] location = new double[2];
    double asc = 0.0;
    double dec = 0.0;
    double moonasc = 0.0;
    double moondec = 0.0;
    double phai0;

    if (mode == PLAYMODE)
     {
      dataholder.getPositions(utc, dataset);
      asc = dataset[0];
      dec = dataset[1];
      sun.setRightAscension(dataset[0]);
      sun.setCelestialDeclination(dataset[1]);
      sun.setDistance(dataset[2]);

      moon.setRightAscension(dataset[3]);
      moon.setCelestialDeclination(dataset[4]);
      moon.setDistance(dataset[5]);
      moonasc = dataset[3];
      moondec = dataset[4];
      phai0 = dataset[6];
     }
    else if (mode == RECORDMODE)
     {
      try
       {
        SunAndMoon.getSunRightAscension(utc, result);
        asc = result[0];
        dec = result[1];
        sun.setRightAscension(result[0]);
        sun.setCelestialDeclination(result[1]);
        sun.setDistance(result[2]);

        SunAndMoon.getMoonRightAscension(utc, result);
        moon.setRightAscension(result[0]);
        moon.setCelestialDeclination(result[1]);
        moon.setDistance(result[2]);
        moonasc = result[0];
        moondec = result[1];
       }
      catch (Exception) {/* Debug.Log("Exception1 ");*/ return; }
      phai0 = Almanac.getGreenidgeSiderealTime(utc);//グリニッジ恒星時
      //記録する
      dataholder.setPositions(asc, dec, sun.getDistance(), moonasc, moondec, moon.getDistance(), phai0, utc);
     }
    else return;

    //輪郭の描画
    //ベッセル数
    VesselElements ve = new VesselElements(sun, moon, utc);
    // SolarEclipse.getD(ve, 143.009267, 42.7396185, 0);
    SolarEclipse.getCrossPoint(ve, result, result2);
    //月影と地球外円との交点を調べる
    double maxQ = SolarEclipse.getPenumbralQ(ve, result);
    double minQ = SolarEclipse.getPenumbralQ(ve, result2);
    //Debug.Log("MaxQ = " + maxQ + " minQ = " + minQ);

    //半影の描画
    if (Double.IsNaN(maxQ) && Double.IsNaN(minQ))
     {
      clearTexture();
      //交点が存在しない場合。月影がすべて地球上に投射されている。
      double last_longitude;
      double last_latitude;
      Color gray = new Color(0, 0, 0, 0.4f);

      //初期の点
      SolarEclipse.getPenumbralOutline(ve, 0.0, result);
      Coordinate.transformICRStoGCS(ve, result, location);
      last_longitude = location[0];
      last_latitude = location[1];
      double max_lat = -90, min_lat = 90;
      double east_lon = location[0];
      double west_lon = double.NaN;

      //bool
      for (double i = 1.0; i <= 360.0; i += 1.0)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result2);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2]))
         {
          continue;//NaNが含まれていたらスキップする
         }
        Coordinate.transformICRStoGCS(ve, result2, location);

        if (double.IsNaN(location[0]) | double.IsNaN(location[1])) continue;
        //値の取得で例外が発生したら、処理をスキップする
        try
         {
          drawOutline(last_longitude, last_latitude, location[0], location[1], shadow, sunoutline);
         }
        catch(Exception ex){ continue; }

        last_longitude = location[0];
        last_latitude = location[1];
        if (i == 180.0) west_lon = location[0];
        if (max_lat < last_latitude) max_lat = last_latitude;
        if (min_lat > last_latitude) min_lat = last_latitude;
       }

      if(!double.IsNaN(west_lon) & !double.IsNaN(east_lon))
       {
        if (west_lon > east_lon) east_lon += 360.0;

        Point paintpoint = getScreenPoint((east_lon + west_lon) / 2, (max_lat + min_lat) / 2);
        fillShadowEx(paintpoint.x, paintpoint.y, shadow, shadowcolor);
       // Debug.Log("west= " + west_lon + " east=" +east_lon) ;
       }
     }
    else if (!Double.IsNaN(maxQ) && !Double.IsNaN(minQ))//交点が存在する
     {
      double first_longitude;
      double first_latitude;
      double last_longitude;
      double last_latitude;

      clearTexture();
      Color gray = new Color(0, 0, 0, 0.4f);
      if ((maxQ - minQ) >= 0.0) maxQ -= 360.0;
      SolarEclipse.getPenumbralOutline(ve, maxQ, result);
      Coordinate.transformICRStoGCS(ve, result, location);
      //Debug.Log("MaxQ :" + location[0] + ":" + location[1]+ ":" + maxQ);
      SolarEclipse.getPenumbralOutline(ve, minQ, result);
      Coordinate.transformICRStoGCS(ve, result, location);

      //初期の点
      double delta = 0.0; 
      while (true)
       {
        SolarEclipse.getPenumbralOutline(ve, maxQ + delta, result);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) delta += 0.1;
        else break;
        if (delta > 5.0) break;
       }

      Coordinate.transformICRStoGCS(ve, result, location);
      first_longitude = last_longitude = location[0];
      first_latitude = last_latitude = location[1];
     // double max_lon = 0, max_lat = -90, min_lon = 360, min_lat = 90;

      for (double i = maxQ + delta + 0.5 ; i < minQ; i += 0.5)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result2);
        if (Double.IsNaN(result2[0]) | Double.IsNaN(result2[1]) | Double.IsNaN(result2[2]))
         {
          continue;//NaNが含まれていたらスキップする
         }
        Coordinate.transformICRStoGCS(ve, result2, location);

        //値の取得で例外が発生したら、処理をスキップする
        if (double.IsNaN(location[0]) | double.IsNaN(location[1])) continue;
        try 
         {
          drawOutline(last_longitude, last_latitude, location[0], location[1], shadow, sunoutline);
         }
        catch (Exception e){ continue;  }

        last_longitude = location[0];
        last_latitude = location[1];
       }

      SolarEclipse.getPenumbralOutline(ve, minQ, result);
      if (!Double.IsNaN(result[0]) & !Double.IsNaN(result[1]) & !Double.IsNaN(result[2]))
       {
        Coordinate.transformICRStoGCS(ve, result, location);
        if (!double.IsNaN(location[0]) & !double.IsNaN(location[1]))
         {
          drawOutline(last_longitude, last_latitude, location[0], location[1], shadow, sunoutline);
          last_longitude = location[0];
          last_latitude = location[1];
         }
       }

      //同時線を描く
      {
       //日の出･日の入りの同時線を描く
       int alllength = getSunLine(sun, phai0);

       //終点から同時線までの最短の線を描く
       Point pnt = getScreenPoint(last_longitude, last_latitude);
       double leastdistance = double.MaxValue;
       int finishindex = -1; //終点と最も近い点のインデックス
       if(pnt.x < 0) pnt.x += shadow.width;
       else if (pnt.x >= shadow.width) pnt.x -= shadow.width;
       if (pnt.y < 0) pnt.y = 0;
       else if (pnt.y >= shadow.width) pnt.y = shadow.width;

       for (int i = 0; i < alllength; i++)
        {
         double xdiff = Math.Abs(pnt.x - sunline[i * 2]);
         if (xdiff > shadow.width / 2) xdiff = shadow.width - xdiff;
         double ydiff = pnt.y - sunline[i * 2 + 1];
         double length = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
         if(length < leastdistance){ leastdistance = length; finishindex = i; }
        }
      if(finishindex != -1 & leastdistance != double.MaxValue)
       {
        if (!double.IsNaN(last_longitude) & !double.IsNaN(last_latitude))
         drawScreenOutline(pnt.x, pnt.y, sunline[finishindex * 2], sunline[finishindex * 2 + 1], sunoutline);
       }

       //開始点から同時線までの最短の線を描く
       pnt = getScreenPoint(first_longitude, first_latitude);
       leastdistance = double.MaxValue;
       int startindex = -1; //始点と最も近い点のインデックス
       if (pnt.x < 0) pnt.x += shadow.width;
       else if (pnt.x >= shadow.width) pnt.x -= shadow.width;
       if (pnt.y < 0) pnt.y = 0;
       else if (pnt.y >= shadow.width) pnt.y = shadow.width;

       for (int i = 0; i < alllength; i++)
        {
         double xdiff = Math.Abs(pnt.x - sunline[i * 2]);
         if (xdiff > shadow.width / 2) xdiff = shadow.width - xdiff;
         double ydiff = pnt.y - sunline[i * 2 + 1];
         double length = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
         if (length < leastdistance) { leastdistance = length; startindex = i; }
        }
       if (startindex != -1 & leastdistance != double.MaxValue)
        {
         if (!double.IsNaN(last_longitude) & !double.IsNaN(last_latitude))
         drawScreenOutline(pnt.x, pnt.y, sunline[startindex * 2], sunline[startindex * 2 + 1], sunoutline);
        }
       //判定を正確にするため、同時線は最後に描く。alllengthは要素数だから、マイナスしない
       for (int i = 0; i < alllength; i++)
        {
         drawScreenPixel(sunline[i * 2], sunline[i * 2 + 1], boundscolor);
        }

      //影の中を塗る
       int middlepoint = -1;
       if (Math.Abs(finishindex - startindex) > (alllength / 2))
        {
         middlepoint = (finishindex + startindex + alllength) / 2;
         if (middlepoint >= alllength) middlepoint -= alllength;
         else if (middlepoint < 0) { middlepoint += alllength; }
        }
       else
        {
         middlepoint = (finishindex + startindex) / 2;
        }
       getInnerPoint(sunline[middlepoint * 2], sunline[middlepoint * 2 + 1], shadow);//塗の指示
      }//同時線の描画の終わり

    } //半影の描画の終わり
   else if(!Double.IsNaN(maxQ) | !Double.IsNaN(minQ)) { /*Debug.Log("minQ or maxQ is NaN");*/ }

    //本影の描画
     { 
      bool start = false;
      double last_longitude = 0;
      double last_latitude = 0;

      for (int i = 0; i <= 360; i += 5)
       {
        SolarEclipse.getUmbralOutline(ve, (double)i, result);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする

        Coordinate.transformICRStoGCS(ve, result, location);
        if (!start) { last_longitude = location[0]; last_latitude = location[1]; start = true; continue; }
        if (umbra != null) drawOutline(last_longitude, last_latitude, location[0], location[1], umbra, centercolor);
        else drawOutline(last_longitude, last_latitude, location[0], location[1], shadow, centercolor);

        last_longitude = location[0];
        last_latitude = location[1];
       }
     }
   }


  //同時線の点列を得る
  private int getSunLine(EquatorialCoordinate sun, double phai0)
   {
    double dist = sun.getDistance();
    double parallax = SunAndMoon.getSunParallax(dist);//太陽視差
    double k = SunAndMoon.getSunriseAltitude(SunAndMoon.getSunDiameter(dist), 0.0, SunAndMoon.refraction, parallax);
    double celestialdeclination = sun.getCelestialDeclination();
    int halfheight = shadow.height / 2;

    //同時線の緯度・経度を記録した配列を作る
    bool south = false;
    bool north = false;
    int southlatitude = 0, northlatitude = 0;

    for (int i = 0; i < shadow.height; i++)
     {
      double latitude = (double)(i - halfheight) / (double)halfheight * 90.0;

      //緯度を元に時角を計算する
      double jikaku = SunAndMoon.getTimeAngle(k, celestialdeclination, latitude);
      if (!Double.IsNaN(jikaku))//時角がNaNでない
       {
        if (!south) { south = true; southlatitude = i; }
        double hinode_keido = SunAndMoon.reviseAngle(-jikaku + sun.getRightAscension() - phai0);
        double hinoiri_keido = SunAndMoon.reviseAngle(jikaku + sun.getRightAscension() - phai0);
        dawnline[i * 3] = hinode_keido;
        dawnline[i * 3 + 1] = hinoiri_keido;
        dawnline[i * 3 + 2] = latitude;
       }
      else
       {
        if (south & !north) { north = true; northlatitude = i - 1; }
       }
     }

   //配列を作る
   //配列に基づいて線を引く      
    int index = 0;
    for (int i = southlatitude; i < northlatitude; i++)
     {
      int pointer = i * 3;
      double long1 = dawnline[pointer];
      double lat1 = dawnline[pointer + 2];
      pointer = (i + 1) * 3;
      double long2 = dawnline[pointer];
      double lat2 = dawnline[pointer + 2];
      //書き込み
      index = writeToSunLine(getLine(long1, lat1, long2, lat2), index);
    }

    index = writeToSunLine(getLine(dawnline[northlatitude * 3], dawnline[northlatitude * 3 + 2], dawnline[northlatitude * 3 + 1], dawnline[northlatitude * 3 + 2]), index);

    for (int j = northlatitude; j > southlatitude; j--)
     {
      int pointer = j * 3;
      double long1 = dawnline[pointer + 1];
      double lat1 = dawnline[pointer + 2];
      pointer = (j - 1) * 3;
      double long2 = dawnline[pointer + 1];
      double lat2 = dawnline[pointer + 2];
      index = writeToSunLine(getLine(long1, lat1, long2, lat2), index);
     }
    index = writeToSunLine(getLine(dawnline[southlatitude * 3 + 1], dawnline[southlatitude * 3 + 2], dawnline[southlatitude * 3], dawnline[southlatitude * 3 + 2]), index);

    return index;
   }

  private int writeToSunLine(int[] line, int index)
   {
    for (int i = 0; i < line.Length / 2 - 1; i++)
     {
      int u = i * 2;
      int v = index * 2;

      if (line[u] < 0) sunline[v] = line[u] + shadow.width;
      else if (line[u] >= shadow.width) sunline[v] = line[u] - shadow.width;
      else sunline[v] = line[u];
      sunline[v + 1] = line[u + 1];
      index++;
     }
    return index;
   }


  private void getInnerPoint(int x, int y, Texture2D tex)
   {
    bool hitsunline = false;
    bool hitborder = false;
    bool skip = false; 
    //右向きに探索
    int searchpointX = x , searchpointY = y;
    Color linecolor = shadow.GetPixel(searchpointX, searchpointY);

    while (shadow.GetPixel(searchpointX, searchpointY) == linecolor)
     {
      searchpointX++; if(searchpointX >= shadow.width) searchpointX = 0;
      if (searchpointX == x) { skip = true; break; }
     }
    while (!skip)
     {
      Color color = shadow.GetPixel(searchpointX, searchpointY);
      if (!hitsunline & color == sunoutline)
       {
        hitsunline = true;
        fillShadowEx(searchpointX - 1, searchpointY, shadow, shadowcolor);
        return; 
       }
      else if (!hitborder & color == boundscolor) { break; }
      //Debug.Log("searchPointX =" + searchpointX+ " b =" +color.b + ":" + color.b +" g =" + color.g + ":" + color.g);
      searchpointX++;
      if (searchpointX >= tex.width) searchpointX = 0;
      if (searchpointX == x) break;
     }

    //左向きに探索
    hitsunline = false;
    hitborder = false;
    skip = false;
    searchpointX = x; searchpointY = y;
    linecolor = shadow.GetPixel(searchpointX, searchpointY);

    //探索開始地点の線のピクセルはスキップする
    while (shadow.GetPixel(searchpointX, searchpointY) == linecolor)
     {
      searchpointX--;
      if (searchpointX < 0) searchpointX = shadow.width;
      if (searchpointX == x) {skip = true; break; }
     }

    while (!skip)
     {
      Color color = shadow.GetPixel(searchpointX, searchpointY);
      if (!hitsunline & color == sunoutline)
       {
        hitsunline = true;
        if (searchpointX >= shadow.width) searchpointX = 0;
        fillShadowEx(searchpointX + 1, searchpointY, shadow, shadowcolor);
        return;
       }
      else if (!hitborder & color == boundscolor) { break; }
      searchpointX--;
      if (searchpointX < 0) searchpointX = shadow.width - 1;
      if (searchpointX == x) break;
     }

    //上向きに探索
    hitsunline = false;
    hitborder = false;
    skip = false;
    searchpointX = x; searchpointY = y;
    int actualX = searchpointX;
    int actualY = searchpointY;

    //探索開始地点の線のピクセルはスキップする
    linecolor = shadow.GetPixel(searchpointX, searchpointY);
    while (shadow.GetPixel(actualX, actualY) == linecolor)
     {
      searchpointY++;

      if (searchpointY >= shadow.height)
       {
        actualX = searchpointX + shadow.width / 2;//経度を180度回す（反対側に出る）
        if (actualX > shadow.width) actualX -= shadow.width;
        //actualY = shadow.height - (searchpointY - shadow.height) - 1;//この式を書き換えて下の式を得る
        actualY = shadow.height * 2 - searchpointY - 1;
       }
      else { actualY = searchpointY; }//経度が90度を超えていない
     }
   
    while (true)
     {
      Color color = shadow.GetPixel(actualX, actualY);

      if (!hitsunline & color == sunoutline)
       {
        hitsunline = true;
        if (searchpointY >= shadow.height) fillShadowEx(actualX, actualY + 1, shadow, shadowcolor);
        else fillShadowEx(actualX, actualY - 1, shadow, shadowcolor);
        return;
       }
      else if (!hitborder & color == boundscolor) { break; }
      searchpointY++;
      if (searchpointY >= shadow.height)
       {
        actualX = searchpointX + shadow.width / 2;//経度を180度回す（反対側に出る）
        if (actualX > shadow.width) actualX -= shadow.width;
        //actualY = shadow.height - (searchpointY - shadow.height) - 1;//この式を書き換えて下の式を得る
        actualY = shadow.height * 2  - searchpointY - 1;
       }
      else { actualY = searchpointY; }//緯度が90度を超えていない
  
      if (searchpointY >= (shadow.height * 2)) break;//反対側に何も検出されなかったらエラー
     }

    //下向きに探索
    hitsunline = false;
    hitborder = false;
    skip = false;
    searchpointX = x; searchpointY = y;
    actualX = searchpointX;
    actualY = searchpointY;

    //探索開始地点の線のピクセルはスキップする
    linecolor = shadow.GetPixel(searchpointX, searchpointY);
    while (shadow.GetPixel(actualX, actualY) == linecolor)
     {
      searchpointY--;

      if (searchpointY < 0)
       {
        actualX = searchpointX + shadow.width / 2;//経度を180度回す（反対側に出る）
        if (actualX > shadow.width) actualX -= shadow.width;
        //actualY = shadow.height - (searchpointY - shadow.height) - 1;//この式を書き換えて下の式を得る
        actualY = Math.Abs(searchpointY) - 1;
       }
      else { actualY = searchpointY; }//1緯度が0度を下回っていない
     }

    while (true)
     {
      Color color = shadow.GetPixel(actualX, actualY);

      if (!hitsunline & color == sunoutline)
       {
        hitsunline = true;
        if (searchpointY < 0 ) fillShadowEx(actualX, actualY - 1, shadow,shadowcolor);
        else fillShadowEx(actualX, actualY + 1, shadow, shadowcolor);
        return;
       }
      else if (!hitborder & color == boundscolor) { break; }
      searchpointY--;

      if (searchpointY < 0)
       {
        actualX = searchpointX + shadow.width / 2;//経度を180度回す（反対側に出る）
        if (actualX > shadow.width) actualX -= shadow.width;
        //actualY = shadow.height - (searchpointY - shadow.height) - 1;//この式を書き換えて下の式を得る
        actualY = Math.Abs(searchpointY) - 1;//shadow.height * 2 - searchpointY - 1;
       }
      else { actualY = searchpointY; }//緯度が90度を超えていない

      if (Math.Abs(searchpointY) >= shadow.height ) break;//反対側に何も検出されなかったらエラー
     }
   }


  //経度・緯度からテクスチャ上の線の点列を得る
  private int[] getLine(double last_longitude, double last_latitude, double longitude, double latitude)
   {
    int width = shadow.width;
    int height = shadow.height;
    int halfwidth = width / 2;
    int halfheight = height / 2;

    int x1 = (int)(last_longitude / 360.0 * width);
    int y1 = halfheight + (int)(last_latitude / 90 * halfheight);
    int x2 = (int)(longitude / 360.0 * width);
    int y2 = halfheight + (int)(latitude / 90.0 * halfheight);
/*
   //補正すると線が正しく引けないのでコメントアウト
    if (x1 < 0) x1 += width;
    else if (x1 >= width) x1 -= width;

    if (x2 < 0) x2 += width;
    else if (x2 >= width) x2 -= width;
*/
    if (y1 < 0) y1 = 0;
    else if (y1 > height) y1 = height - 1;

    if (y2 < 0) y2 = 0;
    else if (y2 > height) y2 = height - 1;

    //180度以上の開きがある時は経度０をまたぐ
    if (Math.Abs(last_longitude - longitude) > 180.0)
     {
      //x1を基準とし、x2を動かす
      if (x1 > x2) x2 += shadow.width;
      else x2 -= shadow.width;
     }
    return Brezengham.line(x1, y1, x2, y2);
   }

  //テクスチャ上の位置を指定して点を描く
  private void drawScreenPixel(int x1, int y1, Color color)
   {
    shadow.SetPixel(x1, y1, color);
   }

 //テクスチャ上の位置を指定して線を描く
 private void drawScreenOutline(int x1, int y1, int x2, int y2, Color color)
   {
    //180度以上の開きがある時は経度０をまたぐ
    if (Math.Abs(x1 - x2) > (shadow.width /2))
     {
      //x1を基準とし、x2を動かす
      if (x1 > x2) x2 += shadow.width;
      else x2 -= shadow.width;
     }

    int[] linepoints = Brezengham.line(x1, y1, x2, y2);

    for (int i = 0; i < linepoints.Length / 2; i++)
     {
      int u = linepoints[i * 2];
      int v = linepoints[i * 2 + 1];
      if (u < 0) u += shadow.width;
      else if (u >= shadow.width) u -= shadow.width;
//   Debug.Log("u=" + u + " v= "+ v);
      shadow.SetPixel(u, v, color);
     }
   }

  //経度・緯度を指定して線を描く
  private void drawOutline(double last_longitude, double last_latitude, double longitude, double latitude, Texture2D tex, Color color)
   {
    int[] linepoints = getLine(last_longitude, last_latitude, longitude, latitude);
    for (int i = 0; i < linepoints.Length / 2; i++)
     {
      int u = linepoints[i * 2];
      int v = linepoints[i * 2 + 1];
      if (u < 0) u += shadow.width;
      else if (u > shadow.width) u -= shadow.width;

      tex.SetPixel(u, v, color);
     }
   }
 
  //経度・緯度を指定して点を描く
  private void drawShadowTexture(double longitude, double latitude, Texture2D tex, Color color)
   {
    int width = shadow.width;
    int height = shadow.height;
    int halfwidth = width / 2;
    int halfheight = height / 2;

    int x = (int)(halfwidth * longitude / 180.0);
    int y = halfheight + (int)(halfheight * latitude / 90.0);
  //if (x < 0) x += width;

    if (y < 0) y = 0;
    else if (y > height) y = height;
  
    if (x < 0) x += width;
    else if (x > width) x -= width;

    tex.SetPixel(x, y, color);
   }

  //テクスチャ上の点を指定して内部を塗る
  private void fillShadowEx(int x, int y, Texture2D tex, Color color)
   {
    LineSeed.paintCylinder(x, y, tex, color);
   }

/*
  private void fillShadow(double longitude, double latitude, Texture2D tex, Color color)
   {
    int width = shadow.width;
    int height = shadow.height;
    int halfwidth = width / 2;
    int halfheight = height / 2;

    int x = (int)(halfwidth * longitude / 180.0);
    int y = halfheight + (int)(halfheight * latitude / 90.0);
    if (x < 0) x += width;

    if (y < 0) y = 0;
    else if (y > height) y = height;

    if (x < 0) x = 0;
    else if (x > width) x = width;

    LineSeed.paint(x, y, tex, color);
   }
*/
  //閉領域の内部の点を得る
  private void getCoordination(int x, int y, double[] location)
   {
    double longitude = (double)x / (double)shadow.width ;
    if (longitude > 180.0) longitude -= 360.0;

    int halfheight = shadow.height / 2; 
    y -= halfheight;

    double latitude = (double)y / (double)halfheight * 90.0;

    location[0] = longitude;
    location[1] = latitude;
   }

 //経度・緯度のスクリーン上の位置を返す
  private Point getScreenPoint(double longitude, double latitude)
   {
    int width = shadow.width;
    int height = shadow.height;
    int halfwidth = width / 2;
    int halfheight = height / 2;

    return new Point((int)(halfwidth * longitude / 180.0), halfheight + (int)(halfheight * latitude / 90.0));
   }

  //テクスチャの消去
  private void clearTexture()
   {
    Color transparent = new Color(0, 0, 0, 0);

    for (int y = 0; y < shadow.height; y++)
     {
      for (int x = 0; x < shadow.width; x++)
       {
        shadow.SetPixel(x, y, transparent);
       }
     }
   }

  //指定の色を変更する
  private void changeColor(Color src, Color dest)
   {
    for (int y = 0; y < shadow.height; y++)
     {
      for (int x = 0; x < shadow.width; x++)
       {
        if (shadow.GetPixel(x, y) == src)  shadow.SetPixel(x, y, dest);
       }
     }
   }

  private EclipseData readJSONData(string filename)
   {
    // Assetsフォルダに保存する
    string path = Application.dataPath + "/" + filename;

    StreamReader reader = new StreamReader(path);
    string jsonstring = reader.ReadToEnd();
    EclipseData data = null;

    try
     {
      data = JsonUtility.FromJson<EclipseData>(jsonstring);
      reader.Close();
     }
    catch(Exception e){ /*Debug.Log("readJson " + e.ToString());*/ }

    return data;
   }
 
 /*
   //経度からX座標を計算する
   private double getXfromLongitude(double longitude)
   {
     double result = longitude - longitudeleft;

     if (result <= -360.0) { result += Math.Ceiling(result / 360.0) * 360.0; }
     else if (result >= 360.0) { result -= Math.Floor(result / 360.0) * 360.0; }

     result = result / 360.0 * imageWidth; //X軸方向の変換

     if (result > imageWidth) result -= imageWidth;
     else if (result < 0) result += imageWidth;

     return result;
   }

   //Y座標から緯度を計算する
   private double getLatitudeFromY(int y)
   {
     return (double)y / (double)Yequator * 90.0;
   }
 */
 }

