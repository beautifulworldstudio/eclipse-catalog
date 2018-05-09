using UnityEngine;
using System;
using System.IO;



public class UmbralOutline : MonoBehaviour
 {
  private const float DegToRad = Mathf.PI / 180.0f;
  private const float RadToDeg = 180.0f / Mathf.PI;
  private const string moonobject = "Moon";
  private DateTime end;
  private DateTime utc;

  private bool play = false;
  private double interval;
  private Texture2D shadow;
  private bool painted;
  private Material material;

  private float distanceToMoon;
 /*
   private JFrame window;
   private BufferedImage earth;
   private BufferedImage screen;
   private BufferedImage screen2;
   private double scrAngle = 30.0;
   private int imageWidth;
   private int imageHeight;
   private int scrWidth;
   private int scrHeight;
   private double longitudeleft = -29.6;
   private int Yequator = 198;
   private TimeZone tz;
   private Date finish;



   private static readonly double Altitude = 17000; //高度
   private double[] viewpoint; // = new double[]{ -11581.09, 11581.09,0.0};
   private double[][] camvec; // = new double[][]{{-Math.cos(Math.PI / 4.0), -Math.sin(Math.PI / 4.0), 0.0}, { Math.cos(Math.PI / 4.0), -Math.sin(Math.PI / 4.0), 0.0}, { 0.0, 0.0, 1.0 }};
   private double InverseCam[][];
 */

 void Start()
  {
   //月の大きさと距離の初期化
   float earthradius = GameObject.Find("perfectsphere").GetComponent<Renderer>().bounds.size.x / 2;
   distanceToMoon = (float)(Constants.moondistance / Constants.de * earthradius);
  float scale = (float)Constants.Dmoon * earthradius *3.6f;
   GameObject.Find("Moon").transform.localScale = new Vector3(scale, scale, scale);
   Debug.Log("erathradius=" + earthradius+ " scale= " + scale + " distanceToMoon= "+ distanceToMoon);

   //時刻はUTCで設定する
   int[] date = EclipseCalendar.schedule[5];
   end = new DateTime(date[5], date[6], date[7], date[8], date[9], 0, DateTimeKind.Utc);
   utc = new DateTime(date[0], date[1], date[2], date[3], date[4], 0, DateTimeKind.Utc);
  //    end = new DateTime(2016, 3, 9, 4, 30, 0, DateTimeKind.Utc);
  //    utc = new DateTime(2016, 3, 8, 23, 30, 0, DateTimeKind.Utc);
  Debug.Log("passed start");
  //Debug.Log(checkPCManufacturer());
  //計算開始
  interval = 0.0;
  play = true;
  shadow = new Texture2D(1024, 1024);
  Color opaque = new Color(0, 0, 0, 0);
  for (int i = 0; i < shadow.width; i++)
  {
   for (int j = 0; j < shadow.height; j++)
   {
    shadow.SetPixel(i, j, opaque);
   }
  }

   //テクスチャ初期化
   GameObject obj = GameObject.Find("perfectsphere");
   Material[] mats = obj.GetComponent<Renderer>().materials;
   mats[1].SetTexture("_MainTex", shadow);
/*
  for (int i = 0; i < mats.Length; i++)
   {
    Debug.Log("passed " + mats[i].name + "--");
    if (mats[i].name.IndexOf("perfectsphere") != -1)
     {
      material = mats[i];
      mats[i].SetTexture("_MainTex", shadow);
      mats[i].SetTexture("_EmissionMap", shadow);
      Debug.Log("passed setexture");
     }
   }
*/
  //updateScreen2();
  updateSunAndMoonPosition();
  Debug.Log("persistentChachePath=" + Application.persistentDataPath);
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
     return;
    }

    painted = false;
    //updateSunAndMoonPosition();
    //updateScreen2();
    drawLines();
   // shadow.Apply();
    //        material.SetTexture("_MainTex", shadow);
    interval = 0.0;
   }
  }
 }

 public UmbralOutline()// throws IOException
 {
  /*
      earth = ImageIO.read(new File("C:\\Users\\Kentaro\\Documents\\java\\astronomy\\map.png"));
      imageWidth = earth.getWidth();
      imageHeight = earth.getHeight();
      screen2 = new BufferedImage(800, 600, BufferedImage.TYPE_INT_RGB);
      scrWidth = 800;
      scrHeight = 600;
      screen = new BufferedImage(800, 600, BufferedImage.TYPE_INT_RGB);
      initCameraVector(100, 60);

      targettime = starttime;
      finish = endtime;
      tz = TimeZone.getTimeZone("UTC");
      painted = false;

      window = new JFrame("Umbral outline");
      window.getContentPane().add(this);
      window.addWindowListener(this);
      window.setSize(800, 600);
      window.setVisible(true);
  */
  //createScreen2();//球体地図を生成する。
 }

 /*
   //球体版のカメラの変換行列を設定する。
   private void initCameraVector(double longitude, double latitude)
   {
     viewpoint = new double[3];
     double[] xaxis = new double[3];
     double[] position = new double[3];

     double phai = longitude / 180.0 * Math.PI;
     double rho = latitude / 180.0 * Math.PI;

     //緯度に基づいて回転させる
     position[0] = Altitude * Math.Cos(rho);
     position[1] = 0.0;
     position[2] = Altitude * Math.Sin(rho);

     //Z軸に基づいて回転させる。
     double[][] Zrevolve = new double[][] { { Math.Cos(phai), Math.Sin(phai), 0.0 }, { -Math.Sin(phai), Math.Cos(phai), 0.0 }, { 0.0, 0.0, 1.0 } };
     Matrix.multiplication31type2(Zrevolve, position, viewpoint);

     //接線ベクトルを計算する
     position[0] = 0.0;
     position[1] = 1.0;
     position[2] = 0.0;

     //    double[][] Xrevolve = new double[][]{{1.0, 0.0, 0.0 }, { 0.0, Math.cos(rho), -Math.sin(rho) }, {0.0 Math.sin(rho), Math. cos(rho) }};

     Matrix.multiplication31type2(Zrevolve, position, xaxis);

     double norm = Math.Sqrt(viewpoint[0] * viewpoint[0] + viewpoint[1] * viewpoint[1] + viewpoint[2] * viewpoint[2]);
     double[] yaxis = new double[] { -(viewpoint[0] / norm), -(viewpoint[1] / norm), -(viewpoint[2] / norm) };

     double[] zaxis = new double[3];
     Matrix.getOuterProduct(xaxis, yaxis, zaxis);

     norm = Math.Sqrt(zaxis[0] * zaxis[0] + zaxis[1] * zaxis[1] + zaxis[2] * zaxis[2]);
     zaxis[0] /= norm;
     zaxis[1] /= norm;
     zaxis[2] /= norm;


     camvec = new double[][] { xaxis, yaxis, zaxis };
     InverseCam = new double[3][3];
     Matrix.getInverseMatrixType2(camvec, InverseCam);
   }
 */

 //画面を更新する(球体版)
 /*
   private void createScreen2()
    {
     double hinode_keido, hinoiri_keido, asayake, higure;
     int hinoiriX = 0, hinodeX = 0;
     int asayakeX = 0, higureX = 0;

     //    if (screen2 == null | earth == null) return;

     //イメージを初期化
     Color opaque = new Color(0, 0, 0, 0);
     for (int i = 0; i < shadow.width; i++)
      {
       for (int j = 0; j < shadow.height; j++)
        {
         shadow.SetPixel(i, j, opaque);
        }
      }

     double scrDist = (scrWidth / 2.0) / Math.Tan(scrAngle / 180.0 * Math.PI);
     double[] vector = new double[3];
     double[] result = new double[3];

     double[] center = new double[] { viewpoint[0], viewpoint[1], viewpoint[2] }; //地球中心へ向かうベクトル
     double norm = Math.Sqrt(center[0] * center[0] + center[1] * center[1] + center[2] * center[2]);
     center[0] /= norm;
     center[1] /= norm;
     center[2] /= norm;

     double altitude = Math.Sqrt(viewpoint[0] * viewpoint[0] + viewpoint[1] * viewpoint[1] + viewpoint[2] * viewpoint[2]);

     for (int i = 0; i < scrWidth; i++)
     {
       double x = i - (scrWidth / 2);
       double y = scrDist;
       for (int j = 0; j < scrHeight; j++)
       {
         vector[0] = x;
         vector[1] = y;
         vector[2] = -(j - (scrHeight / 2));
         norm = Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
         vector[0] /= norm;
         vector[1] /= norm;
         vector[2] /= norm;
         Matrix.multiplication31type2(camvec, vector, result);

         double theta = Math.Acos(-center[0] * result[0] - center[1] * result[1] - center[2] * result[2]);

         double b2 = 2.0 * altitude * Math.Cos(theta); //コサインを変換する必要ないのでは?
         double D = b2 * b2 - 4.0 * (altitude * altitude - Constants.de * Constants.de);

         if (D >= 0)
         {
           double root = 0.5 * Math.Sqrt(D);

           double distance = altitude * Math.Cos(theta) - root;
           if (distance < 0) {  System.out.println("負の実数解"); return; }

           //x1は交点までの距離。方向ベクトルに距離を掛けると位置が出る
           double pointX = viewpoint[0] + result[0] * distance;
           double pointY = viewpoint[1] + result[1] * distance;
           double pointZ = viewpoint[2] + result[2] * distance;

           double latitude = Math.Asin(pointZ / Constants.de) / Math.PI * 180.0;
           double longitude = Math.Acos(pointX / Math.Sqrt(pointX * pointX + pointY * pointY)) / Math.PI * 180.0;
           if (pointY < 0) longitude = -longitude; //象限を考慮する

           double xOnMap = longitude - longitudeleft;
           if (xOnMap < 0.0) xOnMap += 360.0;
           int x1 = (int)(xOnMap * (imageWidth / 360.0));

           double yOnMap = (imageHeight / 180.0) * latitude;
           int y1 = Yequator - (int)yOnMap;
           if (x1 < earth.getWidth() & y1 < earth.getHeight()) screen2.setRGB(i, j, earth.getRGB(x1, y1));
         }
       }
     }
   }
 */

  //3D空間内の地用と月のモデルの位置を変更する
  private void updateSunAndMoonPosition()
   {
　　EquatorialCoordinate sun = new EquatorialCoordinate();
    EquatorialCoordinate moon = new EquatorialCoordinate();

    double asc = 0.0;
    double dec = 0.0;
    double moonasc = 0.0;
    double moondec = 0.0;

    try
     {
      double[]result = new double[2];
      SunAndMoon.getSunRightAscension(utc, result);
      asc = result[0] ;//赤経
      dec = result[1];//赤緯

      SunAndMoon.getMoonRightAscension(utc, result);
      moonasc = result[0];
      moondec = result[1];
     }
    catch (Exception) { Debug.Log("Exception1 "); return; }

    double phai0 = Almanac.getGreenidgeSiderealTime(utc);//グリニッジ恒星時
                                                       //恒星時をもとに、背景の回転を行う（恒星時は春分点の時角)
    Material skybox = RenderSettings.skybox;
    skybox.SetFloat("_Rotation", (float)-phai0);//時角のマイナス方向に回転。skyboxのマテリアルは左右が逆
    //太陽位置
     {
     //赤緯・赤経は北極から見て時計回り。自覚、恒星時は反時計回り。時角に合わせて計算する
      //float ramda = -(float)((-asc + phai0) * DegToRad);これを書き換えて下の式になる
      float ramda = (float)((asc - phai0) * DegToRad);
      float psy = (float)(dec * DegToRad);
      float sundistance = 400;
      float x = Mathf.Cos(psy) * Mathf.Cos(ramda) * sundistance;
      float y = Mathf.Cos(psy) * Mathf.Sin(ramda) * sundistance;
      float z = Mathf.Sin(psy) * sundistance;

      GameObject light = GameObject.Find("Directional Light");

      Vector3 sunpos = light.transform.position;
      sunpos.Set(x, z, y);
      light.transform.position = sunpos;
      sunpos.Normalize();
      sunpos *= -1;
      light.transform.forward = sunpos;
     }
    //月位置
     {
      float ramda = (float)((moonasc - phai0) * DegToRad);
      float psy = (float)(moondec * DegToRad);
      float x = Mathf.Cos(psy) * Mathf.Cos(ramda) * distanceToMoon;
      float y = Mathf.Cos(psy) * Mathf.Sin(ramda) * distanceToMoon;
      float z = Mathf.Sin(psy) * distanceToMoon;

      GameObject light = GameObject.Find("Moon");

      Vector3 moonpos = light.transform.position;
      moonpos.Set(x, z, y);
      light.transform.position = moonpos;
     }
   }


  public void drawLines()
   {
    //テクスチャ初期化
    Color opaque = new Color(0, 0, 0, 0);
    for (int i = 0; i < shadow.width; i++)
     {
      for (int j = 0; j < shadow.height; j++)
       {
        shadow.SetPixel(i, j, opaque);
       }
     }

    EquatorialCoordinate sun = new EquatorialCoordinate();
    EquatorialCoordinate moon = new EquatorialCoordinate();

    double[] result = new double[3];
    double[] result2 = new double[3];
    double[] location = new double[2];
    double asc = 0.0;
    double dec = 0.0;
    double moonasc = 0.0;
    double moondec = 0.0;

    try
     {
      SunAndMoon.getSunRightAscension(utc, result);
      sun.setRightAscension(result[0]);
      sun.setCelestialDeclination(result[1]);
      sun.setDistance(result[2]);
      asc = result[0];//赤経
      dec = result[1];//赤緯

      SunAndMoon.getMoonRightAscension(utc, result);
      moon.setRightAscension(result[0]);
      moon.setCelestialDeclination(result[1]);
      moon.setDistance(result[2]);
      moonasc = result[0];
      moondec = result[1];
     }
    catch (Exception) { Debug.Log("Exception1 "); return; }

    double phai0 = Almanac.getGreenidgeSiderealTime(utc);//グリニッジ恒星時

    //太陽位置を計算してライトの位置と向きを変更する
     {
      //恒星時をもとに、背景の回転を行う（恒星時は春分点の時角)
      Material skybox = RenderSettings.skybox;
      skybox.SetFloat("_Rotation", (float)-phai0);//時角のマイナス方向に回転。skyboxのマテリアルは左右が逆

      //赤緯・赤経は北極から見て時計回り。自覚、恒星時は反時計回り。時角に合わせて計算する
      //float ramda = -(float)((-asc + phai0) * DegToRad);これを書き換えて下の式になる
      float ramda = (float)((asc - phai0) * DegToRad);
      float psy = (float)(dec * DegToRad);
      float sundistance = 400;
      float x = Mathf.Cos(psy) * Mathf.Cos(ramda) * sundistance;
      float y = Mathf.Cos(psy) * Mathf.Sin(ramda) * sundistance;
      float z = Mathf.Sin(psy) * sundistance;

      GameObject light = GameObject.Find("Directional Light");
      Vector3 sunpos = light.transform.position;
      sunpos.Set(x, z, y);
      light.transform.position = sunpos;
      sunpos.Normalize();
      sunpos *= -1;
      light.transform.forward = sunpos;
     }
  /*
      //日の出･日の入りの同時線を描く
       {
        double dist = sun.getDistance();
        double parallax = SunAndMoon.getSunParallax(dist);//太陽視差
        double k = SunAndMoon.getSunriseAltitude(SunAndMoon.getSunDiameter(dist), 0.0, SunAndMoon.refraction, parallax);
        double celestialdeclination = sun.getCelestialDeclination();

        for (int i = -90; i < 90; i++)
         {
          //緯度を取得
          double latitude = i;//getLatitudeFromY(Yequator - i);

          //緯度を元に時角を計算する
          double jikaku = SunAndMoon.getTimeAngle(k, celestialdeclination, latitude);

          if (!Double.IsNaN(jikaku))//時角がNaNでない
           {
            double hinode_keido = SunAndMoon.reviseAngle(-jikaku + sun.getRightAscension() - phai0);
            double hinoiri_keido = SunAndMoon.reviseAngle(jikaku + sun.getRightAscension() - phai0);
            //   hinodeX =(int)getXfromLongitude(hinode_keido);
            //   hinoiriX = (int)getXfromLongitude(hinoiri_keido);//昼側か調べる
            drawShadowTexture(hinode_keido, latitude, Color.white);
            drawShadowTexture(hinoiri_keido, latitude, Color.white);
           }
         }
       }
  */
    //輪郭の描画
    //ベッセル数
    VesselElements ve = new VesselElements(sun, moon, utc);
    SolarEclipse.getCrossPoint(ve, result, result2);
    //月影と地球外円との交点を調べる
    double maxQ = SolarEclipse.getPenumbralQ(ve, result);
    double minQ = SolarEclipse.getPenumbralQ(ve, result2);
    //Debug.Log("MaxQ = " + maxQ + " minQ = " + minQ);

    //半影の描画
    if (Double.IsNaN(maxQ) && Double.IsNaN(minQ))
     {
      //交点が存在しない場合。月影がすべて地球上に投射されている。

      double first_longitude = Double.NaN;
      double first_latitude = Double.NaN;
      double last_longitude ;
      double last_latitude ;
      Color gray = new Color( 0, 0, 0, 0.4f);

      //初期の点
      SolarEclipse.getPenumbralOutline(ve, 0.0, result);
      Coordinate.transformICRStoGCS(ve, result, location);
      last_longitude = location[0];
      last_latitude = location[1];
      bool fill = true;
      for (double i = 1.0; i <= 360.0; i += 1.0)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result2);
        if (Double.IsNaN(result2[0]) | Double.IsNaN(result2[1]) | Double.IsNaN(result2[2]))
         {
          fill = false;
          continue;//NaNが含まれていたらスキップする
         }
        Coordinate.transformICRStoGCS(ve, result2, location);
//         drawShadowTexture(location[0], location[1], gray);
    //Debug.Log("i=" + i + ":" +location[0] + ":" + location[1]);
        drawOutline(last_longitude, last_latitude, location[0], location[1], Color.red);   
        last_longitude = location[0];
        last_latitude = location[1];
       }

     if (fill)
      {
       fillShadow(last_longitude - 2, last_latitude, shadow, gray);
      }
   /*
        byte[] pngData = shadow.EncodeToPNG();   // pngのバイト情報を取得.

        // ファイルダイアログの表示.
        string filePath = EditorUtility.SaveFilePanel("Save Texture", "", shadow.name + ".png", "png");

        if (filePath.Length > 0)
         {
          // pngファイル保存.
          File.WriteAllBytes(filePath, pngData);
          }
   */
   //Debug.Log(first_longitude + ":" + first_latitude + "::" + last_longitude + ":" + last_latitude);
  }
     else if (!Double.IsNaN(maxQ) && !Double.IsNaN(minQ))//交点が存在する
      {
       double first_x = double.NaN;
       double first_y = double.NaN;
       double first_z = double.NaN;
       double last_x = double.NaN;
       double last_y = double.NaN;
       double last_z = double.NaN;
   Color gray = new Color(0, 0, 0, 0.4f);
   if ((maxQ - minQ) >= 0.0) maxQ -= 360.0;
       SolarEclipse.getPenumbralOutline(ve, maxQ, result);
       Coordinate.transformICRStoGCS(ve, result, location);
       //Debug.Log("MaxQ :" + location[0] + ":" + location[1]+ ":" + maxQ);
       SolarEclipse.getPenumbralOutline(ve, minQ, result);
       Coordinate.transformICRStoGCS(ve, result, location);
      //Debug.Log("MinQ :" + location[0] + ":" + location[1] + ":" + minQ);
   /*
         //maxQが通常の計算でNaNとなる場合に備えて、強制的に描画する。
         SolarEclipse.getPenumbralOutline(ve, maxQ, result);
         result[2] = -0.01;//強制的に基準面に設定する
         Coordinate.transformICRStoGCS(ve, result, location);
         drawShadowTexture(location[0], location[1], Color.black);
   */
      for (double i = maxQ /*Math.Ceiling(maxQ)*/; i < minQ; i += 0.2)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする

        if (Double.IsNaN(first_x) | Double.IsNaN(first_y) | Double.IsNaN(first_z))
         {
          first_x = result[0];
          first_y = result[1];
          first_z = result[2];
         }
        last_x = result[0];
        last_y = result[1];
        last_z = result[2];

        Coordinate.transformICRStoGCS(ve, result, location);
        drawShadowTexture(location[0], location[1],gray);
       }

       {
        SolarEclipse.getPenumbralOutline(ve, minQ, result);
        if (!Double.IsNaN(result[0]) & !Double.IsNaN(result[1]) & !Double.IsNaN(result[2]))
         {
          Coordinate.transformICRStoGCS(ve, result, location);
          last_x = result[0];
          last_y = result[2];
          last_z = result[1];

          drawShadowTexture(location[0], location[1], gray);
         }
       }
     }

  //本影の描画
    for (int i = 0; i <= 360; i += 5)
     {
      SolarEclipse.getUmbralOutline(ve, (double)i, result);
      if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする

      Coordinate.transformICRStoGCS(ve, result, location);
      drawShadowTexture(location[0], location[1], Color.black);
     }
  //
    shadow.Apply();
   }


  //画面を更新する(球体地図)
  public void updateScreen2()
   {
    double hinode_keido, hinoiri_keido, asayake, higure;
    int hinoiriX = 0, hinodeX = 0;
    int asayakeX = 0, higureX = 0;
    double x, y;
    double halfPI = Math.PI / 2.0;

    //double scrDist = (scrWidth / 2.0) / Math.Tan(scrAngle / 180.0 * Math.PI);

    //if (screen == null | screen2 == null) return;

    //イメージを初期化

    Color opaque = new Color(0, 0, 0, 0);
    for (int i = 0; i < shadow.width; i++)
     {
      for (int j = 0; j < shadow.height; j++)
       {
        shadow.SetPixel(i, j, opaque);
       }
     }

    EquatorialCoordinate sun = new EquatorialCoordinate();
    EquatorialCoordinate moon = new EquatorialCoordinate();

    double[] result = new double[3];
    double[] result2 = new double[3];
    double[] location = new double[2];
    double asc = 0.0;
    double dec = 0.0;
    double moonasc = 0.0;
    double moondec = 0.0;

    try
    {
      SunAndMoon.getSunRightAscension(utc, result);
      sun.setRightAscension(result[0]);
      sun.setCelestialDeclination(result[1]);
      sun.setDistance(result[2]);
      asc = result[0];//赤経
      dec = result[1];//赤緯

      SunAndMoon.getMoonRightAscension(utc, result);
      moon.setRightAscension(result[0]);
      moon.setCelestialDeclination(result[1]);
      moon.setDistance(result[2]);
      moonasc = result[0];
      moondec = result[1];
     }
    catch (Exception ) { Debug.Log("Exception1 ");  return; }

    double phai0 = Almanac.getGreenidgeSiderealTime(utc);//グリニッジ恒星時
    //恒星時をもとに、背景の回転を行う（恒星時は春分点の時角)
    Material skybox = RenderSettings.skybox;
    skybox.SetFloat("_Rotation", (float)-phai0);//時角のマイナス方向に回転。skyboxのマテリアルは左右が逆

    //太陽位置計算(orbiterと同じコード)
    {
      double Theta = phai0 - asc;
      if (Theta < 0) Theta += 360.0;
      double DegToRad = Math.PI / 180;
      double denominator = (Math.Sin(dec * DegToRad) * Math.Cos(90.0 * DegToRad) - Math.Cos(dec * DegToRad) * Math.Sin(90.0 * DegToRad) * Math.Cos(Theta * DegToRad));
      double A = Math.Atan((-Math.Cos(dec * DegToRad) * Math.Sin(Theta * DegToRad)) / denominator);
      double h = Math.Asin(Math.Sin(dec * DegToRad) * Math.Sin(90.0 * DegToRad) + Math.Cos(dec * DegToRad) * Math.Cos(90.0 * DegToRad) * Math.Cos(Theta * DegToRad));
      A = A / DegToRad;
      h = h / DegToRad;
      //Arctanの象限を検討せよ
      if (denominator > 0)
       {
        //何故か解説書とは逆だが、分母が正の時に１８０度加算して象限を変える必要がある
        A += 180.0;
       }
      Vector3 sunvector = new Vector3(1.0f, 0.0f, 0.0f);
      sunvector = Quaternion.Euler(0.0f, 0.0f, (float)h) * sunvector;
      sunvector = Quaternion.Euler(0.0f, (float)A, 0.0f) * sunvector;

    
      float ratio = 1500.0f / sunvector.magnitude;
      sunvector *= ratio;

      GameObject game = GameObject.Find("Directional Light");
      if (game != null)
      {
        game.transform.position = sunvector;
        Vector3 forward = sunvector;
        forward.Normalize();
        game.transform.forward = -forward;
      }
    }
    //日の出･日の入りの同時線を描く

    double dist = sun.getDistance();
    double parallax = SunAndMoon.getSunParallax(dist);//太陽視差
    double k = SunAndMoon.getSunriseAltitude(SunAndMoon.getSunDiameter(dist), 0.0, SunAndMoon.refraction, parallax);
    double celestialdeclination = sun.getCelestialDeclination();

    for (int i = -90; i < 90; i++)
     {
      //緯度を取得
      double latitude = i;//getLatitudeFromY(Yequator - i);

      //緯度を元に時角を計算する
      double jikaku = SunAndMoon.getTimeAngle(k, celestialdeclination, latitude);

      if (!Double.IsNaN(jikaku))//時角がNaNでない
       {
            hinode_keido = SunAndMoon.reviseAngle(-jikaku + sun.getRightAscension() - phai0);
            hinoiri_keido = SunAndMoon.reviseAngle(jikaku + sun.getRightAscension() - phai0);
        //   hinodeX =(int)getXfromLongitude(hinode_keido);
        //   hinoiriX = (int)getXfromLongitude(hinoiri_keido);//昼側か調べる
        drawShadowTexture(hinode_keido, latitude, Color.white);
        drawShadowTexture(hinoiri_keido, latitude, Color.white);
          }
        }

    //輪郭の描画
    VesselElements ve = new VesselElements(sun, moon, utc);
    SolarEclipse.getCrossPoint(ve, result, result2);
    double maxQ = SolarEclipse.getPenumbralQ(ve, result);
    double minQ = SolarEclipse.getPenumbralQ(ve, result2);
    //Debug.Log("MaxQ = " + maxQ + " minQ = " + minQ);

    //月位置計算(orbiterと同じコード)
    {
      double Theta = phai0 - moonasc;
      if (Theta < 0) Theta += 360.0;
      double DegToRad = Math.PI / 180;
      double denominator = (Math.Sin(moondec * DegToRad) * Math.Cos(90.0 * DegToRad) - Math.Cos(moondec * DegToRad) * Math.Sin(90.0 * DegToRad) * Math.Cos(Theta * DegToRad));
      double A = Math.Atan((-Math.Cos(moondec * DegToRad) * Math.Sin(Theta * DegToRad)) / denominator);
      double h = Math.Asin(Math.Sin(moondec * DegToRad) * Math.Sin(90.0 * DegToRad) + Math.Cos(moondec * DegToRad) * Math.Cos(90.0 * DegToRad) * Math.Cos(Theta * DegToRad));
      A = A / DegToRad;
      h = h / DegToRad;
      //Arctanの象限を検討せよ
      if (denominator > 0)
      {
        //何故か解説書とは逆だが、分母が正の時に１８０度加算して象限を変える必要がある
        A += 180.0;
      }
      Vector3 sunvector = new Vector3(1.0f, 0.0f, 0.0f);
      sunvector = Quaternion.Euler(0.0f, 0.0f, (float)h) * sunvector;
      sunvector = Quaternion.Euler(0.0f, (float)A, 0.0f) * sunvector;
      float ratio = (float)(moon.getDistance() * Constants.AUde * 3.16f) / sunvector.magnitude;
      sunvector *= ratio;

   //
 
   //      Debug.Log("moondist = " + moon.getDistance());
   GameObject game = GameObject.Find("Moon");
      if (game != null)
      {
        game.transform.position = sunvector;
      }
    }
    /*  Vector3 moonPos = new Vector3((float)ve.getX0(), (float)ve.getY0(), (float)ve.getZ0());
        moonPos.Normalize();
        moonPos *= 20;
        GameObject moonobj = GameObject.Find("Moon");

        if (moonobj!= null)
         {
          moonobj.transform.position = moonPos;
         }
    */
    //半影の描画
    if (Double.IsNaN(maxQ) && Double.IsNaN(minQ))
     {
   double first_longitude = Double.NaN;
      double first_latitude = Double.NaN;
      double last_longitude = Double.NaN;
      double last_latitude = Double.NaN;

      for (double i = 0.0; i <= 360.0; i += 0.2)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする
        if (first_longitude == double.NaN | first_latitude == double.NaN )
         {
          first_longitude = result[0];
          first_latitude = result[1];
         }
        last_longitude = result[0];
        last_latitude = result[1];

        Coordinate.transformICRStoGCS(ve, result, location);
        drawShadowTexture(location[0], location[1], Color.red);
       }
      //Debug.Log(first_longitude + ":" + first_latitude + "::" + last_longitude + ":" + last_latitude);
    }
    else if (!Double.IsNaN(maxQ) && !Double.IsNaN(minQ))
    {
      double first_x = double.NaN;
      double first_y = double.NaN;
      double first_z = double.NaN;
      double last_x = double.NaN;
      double last_y = double.NaN;
      double last_z = double.NaN;

      if ((maxQ - minQ) >= 0.0) maxQ -= 360.0;
      SolarEclipse.getPenumbralOutline(ve, maxQ, result);
      Coordinate.transformICRStoGCS(ve, result, location);
      //Debug.Log("MaxQ :" + location[0] + ":" + location[1]+ ":" + maxQ);
      SolarEclipse.getPenumbralOutline(ve, minQ, result);
      Coordinate.transformICRStoGCS(ve, result, location);
      //Debug.Log("MinQ :" + location[0] + ":" + location[1] + ":" + minQ);
      /*
            //maxQが通常の計算でNaNとなる場合に備えて、強制的に描画する。
            SolarEclipse.getPenumbralOutline(ve, maxQ, result);
            result[2] = -0.01;//強制的に基準面に設定する
            Coordinate.transformICRStoGCS(ve, result, location);
            drawShadowTexture(location[0], location[1], Color.black);
      */
      for (double i = maxQ /*Math.Ceiling(maxQ)*/; i < minQ; i += 0.2)
       {
        SolarEclipse.getPenumbralOutline(ve, i, result);
        if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする

        if (Double.IsNaN(first_x) | Double.IsNaN(first_y) | Double.IsNaN(first_z))
        {
          first_x = result[0];
          first_y = result[1];
          first_z = result[2];
        }
        last_x = result[0];
        last_y = result[1];
        last_z = result[2];

        Coordinate.transformICRStoGCS(ve, result, location);

        drawShadowTexture(location[0], location[1], Color.red);
       }
       {
        SolarEclipse.getPenumbralOutline(ve, minQ, result);
        if (!Double.IsNaN(result[0]) & !Double.IsNaN(result[1]) & !Double.IsNaN(result[2]))
         {
          Coordinate.transformICRStoGCS(ve, result, location);
          last_x = result[0];
          last_y = result[2];
          last_z = result[1];
          drawShadowTexture(location[0], location[1], Color.red);
         }
      }
      //drawClosingLine2(ve, first_x, first_y, first_z, last_x, last_y, last_z);
      //Debug.Log(first_longitude + ":" + first_latitude + "::" + last_longitude + ":" + last_latitude);

      /*
            //minQが通常の計算でNaNとなる場合に備えて、強制的に描画する。
            SolarEclipse.getPenumbralOutline(ve, minQ, result);
            result[2] = -0.01;//強制的に基準面に設定する
            Coordinate.transformICRStoGCS(ve, result, location);
            drawShadowTexture(location[0], location[1], Color.red);
      */
    }

    //本影の描画
    for (int i = 0; i <= 360; i += 5)
     {
   SolarEclipse.getUmbralOutline(ve, (double)i, result);
      if (Double.IsNaN(result[0]) | Double.IsNaN(result[1]) | Double.IsNaN(result[2])) continue;//NaNが含まれていたらスキップする
     
      Coordinate.transformICRStoGCS(ve, result, location);
      drawShadowTexture(location[0], location[1], Color.black);
    }
   GameObject earthobj =GameObject.Find("perfectsphere");
   Material[] mats= earthobj.GetComponent<Renderer>().materials;
  Debug.Log("elements =" + mats.Length);
  mats[0].SetTexture("_MainTex",(Texture)shadow);
  //mats[1].SetTexture("_EmissionMap", shadow);
  //repaint();
 }

  private void drawOutline(double last_longitude, double last_latitude, double longitude, double latitude, Color color)
   {
    int x1 = (int)(512 * last_longitude / 180.0);
    int y1 = 512 + (int)(512 * last_latitude / 90.0);
    if (x1 < 0) x1 += 1024;

    if (y1 < 0) y1 = 0;
    else if (y1 > 1024) y1 = 1024;

    if (x1 < 0) x1 = 0;
    else if (x1 > 1024) x1 = 1024;

    int x2 = (int)(512 * longitude / 180.0);
    int y2 = 512 + (int)(512 * latitude / 90.0);
    if (x2 < 0) x2 += 1024;

    if (y2 < 0) y2 = 0;
    else if (y2 > 1024) y2 = 1024;

    if (x2 < 0) x2 = 0;
    else if (x2 > 1024) x2 = 1024;

    int[] linepoints = Brezengham.line(x1, y1, x2, y2);
    for(int i = 0; i< linepoints.Length / 2; i++) shadow.SetPixel(linepoints[i* 2], linepoints[i* 2 + 1], color);
   }

 private void drawShadowTexture(double longitude, double latitude, Color color)
   {
    int x = (int)(512 * longitude / 180.0);
    int y = 512 + (int)(512 * latitude / 90.0) ;
    if (x < 0) x += 1024;

    if (y < 0) y = 0;
    else if (y > 1024) y = 1024;

    if (x < 0) x = 0;
    else if (x > 1024) x = 1024;

    shadow.SetPixel(x, y, color); 
   }

  private void fillShadow(double longitude, double latitude, Texture2D tex, Color color)
   {
    int x = (int)(512 * longitude / 180.0);
    int y = 512 + (int)(512 * latitude / 90.0);
    if (x < 0) x += 1024;

    if (y < 0) y = 0;
    else if (y > 1024) y = 1024;

    if (x < 0) x = 0;
    else if (x > 1024) x = 1024;

    LineSeed.paint(x, y, tex, color);
   }


 private void drawClosingLine(double first_longitude, double first_latitude, double last_longitude, double last_latitude)
   {
    double xstep = (last_longitude - first_longitude) / 20;
    double ystep = (last_latitude - first_latitude) / 20;

    for(int i = 0; i < 20; i++)
     {
      drawShadowTexture(first_longitude + xstep * i, first_latitude + ystep * i, Color.red);
     }
   }

  private void drawClosingLine2(VesselElements ve, double firstx, double firsty, double firstz, double lastx, double lasty, double lastz)
   {
    Vector3 First = new Vector3((float)firstx, (float)firsty,(float)firstz);
    Vector3 Last = new Vector3((float)lastx, (float)lasty, (float)lastz);
    double[] result = new double[3];
    double[] location = new double[3];

    for (float i = 0.0f; i <= 1.0f; i += 0.1f) 
     {
      Vector3 point = Vector3.Slerp(First, Last, i);
      point.Normalize();
      result[0] = point.x;
      result[1] = point.y;
      result[2] = point.z;
      Coordinate.transformICRStoGCS(ve, result, location);
     if (i == 0.5f)
      {
        Debug.Log("First=" + firstx + ":" + firsty + ":" + firstz);
        Debug.Log("Last=" + lastx + ":" + lasty + ":" + lastz);
        Debug.Log("result=" + result[0] + ":" + result[1] + ":" + result[2]);
        Debug.Log("result=" + location[0] + ":" + location[1] );
      }
      drawShadowTexture(location[0], location[1], Color.red);
    }
  }
  /*
    //画面を更新する
    public void updateScreen()
    {
      double hinode_keido, hinoiri_keido, asayake, higure;
      int hinoiriX = 0, hinodeX = 0;
      int asayakeX = 0, higureX = 0;
      double x, y;

      if (screen == null | earth == null) return;

      //イメージを初期化
      for (int i = 0; i < imageWidth; i++)
      {
        for (int j = 0; j < imageHeight; j++)
        {
          screen.setRGB(i, j, earth.getRGB(i, j));
        }
      }

      Graphics imgG = screen.getGraphics();
      EquatorialCoordinate sun = new EquatorialCoordinate();
      EquatorialCoordinate moon = new EquatorialCoordinate();


      double[] result = new double[3];
      double[] result2 = new double[3];
      double[] location = new double[2];

      try
      {
        StarPosition.getSunRightAscension(targettime, result);

        sun.setRightAscension(result[0]);
        sun.setCelestialDeclination(result[1]);
        sun.setDistance(result[2]);

        StarPosition.getMoonRightAscension(targettime, result);
        moon.setRightAscension(result[0]);
        moon.setCelestialDeclination(result[1]);
        moon.setDistance(result[2]);
      }
      catch (IllegalArgumentException e) { return; }


      //日の出･日の入りの同時線を描く
      for (int i = 0; i < imageHeight; i++)
      {
        //緯度を取得
        double latitude = getLatitudeFromY(Yequator - i);
        double phai0 = Almanac.getGreenidgeSiderealTime(targettime);//グリニッジ恒星時

        double dist = sun.getDistance();
        double parallax = StarPosition.getSunParallax(dist);//太陽視差
        double k = StarPosition.getSunriseAltitude(StarPosition.getSunDiameter(dist), 0.0, StarPosition.refraction, parallax);

        //緯度を元に時角を計算する
        double jikaku = StarPosition.getTimeAngle(k, sun.getCelestialDeclination(), latitude);

        if (!Double.isNaN(jikaku))//時角がNaNでない
        {
          hinode_keido = StarPosition.reviseAngle(-jikaku + sun.getRightAscension() - phai0);
          hinoiri_keido = StarPosition.reviseAngle(jikaku + sun.getRightAscension() - phai0);
          hinodeX = (int)getXfromLongitude(hinode_keido);
          hinoiriX = (int)getXfromLongitude(hinoiri_keido);//昼側か調べる

          if (hinodeX < imageWidth) screen.setRGB(hinodeX, i, 0xff0000);
          if (hinodeX < imageWidth) screen.setRGB(hinoiriX, i, 0xff0000);
        }
      }

      //輪郭の描画
      VesselElements ve = new VesselElements(sun, moon, targettime);
      SolarEclipse.getCrossPoint(ve, result, result2);
      double maxQ = SolarEclipse.getPenumbralQ(ve, result);
      double minQ = SolarEclipse.getPenumbralQ(ve, result2);
      int x1 = 0, y1 = 0;
      //半影の描画
      if (Double.isNaN(maxQ) && Double.isNaN(minQ))
      {
        for (double i = 0.0; i <= 360.0; i += 3.0)
        {
          SolarEclipse.getPenumbralOutline(ve, i, result);
          if (Double.isNaN(result[0]) | Double.isNaN(result[1]) | Double.isNaN(result[2])) continue;//NaNが含まれていたらスキップする
          Coordinate.transformICRStoGCS(ve, result, location);

          x = location[0] - longitudeleft;
          if (x < 0.0) x += 360.0;
          int x2 = (int)(x * (imageWidth / 360.0));

          y = (imageHeight / 180.0) * location[1];
          int y2 = Yequator - (int)y;

          //描画
          if (i != 0.0) imgG.drawLine(x1, y1, x2, y2);

          x1 = x2; y1 = y2;
        }
      }

      else
      {
        if ((maxQ - minQ) >= 0.0) maxQ -= 360.0;
        int x2 = 0;
        int y2 = 0;


        SolarEclipse.getPenumbralOutline(ve, maxQ, result);
        result[2] = 0.0;//強制的に基準面に設定する

        Coordinate.transformICRStoGCS(ve, result, location);
        x = location[0] - longitudeleft;
        if (x < 0.0) x += 360.0;
        x1 = (int)(x * (imageWidth / 360.0));

        y = (imageHeight / 180.0) * location[1];
        y1 = Yequator - (int)y;

        for (double i = Math.ceil(maxQ); i < minQ; i += 3.0)
        {
          SolarEclipse.getPenumbralOutline(ve, i, result);
          if (Double.isNaN(result[0]) | Double.isNaN(result[1]) | Double.isNaN(result[2])) continue;//NaNが含まれていたらスキップする
          Coordinate.transformICRStoGCS(ve, result, location);

          x = location[0] - longitudeleft;
          if (x < 0.0) x += 360.0;
          x2 = (int)(x * (imageWidth / 360.0));

          y = (imageHeight / 180.0) * location[1];
          y2 = Yequator - (int)y;

          //描画
          imgG.drawLine(x1, y1, x2, y2);

          x1 = x2; y1 = y2;
        }

        SolarEclipse.getPenumbralOutline(ve, minQ, result);
        result[2] = 0.0;//強制的に基準面に設定する

        Coordinate.transformICRStoGCS(ve, result, location);
        x = location[0] - longitudeleft;
        if (x < 0.0) x += 360.0;
        x2 = (int)(x * (imageWidth / 360.0));

        y = (imageHeight / 180.0) * location[1];
        y2 = Yequator - (int)y;
        imgG.drawLine(x1, y1, x2, y2);
      }

      //本影の描画
      imgG.setColor(Color.BLACK);

      SolarEclipse.getUmbralOutline(ve, 0.0, result);
      Coordinate.transformICRStoGCS(ve, result, location);

      x = location[0] - longitudeleft;
      if (x < 0.0) x += 360.0;
      x1 = (int)(x * (imageWidth / 360.0));

      y = (imageHeight / 180.0) * location[1];
      y1 = Yequator - (int)y;

      for (int i = 60; i <= 360; i += 60)
      {
        SolarEclipse.getUmbralOutline(ve, (double)i, result);
        if (Double.isNaN(result[0]) | Double.isNaN(result[1]) | Double.isNaN(result[2])) continue;//NaNが含まれていたらスキップする

        Coordinate.transformICRStoGCS(ve, result, location);
        //      System.out.println("location[0] " + location[0] + " location[1] =" +location[1] ) ;

        x = location[0] - longitudeleft;
        if (x < 0.0) x += 360.0;
        int x2 = (int)(x * (imageWidth / 360.0));

        y = (imageHeight / 180.0) * location[1];
        int y2 = Yequator - (int)y;

        imgG.drawLine(x1, y1, x2, y2);
        x1 = x2; y1 = y2;
      }

      repaint();
    }


    public void paint(Graphics g)
    {
      g.drawImage(screen, 0, 0, null);
      painted = true;
    }
  */
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
  public void EclipseCalender(int number)
   {
    
    //    utc.set(2012,  4, 21,  3,  0, 0);
    //    utc.set(2012, 10, 14,  1,  0, 0);
    //    utc.set(2013,  4,  10,  1, 30, 0);
    //      utc.set(2013, 10,  3, 14, 50, 0);
    //      utc.set(2014,  3, 29, 8, 0, 0);
    //      utc.set(2016,  2,  9, 4, 0, 0);
    //      utc.set(2016,  8,  1, 11, 30, 0);

    //   utc.set(2018,  7, 11, 11, 30, 0);
    //    utc.set(2019,  0,  6,  3, 40, 0);
    //    utc.set(2019, 11, 26,  8,  0, 0);
    //     utc.set(2020,  5, 21,  9, 30, 0);
    //      utc.set(2028,  6, 22, 6 , 0, 0);
    //    utc.set(2030, 10, 25,  9, 00, 0);
    //    utc.set(2032, 10,  3, 7, 30, 0);

    //    utc.set(2038, 11, 26,  5, 0, 0);


    //    utc.set(2030,  5,  1,  9,  0, 0);
 
    //    utc.set(2012,  4, 20, 21, 30, 0);
    //    utc.set(2012, 10, 13, 20, 30, 0);
    //    utc.set(2013,  4,  9, 21, 00, 0);
    //      utc.set(2013, 10,  3, 10, 10, 0);
    //      utc.set(2014,  3, 29, 3, 0, 0);
    //    utc.set(2016,  2,  9, 0, 00, 0);
    //      utc.set(2016,  8,  1, 7, 0, 0);
    //     utc.set(2018,  7, 11,  9, 00, 0);
    //    utc.set(2019,  0,  5,  23, 40, 0);
    //    utc.set(2019, 11, 26,  3, 40, 0);
    //    utc.set(2020,  5, 21,  3, 40, 0);
    //    utc.set(2028,  6, 22,  0, 30, 0);
    //    utc.set(2030, 10, 25,  4, 30, 0);
    //    utc.set(2032, 10,  3, 3, 0, 0);

    //    utc.set(2038, 11, 25, 23, 30, 0);

    //    utc.set(2030,  5,  1,  5, 30, 0);
    }
  }
