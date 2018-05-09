using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class EclipseDataHolder
 {
  private static EclipseData data;

 
  //番号を受け取ってデータを変更する
  public static void setEclipseData(EclipseData newdata)
   {
    data = newdata;
   }

  public static EclipseData getEclipseData()
   {
    return data;
   }
 }
