using UnityEngine;
using System;

public class SolarEclipse
 {
  private SolarEclipse() { }

  //食分を計算する
  public static void getD(VesselElements ve, double longitude, double latitude, double altitude)//, double[] result)
   {
    longitude = longitude / 180.0 * Math.PI;
    latitude = latitude / 180.0 * Math.PI;

    //測地緯度と地心緯度の差を計算
    double deltalat = 3.3584196 * 0.001 * Math.Sin(2 * latitude) - 5.635 * 0.000001 * Math.Sin(4 * latitude) + 1.7 * Math.Pow(10, -8) * Math.Sin(8 * latitude);

    //地心直交座標を計算 
    double sinlat = Math.Sin(latitude);
    double N = 1 / Math.Sqrt(1 - Constants.e2 * sinlat * sinlat);
 
    double u = (N + altitude) * Math.Cos(latitude) * Math.Cos(longitude);
    double v = (N + altitude) * Math.Cos(latitude) * Math.Sin(longitude);
    double w = (N * (1.0 - Constants.e2) + altitude) * Math.Sin(latitude);

    //基準座標
    double myu = ve.getMyu();
    double d = ve.getDeclination();

    double x = u * Math.Sin(myu) + v * Math.Cos(myu);
    double y = -u * Math.Cos(myu) * Math.Sin(d) + v * Math.Sin(myu) * Math.Sin(d) + w * Math.Cos(d);
    double z = u * Math.Cos(myu) * Math.Cos(d) - v * Math.Sin(myu) * Math.Cos(d) + w * Math.Sin(d);

    //r,deltaを計算する
    double r = Math.Sqrt(x * x + y * y + z * z);
    double xdiff = x - ve.getX0();
    double ydiff = y - ve.getY0();
    double delta = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);

    //cosQ, sinQを計算する
    double cosQ = xdiff / delta;
    double sinQ = ydiff / delta;

    //月の北極方向角を計算する
    double tanphai = (xdiff / ydiff) + 0.0000426 * xdiff * Math.Tan(d) / (sinQ * sinQ);
    double moon_angle_to_northpole = Math.Atan(tanphai)/ Math.PI * 180;
    if (moon_angle_to_northpole < 0) moon_angle_to_northpole += 360;
    else if (moon_angle_to_northpole > 360) moon_angle_to_northpole -= 360;
    if(ydiff > 0)
     {
      if (moon_angle_to_northpole > 270 | moon_angle_to_northpole < 90) moon_angle_to_northpole += 180;
     }
    else if(ydiff < 0)
     {
      if (moon_angle_to_northpole > 90 & moon_angle_to_northpole < 270) moon_angle_to_northpole += 180;
     }
    if (moon_angle_to_northpole > 360) moon_angle_to_northpole -= 360;

  //天頂の北極方位角を計算する
    double tannyu = x / y - delta * latitude * r * x * Math.Cos(d) / (y * y * Math.Cos(latitude));
    double zenith_angle_to_northpole = Math.Atan(tannyu)/ Math.PI * 180;
    if (zenith_angle_to_northpole < 0) zenith_angle_to_northpole += 360;
    else if (zenith_angle_to_northpole > 360) zenith_angle_to_northpole -= 360;

    if (y < 0)
     {
      if (zenith_angle_to_northpole > 270 | zenith_angle_to_northpole < 90) zenith_angle_to_northpole += 180;
     }
    else if (y > 0)
     {
      if (zenith_angle_to_northpole > 90 & zenith_angle_to_northpole < 270) zenith_angle_to_northpole += 180;
     }
    if (zenith_angle_to_northpole > 360) zenith_angle_to_northpole -= 360;

    double omega = moon_angle_to_northpole - zenith_angle_to_northpole;
    if(Math.Abs(omega) > 180)
     {
      if (omega < 0) omega += 360;
      else if (omega > 0) omega -= 360;  
     }
  //omega = omega / Math.PI * 180.0;
   // Debug.Log("omega= " + omega);
  //食分を求める
  double D = (ve.getL1() - z * ve.getTanf1() - delta) / (ve.getL1() + ve.getL2() - z * (ve.getTanf1() + ve.getTanf2()));
  //debug
/*
  Debug.Log("deltalat = " + deltalat);
  Debug.Log("x0 = "+ ve.getX0());
  Debug.Log("y0 = " + ve.getY0());
  Debug.Log("d = " + (ve.getDeclination() / Math.PI * 180.0));
  Debug.Log("l1 = " + ve.getL1());
  Debug.Log("l2 = " + ve.getL2());
  Debug.Log("tanf1 = " + ve.getTanf1());
  Debug.Log("tanf2 = " + ve.getTanf2());
  Debug.Log("myu = " + myu);
  Debug.Log("x = " + x);
  Debug.Log("y = " + y);
  Debug.Log("z = " + z);
  Debug.Log("r = " + r);
  Debug.Log("delta = " + delta);
  Debug.Log("sinQ = " + sinQ);
  Debug.Log("(x - x0)/ (y - y0) = " + (xdiff / ydiff));
  Debug.Log("pais(x-x0)tan(d) / sin2 Q  = " + (0.0000426 * xdiff * Math.Tan(d) / (sinQ * sinQ)));
  Debug.Log("Tan(phai) = " + tanphai);
  Debug.Log("phai = " + moon_angle_to_northpole);
  Debug.Log("x / y = " + (x / y));
  Debug.Log("delta phai = " + (delta * latitude * r * x * Math.Cos(d) / (y * y * Math.Cos(latitude))));
  Debug.Log("tan(nyu) = " + tannyu);
  Debug.Log("nyu = " + zenith_angle_to_northpole);
  Debug.Log("omega = " + omega);
  Debug.Log("D=" + D);
*/
//debug終わり
   }

  //本影の輪郭を計算する。qは角度で与えられる
  public static void getUmbralOutline(VesselElements ve, double q, double[] result)
   {
    getOutline(ve, ve.getL2(), ve.getTanf2(), q, result);
   }


  //半影の輪郭を計算する
  public static void getPenumbralOutline(VesselElements ve, double q, double[] result)
   {
    getOutline(ve, ve.getL1(), ve.getTanf1(), q, result);
   }


  //本影・半影の輪郭を計算する。qは角度で与えられる
  private static void getOutline(VesselElements ve, double l, double tan, double q, double[] result)
   {
    if (result.Length != 3) return;

    double x = 0.0;
    double y = 0.0;
    double z = 1.0; //zの初期値
    double Q = q / 180 * Math.PI;
    double lastz = 0.0; //前のzの値
    double delta = 0.0;
    double cos_d = Math.Cos(ve.getDeclination() / 180 * Math.PI);
    double sin_d = Math.Sin(ve.getDeclination() / 180 * Math.PI);
    double cos_d2 = cos_d * cos_d;
    int count = 0;

    while (Math.Abs(z - lastz) > 10e-7)//zが10の-7乗未満に収束しない場合ループ
     {
      lastz = z;
      delta = l - z * tan;
      x = ve.getX0() + delta * Math.Cos(Q);
      y = ve.getY0() + delta * Math.Sin(Q);

      double x2 = x * x;
      double y2 = y * y;

      z = (-Constants.e2 * y * cos_d * sin_d + Math.Sqrt((1 - Constants.e2) * (1.0 - x2 - y2 - Constants.e2 * (1.0 - x2) * cos_d2))) /
          (1.0 - Constants.e2 * cos_d2);

      if (count++ > 10) break; //10回を超えたら強制的に打ち切り
    }

    result[0] = x;
    result[1] = y;
    result[2] = z;
  }

  //半影と地球外周楕円の交点を計算する
  public static void getCrossPoint(VesselElements ve, double[] result1, double[] result2)
  {
    if (result1.Length != 3 || result2.Length != 3) return;

    double x0 = ve.getX0();
    double y0 = ve.getY0();
    double l1 = ve.getL1();
    double r0 = Math.Sqrt(x0 * x0 + y0 * y0);
    double theta = Math.Atan2(y0, x0); //ラジアン。変換しない。
    double d = ve.getDeclination() / 180.0 * Math.PI;
    double cos_d = Math.Cos(d);
    double sin_d = Math.Sin(d);

    double E2 = (Constants.e2 * cos_d * cos_d) / (1.0 - Constants.e2 * sin_d * sin_d);

    double rhoplus = 1.0; //初期値
    double gammaplus = 0.0;

    int count = 0;
    //正の値の近似計算
    while (true)
    {
      double angle = Math.Acos((r0 * r0 + rhoplus * rhoplus - l1 * l1) / (2.0 * r0 * rhoplus)); //近似計算//正の値
      if (angle < 0.0) angle = -angle;

      gammaplus = angle + theta;
      double newrhoplus = Math.Sqrt((1.0 - E2) / (1.0 - E2 * Math.Cos(gammaplus) * Math.Cos(gammaplus)));

      double dif = Math.Abs(newrhoplus - rhoplus);
      rhoplus = newrhoplus;

      if (dif < 10e-7) break;
      if (count++ > 8) break;
    }

    double rhominus = 1.0;
    double gammaminus = 0.0;
    count = 0;

    //負の値の近似計算
    while (true)
    {
      double angle = Math.Acos((r0 * r0 + rhominus * rhominus - l1 * l1) / (2.0 * r0 * rhominus)); //近似計算//正の値
      if (angle > 0.0) angle = -angle;

      gammaminus = angle + theta;
      double newrhominus = Math.Sqrt((1.0 - E2) / (1.0 - E2 * Math.Cos(gammaminus) * Math.Cos(gammaminus)));

      double dif = Math.Abs(newrhominus - rhominus);
      rhominus = newrhominus;

      if (dif < 10e-7) break;
      if (count++ > 8) break;
    }
    result1[0] = rhoplus * Math.Cos(gammaplus);
    result1[1] = rhoplus * Math.Sin(gammaplus);
    result1[2] = 0.0;
    result2[0] = rhominus * Math.Cos(gammaminus);
    result2[1] = rhominus * Math.Sin(gammaminus);
    result2[2] = 0.0;

    //    System.out.println("rhoplus = " + rhoplus + " rhominus =" + rhominus);
  }

  //基準面上の半影における点の偏角Qを計算する
  public static double getPenumbralQ(VesselElements ve, double[] point)
  {
    if (point == null || point.Length != 3) return Double.NaN;

    double delta = ve.getL1() - point[2] * ve.getTanf1();

    double Q = Math.Acos((point[0] - ve.getX0()) / delta) / Math.PI * 180.0;
    if ((point[1] - ve.getY0()) < 0.0) Q = -Q; //y方向が負ならば角度を反転する。
    if (Q < 0.0) Q += 360.0;

    return Q;
  }

}
