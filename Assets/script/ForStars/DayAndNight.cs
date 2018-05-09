using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DayAndNight
 {
  //太陽位置計算関連変数
  private double asc;//
  private double dec;//
  private double phai0;//
  private double dist; //
  private double parallax;//
  private double k;
  private double twilightk;
  private double time; //
  //描画関連
  private Texture2D screen;
  private int wholewidth; //
  private int wholeheight; //
  private Color nightside;
  private int Xbase; //
  private int Yequator; //
  private const int UtcToJST = 9;

  public DayAndNight(Texture2D texture)
   {
    nightside = new Color(0.0f, 0.125f, 0.415f, 0.75f);

    screen = texture;
    wholewidth = screen.width;
    wholeheight = screen.height;
    Yequator = wholeheight / 2;
    Xbase = 0;
   }

  public void updateTime(DateTime utc)
   {
    //日本以外のマシンで実行された場合を想定する
    DateTimeOffset current = new DateTimeOffset(utc);
    DateTimeOffset japantime = current.AddHours(UtcToJST); //TimeZoneInfo.ConvertTime(current, JST);

  int y = japantime.Year;
  int m = japantime.Month;//calendar.get(Calendar.MONTH) + 1;
  int d = japantime.Day;//calendar.get(Calendar.DAY_OF_MONTH);
  int h = japantime.Hour;// calendar.get(Calendar.HOUR_OF_DAY);
  int min = japantime.Minute;// calendar.get(Calendar.MINUTE);

  //Debug.Log("solareclipse y= " + y + " m= " + m + " d= " + d + " h= " + h + " min " + min);
  calculateSunPosition((double)y, (double)m, (double)d, (double)h, (double)min);
  //time = min / 60.0 + h;
  //timevalue = StarPosition.getTime(y, m, d, time);
 }

 //retrieve the position of the sun
  public void calculateSunPosition(double year, double month, double day, double hour, double minute)
   {
    //Debug.Log( "calculateSunPosiotion");
    //Debug.Log("WORLDCLOCK y= " + year + " m= " + month + " d= " + day + " h= " + hour + " min " + minute);
    time = minute / 60.0 + hour;
    double timevalue = StarPosition.getTime(year, month, day, time);
    double elon = StarPosition.getSunEclipticLongitude(timevalue);//
    double e = StarPosition.getInclination(timevalue);//
    asc = StarPosition.getRightAscension(elon, e);//
    dec = StarPosition.getDeclination(elon, e);//
    phai0 = StarPosition.getSidereal(timevalue, time / 24.0, 0);//
    dist = StarPosition.getSunDistance(timevalue);
    parallax = StarPosition.getParallax(dist);//
    k = StarPosition.getSunriseAltitude(StarPosition.getSunDiameter(dist), 0.0, StarPosition.refraction, parallax);
    twilightk = StarPosition.getTwilightAltitude(0.0, parallax);
   }

 //必要なデータを外部から渡す場合
  public void calculateSunPosition(double[] posdata)// double rightascension, double declination, double sundistance, double timeangle)
   {
    asc = posdata[EclipseData.SUN_ASC];// rightascension;
    dec = posdata[EclipseData.SUN_DEC];// declination;
    phai0 = posdata[EclipseData.PHAI];// timeangle;
    dist = posdata[EclipseData.SUN_DIST];// sundistance;// StarPosition.getSunDistance(timevalue);
    parallax = StarPosition.getParallax(dist);//
    k = StarPosition.getSunriseAltitude(StarPosition.getSunDiameter(dist), 0.0, StarPosition.refraction, parallax);
    twilightk = StarPosition.getTwilightAltitude(0.0, parallax);
   }

 public void updateScreen()
   {
    double hinode_keido, hinoiri_keido, asayake, higure;
  int hinoiriX = 0, hinodeX = 0;
  int asayakeX = 0, higureX = 0;
/*
  Color transparent = new Color(0, 0, 0, 0);
  for (int i = 0; i < screen.height; i++)
  {
   for (int j = 0; j < screen.width; j++)
   {
    screen.SetPixel(j, i, transparent);
   }
  }
*/
  //screen.eraseColor(0xff000000);

  //screencanvas.drawBitmap(worldmap, 0.0f, 0.0f, p);//世界地図で初期化
  for (int i = 0; i < wholeheight; i++)
  {
   //double latitude = getLatitudeFromY(Yequator - i);//Javaなどの左上が0の処理系の場合
   double latitude = getLatitudeFromY(i - Yequator);//unityなどの左下が0の処理系の場合

   double jikaku = StarPosition.getTimeAngle(k, dec, latitude);
   double jikaku_twi = StarPosition.getTimeAngle(twilightk, dec, latitude);

   if (!Double.IsNaN(jikaku))//
   {
    hinode_keido = StarPosition.reviseAngle(-jikaku + asc - phai0);
    hinoiri_keido = StarPosition.reviseAngle(jikaku + asc - phai0);
    hinodeX = (int)getXfromLongitude(hinode_keido);
    hinoiriX = (int)getXfromLongitude(hinoiri_keido);//

    //drawDayLightSide(hinodeX, hinoiriX, i);//
    if (!Double.IsNaN(jikaku_twi))//
    {
     asayake = StarPosition.reviseAngle(-jikaku_twi + asc - phai0);
     higure = StarPosition.reviseAngle(jikaku_twi + asc - phai0);
     asayakeX = (int)getXfromLongitude(asayake);
     higureX = (int)getXfromLongitude(higure);

     drawNightSide(higureX, asayakeX, i);
     //
     if (asayakeX < hinodeX)
     {
      drawTwilight(latitude, asayakeX, hinodeX, i);
     }
     else
     {
      drawTwilight(latitude, asayakeX, wholewidth - 1, i);
      drawTwilight(latitude, 0, hinodeX, i);
     }
     //
     if (hinoiriX < higureX)
     {
      drawTwilight(latitude, hinoiriX, higureX, i);
     }
     else
     {
      drawTwilight(latitude, hinoiriX, wholewidth - 1, i);
      drawTwilight(latitude, 0, higureX, i);
     }
    }
    else//
    {
     if (hinodeX <= hinoiriX)
     {
      drawTwilight(latitude, hinoiriX, wholewidth - 1, i);
      drawTwilight(latitude, 0, hinodeX, i);
     }
     else
     {
      drawTwilight(latitude, hinoiriX, hinodeX, i);
     }
    }
   }
   else //
   {
    if (!Double.IsNaN(jikaku_twi))//
    {
     asayake = StarPosition.reviseAngle(-jikaku_twi + asc - phai0);
     higure = StarPosition.reviseAngle(jikaku_twi + asc - phai0);
     asayakeX = (int)getXfromLongitude(asayake);
     higureX = (int)getXfromLongitude(higure);

     if (asayakeX < higureX)
     {
      drawTwilight(latitude, asayakeX, higureX, i);
      drawNightSide(higureX, wholewidth - 1, i);
      drawNightSide(0, asayakeX, i);
     }
     else
     {
      drawTwilight(latitude, asayakeX, wholewidth - 1, i);
      drawTwilight(latitude, 0, higureX, i);
      drawNightSide(higureX, asayakeX, i);
     }
    }
    else //
    {
     //
     //          double altitude = StarPosition.getSunAltitude(asc, dec, latitude, StarPosition.getSidereal(timevalue, time / 24.0, 0.0));
     //          drawTwilight(0, wholewidth -1, i, altitude);
     drawTwilight(latitude, 0, wholewidth - 1, i);
    }
   }
  }
 }

 //
  private void drawNightSide(int higure, int asayake, int y)
   {
  //p.setColor(0x501d47bc);
  if (higure <= asayake)
  {
   for (int i = higure; i <= asayake; i++)
   {
    screen.SetPixel(i, y, nightside);
    //screen.SetPixel(i, y, nightmap.getPixel(i, y));
    //p.setColor(nightmap.getPixel(i, y));
    // screencanvas.drawPoint(i, y,p);
   }
  }
  else
  {
   for (int i = higure; i < wholewidth; i++)
   {
    screen.SetPixel(i, y, nightside);
    //    screen.SetPixel(i, y, nightmap.getPixel(i, y));
    //p.setColor(nightmap.getPixel(i, y));
    //screencanvas.drawPoint(i, y, p);
   }
   for (int i = 0; i <= asayake; i++)
   {
    screen.SetPixel(i, y, nightside);

    //        screen.SetPixel(i, y, nightmap.getPixel(i, y));
    //p.setColor(nightmap.getPixel(i, y));
    //        screencanvas.drawPoint(i, y, p);
   }
  }
 }

 /* //
   private void drawDayLightSide(int hinode, int hinoiri, int y)
    {
   //    Log.d("WORLDCLOCK", "left = " +hinode  + " right = " +hinoiri);
   //	  Log.d("WorldClock", "y=" + y);
     if (hinode <= hinoiri)
      {
       for (int i = hinode; i <= hinoiri; i++)
        {
         screen.SetPixel(i, y, worldmap.getPixel(i, y));
        }
      }
     else
      {
       for (int i = hinode; i < wholewidth; i++)
        {
         screen.SetPixel(i, y, worldmap.getPixel(i, y));
        }
       for (int i = 0; i <= hinoiri; i++)
        {
         screen.SetPixel(i, y, worldmap.getPixel(i, y));
        }
      }
    }
 */
 //
 private void drawTwilight(double latitude, int startx, int endx, int y)
 {
  int addition = startx <= endx ? 1 : -1;
  double longitude = 0.0;
  if (startx < 0 || startx >= wholewidth || endx < 0 || endx >= wholewidth) return;

  for (int i = startx; i != endx; i += addition)
  {
   longitude = (double)i / (double)wholewidth * 360.0;

   // double phai = StarPosition.getSidereal(timevalue, time / 24.0, longitude);//?P????
   double phai = phai0 + longitude; //地方恒星時　=グリニッジ恒星時＋経度 
   double altitude = StarPosition.getSunAltitude(asc, dec, latitude, phai);//???x
   if (altitude > 0.0) continue;

   if (!Double.IsNaN(altitude))
   {
    double ratio = 1.0f - (8.0 + Math.Floor(altitude)) / 12.0;
    if (ratio >= 1.0) screen.SetPixel(i, y, nightside);//screen.SetPixel(i, y, nightmap.getPixel(i, y));
                                                      //else if(ratio > 1.0) screen.setPixel(i, y, worldmap.getPixel(i, y));
    else { float alpha = nightside.a; alpha *= (float)ratio; screen.SetPixel(i, y, new Color(nightside.r, nightside.g, nightside.b, alpha)); }
    //    screen.SetPixel(i, y, composeColors(nightmap.getPixel(i, y), worldmap.getPixel(i, y), ratio));
   }
  }
 }
 //
 private double getXfromLongitude(double longitude)
 {
  double result = longitude;

  if (result <= -360.0) { result += Math.Ceiling(result / 360.0) * 360.0; }
  else if (result >= 360.0) { result -= Math.Floor(result / 360.0) * 360.0; }

  result = result / 360.0 * wholewidth + Xbase; //

  if (result > wholewidth) result -= wholewidth;
  else if (result < 0) result += wholewidth;

  return result;
 }

 //
 private double getLatitudeFromY(int y)
 {
  return (double)y / (double)Yequator * 90.0;
 }
 //
 //太陽の赤経を返す
 // public double getSunRightAscension() { return asc; }
  //太陽の赤緯を返す
 // public double getSunDeclination(){ return dec;}
  //太陽の距離を返す
 // public double getSunDistance(){ return dist; }
 }
