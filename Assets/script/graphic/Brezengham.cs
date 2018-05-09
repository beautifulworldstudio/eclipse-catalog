using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Brezengham
 {
  public static int[] line(int x1, int y1, int x2, int y2)
   {
    //System.out.println(x1 + ":" + y1 + " : " + x2 + ":" + y2);
    int dx = x2 - x1;//xの距離
    int dy = y2 - y1;//yの距離
    int xadd = 1;
    int yadd = 1;

    if (dx < 0) xadd = -1;
    if (dy < 0) yadd = -1;
    dx = Math.Abs(dx);
    dy = Math.Abs(dy);

    int e = 0;//誤差
    int index = 0;//書き込みインデックス
    int[] result = new int[] { x1, y1 };

    //不正な値の防止
   // if (dx > 1000000 | dy > 1000000) return result;

    if (dx > dy)
     {
      result = new int[(dx + 1) * 2];

      for (int y = y1, x = x1; x != x2; x += xadd)
       {
        e += dy;
        if (e > dx)
         {
          e -= dx;
          y += yadd;
         }

        result[index++] = x;
        result[index++] = y;
       }
     }
    else
     {
      result = new int[(dy + 1) * 2];
   //System.out.println("dx <= dy");

      for (int x = x1, y = y1; y != y2; y += yadd)
       {
        e += dx;
        if (e > dy)
         {
          e -= dy;
          x += xadd;
         }
        result[index++] = x;
        result[index++] = y;
       }
     }

    result[index++] = x2;
    result[index] = y2;

    return result;
   }
 }
