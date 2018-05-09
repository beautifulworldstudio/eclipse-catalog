using UnityEngine;
using System;

public class Almanac
 {
  private Almanac() { }

  //グリニッジ恒星時を計算する（度）
  public static double getGreenidgeSiderealTime(DateTime cal)
   {
    //    cal.DayOfYear;//setした日付を有効にするためのダミー

    //cal.setTimeZone(TimeZone.getTimeZone("UTC")); //世界協定時刻へ変換

    //TJD(NASAが導入した世界時1968年3月24日0時からの日数)の計算方法
    //グレゴリオ暦（1582年10月15日以降）の西暦年をY、月をM、日をDとする。
    //ただし1月のはM=13、2月はM=14、YはY=Y-1とする。

    double JD = getJulianDay(cal);
    double TJD = JD - 2440000.5;
    double thetaG = (0.671262 + 1.0027379094 * TJD);

    return 360.0 * (thetaG - Math.Floor(thetaG));
  }

  //ユリウス日を計算する
  public static double getJulianDay(DateTime cal)
  {
    double Y = (double)cal.Year;
    double M = (double)cal.Month; //Calendarは0から11で格納するため、1加算
    double D = (double)cal.Day;// get(Calendar.DAY_OF_MONTH);
    double H = (double)cal.Hour;// get(Calendar.HOUR_OF_DAY);
    double Mi = (double)cal.Minute;
    double S = (double)cal.Second;

    if (M < 3.0) { Y -= 1.0; M += 12.0; }

    return Math.Floor(365.25 * Y) + Math.Floor(Y / 400.0) - Math.Floor(Y / 100.0) + Math.Floor(30.59 * (M - 2.0)) + D + 1721088.5 + H / 24.0 + Mi / 1440.0 + S / 86400.0;
  }
}
