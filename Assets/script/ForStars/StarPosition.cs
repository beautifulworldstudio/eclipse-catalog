using System;
using UnityEngine;

public class StarPosition
 {
  //大気差(35'8'')
  public static readonly double refraction = 0.58556;


  //黄経
  public static double getSunEclipticLongitude(double T)
  {
    double result = 280.4603 + 360.00769 * T +
       (1.9146 - 0.00005 * T) * Math.Sin((357.538 + 359.991 * T) / 180.0 * Math.PI) +
    0.02 * Math.Sin((355.05 + 719.981 * T) / 180.0 * Math.PI) +
    0.0048 * Math.Sin((234.95 + 19.341 * T) / 180.0 * Math.PI) +
    0.002 * Math.Sin((247.1 + 329.64 * T) / 180.0 * Math.PI) +
    0.0018 * (Math.Sin((297.8 + 4452.67 * T) / 180.0 * Math.PI) + Math.Sin((251.3 + 0.2 * T) / 180.0 * Math.PI)) +
    0.0015 * Math.Sin((343.2 + 450.37 * T) / 180.0 * Math.PI) +
    0.0013 * Math.Sin((81.4 + 225.18 * T) / 180.0 * Math.PI) +
    0.0008 * Math.Sin((132.5 + 659.29 * T) / 180.0 * Math.PI) +
    0.0007 * (Math.Sin((153.3 + 90.38 * T) / 180.0 * Math.PI) + Math.Sin((206.8 + 30.35 * T) / 180.0 * Math.PI)) +
    0.0006 * Math.Sin((29.8 + 337.18 * T) / 180.0 * Math.PI) +
    0.0005 * (Math.Sin((207.4 + 1.5 * T) / 180.0 * Math.PI) + Math.Sin((291.2 + 22.81 * T) / 180.0 * Math.PI)) +
    0.0004 * (Math.Sin((234.9 + 315.56 * T) / 180.0 * Math.PI) + Math.Sin((157.3 + 299.3 * T) / 180.0 * Math.PI) + Math.Sin((21.1 + 720.02 * T) / 180.0 * Math.PI)) +
    0.0003 * (Math.Sin((352.5 + 1079.97 * T) / 180.0 * Math.PI) + Math.Sin((329.7 + 44.43 * T) / 180.0 * Math.PI));

    if (result > 360.0) { result = result - 360.0 * Math.Floor(result / 360.0); }
    else if (result < 0.0) { result = result + 360.0 * Math.Ceiling(result / 360.0); }

    return result;
  }

  //太陽までの距離
  public static double getSunDistance(double T)
  {
    double q = (0.007256 - 0.0000002 * T) * Math.Sin((267.54 + 359.991 * T) / 180.0 * Math.PI) +
   0.000091 * Math.Sin((265.1 + 719.98 * T) / 180.0 * Math.PI) +
   0.00003 * Math.Sin(90 / 180.0 * Math.PI) +
   0.000013 * Math.Sin((27.8 + 4452.67 * T) / 180.0 * Math.PI) +
   0.000007 * (Math.Sin((254 + 450.4 * T) / 180.0 * Math.PI) + Math.Sin((156 + 329.6 * T) / 180.0 * Math.PI));

    return Math.Pow(10.0, q);
  }

  //赤経
  public static double getRightAscension(double elon, double e)
  {
    double alpha = Math.Atan(Math.Cos(e / 180 * Math.PI) * Math.Tan(elon / 180 * Math.PI)) / Math.PI * 180;
    if (elon >= 180.0 && elon < 360.0)
    {
      while (alpha < 180.0 || alpha >= 360.0)
      {
        if (alpha < 180.0) alpha += 180.0;
        else if (alpha >= 360.0) alpha -= 180.0;
      }
    }
    else if (elon >= 0.0 && elon < 180.0)
    {
      while (alpha < 0.0 || alpha >= 180.0)
      {
        if (alpha < 0.0) alpha += 180.0;
        else if (alpha >= 180.0) alpha -= 180.0;
      }
    }
    return alpha;
  }

  //赤緯
  public static double getDeclination(double elon, double e)
  {
    return Math.Asin(Math.Sin(e / 180 * Math.PI) * Math.Sin(elon / 180 * Math.PI)) / Math.PI * 180;
  }

  //黄道傾斜角
  public static double getInclination(double T)
  {
    return 23.439291 - 0.000130042 * T;
  }

 //恒星時(時刻,　経度λ)
 public static double getSidereal(DateTime cal, double ramda)
 {
  double T = getTimeDifferenceUT(cal);

  double phai = 325.4606 + 360.007700536 * T + 0.00000003879 * T * T + ramda; //時間、分、秒は考慮されているため、dの項は削除

  while (phai < -180.0 || phai > 180.0)
  {
   if (phai < -180.0) phai += 360.0;
   else if (phai > 180.0) phai -= 360.0;
  }

  return phai;
 }

 //恒星時(時間変数T,経過日数d,経度λ)
 public static double getSidereal(double T, double d, double ramda)
  {
    double phai = 325.4606 + 360.007700536 * T + 0.00000003879 * T * T + 360.0 * d + ramda;
    while (phai < -180.0 || phai > 180.0)
    {
      if (phai < -180.0) phai += 360.0;
      else if (phai > 180.0) phai -= 360.0;
    }

    return phai;
  }
 //2000年1月1日0時(世界協定時間)からの時刻変数Tを計算する
 public static double getTimeDifferenceUT(DateTime cal)
 {
  //cal.setTimeZone(TimeZone.getTimeZone("UTC"));

  double Year = cal.Year;
  double Month = (double)cal.Month;
  double Day = (double)cal.Day;
  double Hour = (double)cal.Hour;
  double Minute = (double)cal.Minute;
  double Second = (double)cal.Second;

  double y = Year - 2000.0;
  if (Month <= 2) { Month += 12; y -= 1; }

  //2000年1月1日から指定日の午前0時までの経過日数
  double K = 365 * y + 30 * Month + Day - 33.5 + Math.Floor(3 * (Month + 1.0) / 5.0) + Math.Floor(y / 4.0);

  return (K + Hour / 24.0 + Minute / 1440.0 + Second / 86400.0 + getDeltaT(Year) / 86400.0) / 365.25;
 }
 //2000年1月1日からの時刻変数Tを計算する
 public static double getTime(double Year, double Month, double Day, double Hour)
  {
    double y = Year - 2000.0;
    if (Month <= 2) { Month += 12; y -= 1; }

    //2000年1月1日から指定日の午前0時までの経過日数（元式を変更33.875から33.5に）
    double K = 365 * y + 30 * Month + Day - 33.5 + Math.Floor(3 * (Month + 1.0) / 5.0) + Math.Floor(y / 4.0);

    return (K + Hour / 24.0 + getDeltaT(Year) / 86400.0) / 365.25;
  }

  //自転遅れΔTを計算する(NASAの公式)
  public static double getDeltaT(double Y)
  {
    //ΔTを計算する
    double deltaT = 0.0, t = 0;

    if (Y < 1961.0 && Y >= 1941.0) { t = Y - 1950.0; deltaT = 29.07 + 0.407 * t - Math.Pow(t, 2.0 / 233.0) + Math.Pow(t, 3.0 / 2547.0); }
    else if (Y < 1986.0 && Y >= 1961.0) { t = Y - 1975.0; deltaT = 45.45 + 1.067 * t - Math.Pow(t, 2.0 / 260.0) - Math.Pow(t, 3.0 / 718.0); }
    else if (Y < 2005.0 && Y >= 1986.0) { t = Y - 2000.0; deltaT = 63.86 + 0.3345 * t - 0.060374 * Math.Pow(t, 2.0) + 0.0017275 * Math.Pow(t, 3.0) + 0.000651814 * Math.Pow(t, 4.0) + 0.00002373599 * Math.Pow(t, 5.0); }
    else { t = Y - 2000; deltaT = 62.92 + 0.32217 * t + 0.005589 * Math.Pow(t, 2); }

    return deltaT;
  }

  //太陽視半径(太陽までの天文単位での距離)
  public static double getSunDiameter(double r)
  {
    return 0.266994 / r;
  }

  //視差(太陽までの天文単位での距離)
  public static double getParallax(double r)
  {
    return 0.00244428 / r;
  }

  //日の出高度(太陽視半径S,大気屈折効果E,大気差R,視差π)
  public static double getSunriseAltitude(double S, double E, double R, double P)
  {
    return -S - E - R + P;
  }

  //薄明高度の計算(大気屈折効果E,視差π)
  public static double getDawnAltitude(double E, double P)
  {
    return -E - P + 10.00;
  }

  //薄明高度の計算(大気屈折効果E,視差π)
  public static double getTwilightAltitude(double E, double P)
  {
    return -E - P - 7.3611;
  }

  //時角(日の出高度k, 太陽の赤緯, 地球上の緯度)
  public static double getTimeAngle(double k, double delta, double lat)
  {
    delta = delta / 180.0 * Math.PI;
    lat = lat / 180.0 * Math.PI;

    return Math.Abs(Math.Acos((Math.Sin(k / 180.0 * Math.PI) - Math.Sin(delta) * Math.Sin(lat)) / (Math.Cos(delta) * Math.Cos(lat))) * 180.0 / Math.PI);
  }

 //太陽（あるいは任意の天体）の方位を計算する赤経・赤緯・緯度・恒星時
 public static double getSunDirection(double asc, double dec, double lat, double phai)
  {
   double t = (phai - asc) / 180.0 * Math.PI;
   double latitude = lat / 180.0 * Math.PI;
   double declination = dec / 180.0 * Math.PI;

   double numerator = -Math.Cos(declination) * Math.Sin(t);
   double denominator = (Math.Sin(declination) * Math.Cos(latitude) - Math.Cos(declination) * Math.Sin(latitude) * Math.Cos(t));
   double result = Math.Atan( numerator / denominator) / Math.PI * 180.0;

   if (denominator < 0.0) result += 180.0;

   return result;
  }

 //太陽（あるいは任意の天体）の高度を計算する(赤経 , 赤緯、緯度、恒星時,)
 public static double getSunAltitude(double asc, double dec, double lat, double phai)
  {
    double t = (phai - asc) / 180.0 * Math.PI;
    double latitude = lat / 180.0 * Math.PI;
    double declination = dec / 180.0 * Math.PI;
    double result = Math.Asin(Math.Sin(declination) * Math.Sin(latitude) + Math.Cos(declination) * Math.Cos(latitude) * Math.Cos(t)) / Math.PI * 180.0;

   double R = 0.0167 / Math.Tan((result + 8.6 / (result + 4.4)) / 180.0 * Math.PI);

   return result + R;
  }

  //角度の補正(角度)
  public static double reviseAngle(double angle)
  {
    if (angle >= 360.0 | angle < 0.0) return angle - Math.Floor(angle / 360.0) * 360.0;

    return angle;
  }


 }
