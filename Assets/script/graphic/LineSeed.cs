using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 


public class LineSeed
 {
  public static void paint(int x, int y, Texture2D tex, Color paintcolor)
   {
    if (x < 0 | x >= tex.width | y < 0 | y >= tex.height) { /* Debug.Log("invalid value " + x + ":" + y + ": " + tex.width + ":" + tex.height);*/ return; }

    Color targetcolor = tex.GetPixel(x, y);
    ArrayList buffer = new ArrayList();
    buffer.Add(new Point(x, y));

    while(buffer.Count != 0)
     {
      IEnumerator entries = buffer.GetEnumerator();
      entries.MoveNext();
      Point seed = (Point)entries.Current;
      buffer.Remove(seed);

       //ピクセルに色を置く
      tex.SetPixel(seed.x, seed.y, paintcolor);
      //探索開始ピクセルを初期化
      int u = seed.x - 1;
      int left = 0;
      int right = 0;

      //左方向に探索して塗る
      while (u >= 0 && tex.GetPixel(u, seed.y) == targetcolor) { tex.SetPixel(u, seed.y, paintcolor); u--; }
      u++;//塗る対象のピクセルではないので、+1して元に戻す
      left = u >= 0 ? u : 0;

      //右方向に探索して塗る
      u = seed.x + 1;
      while (u < tex.width && tex.GetPixel(u, seed.y) == targetcolor) { tex.SetPixel(u, seed.y, paintcolor); u++; }
　　  u--;//塗る対象のピクセルではないので、-1して元に戻す
      right = u < tex.width ? u : tex.width - 1;

      //1段下に次のシードを探索する
      Point nextseed = null;
      int v = seed.y + 1; 
      if (v < tex.height)
       {
        for (int i = left; i <= right; i++)
         {
          if (tex.GetPixel(i, v) == targetcolor)
           {
            if (nextseed == null) nextseed = new Point(i, v);
            else if (Math.Abs(nextseed.x - i) == 1) nextseed.x = i;
           }
          else if (nextseed != null)
           {
            buffer.Add(nextseed);
            nextseed = null; 
           }
         }
        //ループ脱出時にシードが未登録なら記録する
        if (nextseed != null) buffer.Add(nextseed);
       }

      //1段上に次のシードを探索する
      nextseed = null;
      v = seed.y - 1;
      if (v >= 0)
       {
        for (int i = left; i <= right; i++)
         {
          if (tex.GetPixel(i, v) == targetcolor)
           {
            if (nextseed == null) nextseed = new Point(i, v);
            else if (Math.Abs(nextseed.x - i) == 1) nextseed.x = i;
           }
          else if (nextseed != null)
           {
            buffer.Add(nextseed);
            nextseed = null;
           }
         }
        //ループ脱出時にシードが未登録なら記録する
        if (nextseed != null) buffer.Add(nextseed);
       }
     }//endof foreach
   }


  public static void paintCylinder(int x, int y, Texture2D tex, Color paintcolor)
   {
    if (y < 0 | y >= tex.height) { /*Debug.Log("invalid value " + x + ":" + y + ": " + tex.width + ":" + tex.height);*/ return; }

    Color targetcolor = tex.GetPixel(x, y);
    List<Point> buffer = new List<Point>();
    buffer.Add(new Point(x, y));

    while (buffer.Count > 0)
     {
     IEnumerator entries = buffer.GetEnumerator();
     entries.MoveNext();
     Point seed = (Point)entries.Current;
     buffer.Remove(seed);
  
      //ピクセルに色を置く
      if (seed.x < 0) tex.SetPixel(seed.x + tex.width, seed.y, paintcolor);
      else if (seed.x >= tex.width) tex.SetPixel(seed.x - tex.width, seed.y, paintcolor);
      else tex.SetPixel(seed.x, seed.y, paintcolor);

      //探索開始ピクセルを初期化
      int u = seed.x - 1;
      int left = 0;
      int right = 0;

      //左方向に探索して塗る
      while (true)
       {
        Color pixelcolor;
        int actualU = u;

        if (u < 0) { actualU = u + tex.width; }
        else if (u >= tex.width) { actualU = u - tex.width; }

        pixelcolor = tex.GetPixel(actualU, seed.y);

        if ( pixelcolor != targetcolor) break;
        tex.SetPixel(actualU, seed.y, paintcolor);
        u--; 
       }
      u++;//塗る対象のピクセルではないので、+1して元に戻す
      left = u;

      //右方向に探索して塗る
      u = seed.x + 1;
      while (true)
       {
        Color pixelcolor;
        int actualU = u;
 
        if (u < 0) { actualU = u + tex.width; }
        else if (u >= tex.width) { actualU = u - tex.width; }

        pixelcolor = tex.GetPixel(actualU, seed.y);

        if (pixelcolor != targetcolor) break;
        tex.SetPixel(actualU, seed.y, paintcolor);
        u++;
       }
      u--;//塗る対象のピクセルではないので、-1して元に戻す
      right = u;

      //1段下に次のシードを探索する
      Point nextseed = null;
      int v = seed.y + 1;

      if (v < tex.height)
       {
        for (int i = left; i <= right; i++)
         {
          int actualU = i;

          if (i < 0) { actualU = i + tex.width; }
          else if (i >= tex.width) { actualU = i - tex.width; }

          if (tex.GetPixel(actualU, v) == targetcolor)
           {
            if (nextseed == null) nextseed = new Point(i, v);
            else if (Math.Abs(nextseed.x - i) == 1) nextseed.x = i;
           }
          else if (nextseed != null)
           {
            buffer.Add(nextseed);
            nextseed = null;
           }
         }
        //ループ脱出時にシードが未登録なら記録する
        if (nextseed != null) { buffer.Add(nextseed);  }
       }

      //1段上に次のシードを探索する
      nextseed = null;
      v = seed.y - 1;
      if (v >= 0)
       {
        for (int i = left; i <= right; i++)
         {
          int actualU = i;

          if (i < 0) { actualU = i + tex.width; }
          else if (i >= tex.width) { actualU = i - tex.width; }

          if (tex.GetPixel(actualU, v) == targetcolor)
           {
            if (nextseed == null) nextseed = new Point(i, v);
            else if (Math.Abs(nextseed.x - i) == 1) nextseed.x = i;
           }
          else if (nextseed != null)
           {
            buffer.Add(nextseed);
            nextseed = null;
           }
         }
        //ループ脱出時にシードが未登録なら記録する
        if (nextseed != null) 
         {
          buffer.Add(nextseed);
         }
       }
     }//endof foreach
   }
 }
