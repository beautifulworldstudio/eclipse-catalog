using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


[Serializable]
public class EclipseData
 {
  public const int SUN_ASC = 0;
  public const int SUN_DEC = 1;
  public const int SUN_DIST = 2;
  public const int MOON_ASC = 3;
  public const int MOON_DEC = 4;
  public const int MOON_DIST = 5;
  public const int PHAI = 6;

  public float initialCameraLongitude;   //初期カメラ経度
  public float initialCameraLatitude;   //初期カメラ緯度
  public float initialMapLongitude;  //初期地図経度
  public float initialMapLatitude;  //初期地図緯度
  public int interval;         //計算間隔
  public long starttimebinary; //開始時刻(UTC)
  public long finishtimebinary;//終了時刻(UTC)

  public double[] sun_ascension;   //太陽赤経データ
  public double[] sun_declination;   //太陽赤緯データ
  public double[] sun_distance;      //太陽距離データ
  public double[] moon_ascension;   //月赤経データ
  public double[] moon_declination;   //月赤緯データ
  public double[] moon_distance;   //月距離データ
  public double[] phai0;               //グリニッジ恒星時

  [NonSerialized]
  private DateTime starttime;
  [NonSerialized]
  private DateTime finishtime;

  public EclipseData(){ }

  //データ採取時のコンストラクタ
  public EclipseData(DateTime start, DateTime finish, int minutes)
   {
    interval = minutes;

    TimeSpan difference = finish.Subtract(start);
    if (difference.Days > 1) return; //1日以上の差はエラー

    starttimebinary = start.ToBinary();
    finishtimebinary = finish.ToBinary();

    int elements = difference.Hours * 60 + difference.Minutes + 1;

    sun_ascension = new double[elements];
    sun_declination = new double[elements];
    sun_distance = new double[elements];
    moon_ascension = new double[elements];
    moon_declination = new double[elements];
    moon_distance = new double[elements];
    phai0 = new double[elements];

    initDateTime();
   }


  //データを復元した後の初期化
  public bool initDateTime()
   {
    try
     {
      starttime = DateTime.FromBinary(starttimebinary);
      finishtime = DateTime.FromBinary(finishtimebinary);
      //Debug.Log(finishtime.Year + "-" + finishtime.Month + "-" + finishtime.Day + " " + finishtime.Hour + ":" + finishtime.Minute);
      //starttime = new DateTime(start_year, start_month, start_day, start_hour, start_mimute, 0, DateTimeKind.Utc);
      //finishtime = new DateTime(finish_year, finish_month, finish_day, finish_hour, finish_mimute, 0, DateTimeKind.Utc);
     }
    catch (Exception e) { return false; }

    return true;
   }

  public DateTime getStartTime()
   {
    return starttime;
   }

  public DateTime getFinishTime()
   {
    return finishtime;
   }

  public float getInitialCameraLongitude()
   {
    return initialCameraLongitude;
   }

  public float getInitialCameraLatitude()
   {
    return initialCameraLatitude;
   }

  public float getInitialMapLongitude()
   {
    return initialMapLongitude;
   }

  public float getInitialMapLatitude()
   {
    return initialMapLatitude;
   }

  public void setPositions(double sunasc, double sundec, double sundist, double moonasc, double moondec, double moondist, double theta, DateTime time)
   {
    //DateTime start = new DateTime(start_year, start_month, start_day, start_hour, start_mimute, 0);
    TimeSpan span = time.Subtract(starttime);
    int minutes = span.Hours * 60 + span.Minutes;
    sun_ascension[minutes] = sunasc;
    sun_declination[minutes] = sundec;
    sun_distance[minutes] = sundist;
    moon_ascension[minutes] = moonasc;
    moon_declination[minutes] = moondec;
    moon_distance[minutes] = moondist;
    phai0[minutes] = theta;
   }

  public void getPositions(DateTime time, double[] result)
   {
    //DateTime start = new DateTime(start_year, start_month, start_day, start_hour, start_mimute, 0);
    //DateTime finish = new DateTime(finish_year, finish_month, finish_day, finish_hour, finish_mimute, 0);

    TimeSpan span = time.Subtract(starttime);
    int minutes = span.Hours * 60 + span.Minutes;
    span = finishtime.Subtract(starttime);
    int alllength = span.Hours * 60 + span.Minutes + 1;
    if (minutes < 0 | alllength <= minutes) { return; }

    result[0] = sun_ascension[minutes];
    result[1] = sun_declination[minutes];
    result[2] = sun_distance[minutes]; 
    result[3] = moon_ascension[minutes];
    result[4] = moon_declination[minutes];
    result[5] = moon_distance[minutes];
    result[6] = phai0[minutes];
   }
 
  public void writeJSON(string filename)
   {
    // JSONにシリアライズ
    string json = JsonUtility.ToJson(this);
    // Assetsフォルダに保存する
    string path = Application.dataPath + "/eclipsedata/" + filename;

    StreamWriter writer = new StreamWriter(path, false); // 上書き
    writer.WriteLine(json);
    writer.Flush();
    writer.Close();
   }
 }

