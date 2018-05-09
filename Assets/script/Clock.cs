using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Clock : MonoBehaviour
 {
  private readonly string[] filenames = new string[] { "numero/0_white.png", "numero/1_white.png", "numero/2_white.png", "numero/3_white.png",
   "numero/4_white.png", "numero/5_white.png", "numero/6_white.png", "numero/7_white.png", "numero/8_white.png", "numero/9_white.png", "UI/clock.png"};

  private const int clockX = 20;
  private const int clockY = 20;
  private const int pics = 13;
  private const int baseline = 34;
  private const int clockposindex = 12;
  private const int clockimageindex = 10;
  private readonly int[] characterpos = new int[] {22, 57, 92, 128, 178, 211, 266, 302, 356, 393, 442, 474 };
/*
  private const int chara1 = 22;
  private const int chara2 = 57;
  private const int chara3 = 92;
  private const int chara4 = 128;
  private const int chara5 = 178;
  private const int chara6 = 211;
  private const int chara7 = 266;
  private const int chara8 = 302;
  private const int chara9 = 356;
  private const int chara10 = 393;
  private const int chara11 = 442;
  private const int chara12 = 474;
*/

  private DateTime current;
  private Texture2D[] images;
  private Rect[] imagepositions;
  private bool ready;


		void Start ()
　 {
    ready = false;
    images = new Texture2D[filenames.Length]; 
   
    for (int i = 0; i < filenames.Length; i++)
     {
      StartCoroutine("getTextureAsset", filenames[i]);
     }
	　}

  private void init()
   {
    for (int i = 0; i < images.Length;i++)
     {
      if (images[i] == null) return;
     }

    imagepositions = new Rect[13];
    //時計の位置
    imagepositions[12] = new Rect(clockX, clockY, images[10].width, images[10].height);

    for (int i = 0; i < imagepositions.Length - 1;i++)
     {
      imagepositions[i] = new Rect(clockX + characterpos[i], clockY + baseline, images[0].width, images[0].height);
     }
    ready = true;
   }

  void Update ()
   {
		  if(!ready)
     {
      init(); 
     }
	  }

  void OnGUI()
   {
    if (!ready) return;

    //時計の描画    
    GUI.DrawTexture(imagepositions[clockposindex], images[clockimageindex]);

    if (current == null) return;

    //時刻の描画
    int year = current.Year;
    int quotient = year / 1000;
    GUI.DrawTexture(imagepositions[0], images[quotient]);

    year -= (quotient * 1000);
    quotient = year / 100;
    GUI.DrawTexture(imagepositions[1], images[quotient]);

    year -= (quotient * 100);
    quotient = year / 10;
    GUI.DrawTexture(imagepositions[2], images[quotient]);

    year -= (quotient * 10);
    GUI.DrawTexture(imagepositions[3], images[year]);

    int month = current.Month;
    quotient = month / 10;
    GUI.DrawTexture(imagepositions[4], images[quotient]);

    month -= (quotient * 10);
    GUI.DrawTexture(imagepositions[5], images[month]);

    int day = current.Day;
    quotient = day / 10;
    GUI.DrawTexture(imagepositions[6], images[quotient]);

    day -= (quotient * 10);
    GUI.DrawTexture(imagepositions[7], images[day]);

    int hour = current.Hour;
    quotient = hour / 10;
    if (quotient != 0) GUI.DrawTexture(imagepositions[8], images[quotient]);

    hour -= (quotient * 10);
    GUI.DrawTexture(imagepositions[9], images[hour]);

    int min = current.Minute;
    quotient = min / 10;
    GUI.DrawTexture(imagepositions[10], images[quotient]);

    min -= (quotient * 10);
    GUI.DrawTexture(imagepositions[11], images[min]);
   }


  //時刻を通知
  public void setTime(DateTime time)
   {
    //UTCからJSTへ変換する
    current = time.AddHours(9);
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
      //container.iconimage = www.texture;
      // text = "count =" + www.text.Length.ToString();
     }
   }


  //読み込んだテクスチャを変数に格納する
  private void setTexture(Texture2D tex, string datafilename)
   {
    for (int i = 0; i < filenames.Length;i++)
     {
      if (datafilename == filenames[i])
       {
        images[i] = tex;
        break;
       }
     }
   }
 }
