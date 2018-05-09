using UnityEngine;

public class caliculation
 {
  public const float DecToRad = Mathf.PI / 180.0f;
  public const float polar = 6356751.9f; //極半径・メートル
  public const float equater = 6378136.6f;//赤道半径・メートル

  //地球上の経度、緯度、高さから座標値を計算する
  public static void getXYZPosition(float longitude, float latitude, float height, float[] container)
   {
    if (container == null || container.Length != 3) return;
    latitude = latitude * DecToRad; //緯度
    longitude = longitude * DecToRad;//経度

    float xlength = Mathf.Cos(latitude) * equater; //経度0度上で緯度分だけ回転させる。
    float zlength = Mathf.Sin(latitude) * polar;
    float radius = Mathf.Sqrt(xlength * xlength + zlength * zlength) + height;//高度を加える
    
    float x = Mathf.Cos(latitude) * radius;//楕円と高度をを考慮した座標値
    float z = Mathf.Sin(latitude) * radius;

    float y = x * Mathf.Sin(longitude);//経度分回転させる
    x = x * Mathf.Cos(longitude);

    container[0] = x;
    container[1] = y;
    container[2] = z;
   }

  //地球上の経度、緯度から座標値を計算する
  public static void getXYZPosition(float longitude, float latitude, float[] container)
   {
    if (container == null || container.Length != 3) return;
    latitude = latitude * DecToRad; //緯度
    longitude = longitude * DecToRad;//経度

    float x = Mathf.Cos(latitude) * equater; //経度0度上で緯度分だけ回転させる。
    float z = Mathf.Sin(latitude) * polar;
   
    float y = x * Mathf.Sin(longitude);//経度分回転させる
    x = x * Mathf.Cos(longitude);

    container[0] = x;
    container[1] = y;
    container[2] = z;
   }

 public static float getLongitudeFromXYZ(float[] XYZ)
   {
    float radius = Mathf.Sqrt(XYZ[0] * XYZ[0] + XYZ[1] * XYZ[1]);
    float angle = Mathf.Acos(XYZ[0] / radius) / DecToRad;
  return angle;
   }

  public static float getLatitudeFromXYZ(float[] XYZ)
   { 
    float radius = Mathf.Sqrt(XYZ[0] * XYZ[0] + XYZ[1] * XYZ[1] + XYZ[2] * XYZ[2]);
    float angle = Mathf.Asin(XYZ[2] / radius) / DecToRad;
    return angle;
  }

 public static float[,] getMatrix(float latitude, float longitude, float height)
   {
    float[] pos = new float[3];
  
    getXYZPosition(latitude, longitude, pos);
    float[,] matrix = new float[3,3];
    float norm = Mathf.Sqrt(pos[0] * pos[0] + pos[1] * pos[1] + pos[2] * pos[2]);
    pos[0] /= norm;
    pos[1] /= norm;
    pos[2] /= norm;

    //法線方向のベクトルを求める
    matrix[2, 0] = pos[0];
    matrix[2, 1] = pos[1];
    matrix[2, 2] = pos[2];

    latitude *= DecToRad;
    longitude *= DecToRad;

    //緯度方向(x軸)のベクトルを計算する
    //円周の座標（cos * equator, sin * polar, 0）の1回微分（-sin * equator, cos * polar, 0）に経度を代入することで得られる
    matrix[0, 0] = -Mathf.Sin(latitude) * equater;
    matrix[0, 1] =  Mathf.Cos(latitude) * polar;
    matrix[0, 2] = 0.0f;

    //経度方向(y軸)のベクトルを求める
    //ベクトルとz軸との交点を求める。z軸と接線の作る直角三角形を利用する
    if (longitude > 0.0f)
     {
      float angle = Mathf.PI / 2.0f - longitude;
      matrix[1, 0] = -pos[0];
      matrix[1, 1] = -pos[1];
      matrix[1, 2] = Mathf.Sqrt(pos[0] * pos[0] + pos[1] * pos[1] + pos[2] * pos[2]) / Mathf.Cos(angle) - pos[2];
     }
    else if (longitude < 0.0f)
     {
      float angle = Mathf.PI / 2.0f + longitude;
      matrix[1, 0] = -pos[0];
      matrix[1, 1] = -pos[1];
      matrix[1, 2] = pos[2] + Mathf.Sqrt(pos[0] * pos[0] + pos[1] * pos[1] + pos[2] * pos[2]) / Mathf.Cos(angle);//pos.zは必ず負数
     }
    else //赤道上は特異点
     {
      matrix[1, 0] = 0.0f;
      matrix[1, 1] = 0.0f;
      matrix[1, 2] = 1.0f;
     }

    //単位化
    for (int i = 0; i < 3; i++)
     {
      norm = Mathf.Sqrt(matrix[i, 0] * matrix[i, 0] + matrix[i, 1] * matrix[i, 1] + matrix[i, 2] * matrix[i, 2]);
      matrix[i, 0] /= norm;
      matrix[i, 1] /= norm;
      matrix[i, 2] /= norm;
     }

    return matrix;
   }

  public static float[,] getInverseMatrix(float[,] matrix)
   {
    if (matrix.GetLength(0) != matrix.GetLength(1)) return null;//正方行列ではない

    float[,] result = new float[matrix.GetLength(0), matrix.GetLength(1)];
    //行列式を求める
    float determinant = getDeterminant(matrix);

    if (determinant == 0.0f) return result;//行列式不正。逆行列存在せず

    for (int i = 0; i < 3; i++)
     {
      for (int j = 0; j < 3; j++)
       {
        //余因子行列を求める
        float cofactor = getCofactor(matrix, i, j) / determinant;
        if (((i + j) % 2) != 0) cofactor = -cofactor;
        result[j, i] = cofactor; //格納時に転置する
       }
     }
    return result;
   }

  public static float[] multiplyMatrix(float[] vector, float[,] matrix)
   {
    if (vector.Length != matrix.GetLength(0) || matrix.GetLength(0) != matrix.GetLength(1)) return null;

    float[] result = new float[vector.Length];

    for(int i = 0; i < vector.Length; i++)
     {
      for (int j = 0; j < vector.Length; j++)
       {
        result[i] += vector[j] * matrix[j, i];
       }
     }
    return result;
   }

  public static float[] multiplyMatrix2(float[] vector, float[,] matrix)
   {
    if (vector.Length != matrix.GetLength(0) || matrix.GetLength(0) != matrix.GetLength(1)) return null;

    float[] result = new float[vector.Length];

    for (int i = 0; i < vector.Length; i++)
     {
      for (int j = 0; j < vector.Length; j++)
       {
        result[i] += vector[j] * matrix[i, j];
       }
     }
    return result;
   }

  public static float[,] multiplyMatrix3(float[,] matrix, float[,] matrix2)
   {
    float[,] result = new float[matrix.GetLength(0), matrix.GetLength(0)];

    for (int i = 0; i < matrix.GetLength(0); i++)
     {
      for (int j = 0; j < matrix.GetLength(0); j++)
       {
        for (int k = 0; k < matrix.GetLength(0); k++)
         {
          result[i, j] += matrix[i, k] * matrix2[k, j];
         }
       }
     }
    return result;
   }


 //渡された行列の行列式を求める
  public static float getDeterminant(float[,] matrix)
   {
    if (matrix.GetLength(0) != matrix.GetLength(1)) return 0;//正方行列ではない

    int rows = matrix.GetLength(0);

    //2行2列の時は簡単に計算して返す
    if (rows == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

    //3行以上の時
    int[] index = new int[rows];
    float determinant = 0.0f;

    for (int i = 0; i < rows; i++)
     {
      //加算する値を計算する
      for (int j = 0; j < rows; j++)
       {
        index[j] = i + j;
        if (index[j] >= rows) index[j] -= rows;
       }
      float val = 1.0f;

      for (int k = 0; k < rows; k++)
       {
        val *= matrix[k, index[k]];
       }
      determinant += val;

     //減算する値を計算する
      for (int j = 0; j < rows; j++)
       {
        index[j] = i - j;
        if (index[j] < 0) index[j] += rows;
       }
      val = 1.0f;

      for (int k = 0; k < rows; k++)
       {
        val *= matrix[k, index[k]];
       }
      determinant -= val;
     }
    return determinant;
   }

  //余因子行列の行列式を求める
  private static float getCofactor(float[,] matrix, int i, int j)
   {
    int rows = matrix.GetLength(0); //元の行列の幅
    int columns = rows - 1; //余因子行列の幅

    float[,] cofactor = new float[columns, columns];

    //余因子で行列を作る
    for(int u = 0, counteru = 0; u < rows; u++)
     {
      if (u == i) continue;
      for (int v = 0, counterv= 0; v < rows; v++)
       {
        if (v == j) continue;
        cofactor[counteru, counterv] = matrix[u, v];
        counterv++;
       }
      counteru++;
     }
    return getDeterminant(cofactor);//余因子の行列式を返す
   }
 }
