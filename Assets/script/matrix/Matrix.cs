using UnityEngine;
using System;


public class Matrix 
 {
  private Matrix() { }

  public static void getOuterProduct(double[] a, double[] b, double[] result)
  {
    if (a.Length != 3 || b.Length != 3 || result.Length != 3) return;

    result[0] = a[1] * b[2] - a[2] * b[1];
    result[1] = a[2] * b[0] - a[0] * b[2];
    result[2] = a[0] * b[1] - a[1] * b[0];
  }


  //逆行列を得る(縦配置)
  public static void getInverseMatrixType2(double[][] mat, double[][] result)
  {
    if (mat.Length != 3 | result.Length != 3) return;

    //  System.out.println(-vec_a[0] + ":" + (-vec_a[1]) + " : " + (-vec_a[2]));
    //行列の構造
    // A[] B[] C[]
    // A0  B0  C0
    // A1  B1  C1
    // A2  B2  C2

    double det = mat[0][0] * mat[1][1] * mat[2][2] + mat[1][0] * mat[2][1] * mat[0][2] + mat[2][0] * mat[0][1] * mat[1][2] -
                 (mat[0][0] * mat[2][1] * mat[1][2] + mat[2][0] * mat[1][1] * mat[0][2] + mat[1][0] * mat[0][1] * mat[2][2]);

    if (det == 0.0) { /* Debug.Log("det == 0");*/ return; }

    //余因子
    for (int i = 0; i < 3; i++)
    {
      int left = i == 0 ? 1 : 0;
      int right = (left + 1) == i ? left + 2 : left + 1;

      for (int j = 0; j < 3; j++)
      {
        int top = j == 0 ? 1 : 0;
        int bottom = (top + 1) == j ? top + 2 : top + 1;

        //         double val =  Math.pow(-1.0, i + j) * getDeterminant(mat[left][top], mat[right][top], mat[left][bottom], mat[right][bottom]) / det;

        double val = Math.Pow(-1.0, i + j) * (mat[left][top] * mat[right][bottom] - mat[right][top] * mat[left][bottom]) / det;
        //添え字が一致しないときは転置を実行する
        if (i == j) result[i][j] = val;
        else result[j][i] = val;
      }
    }
    //この時点の配列構造
    //result[0] result[1] result[2]
    //00 10 20
    //10 11 21
    //20 12 22
  }


  //配列が横向きの時の3行3列の掛け算
  public static void multiplication33type1(double[][] left, double[][] right, double[][] result)
  {
    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        result[i][j] = left[i][0] * right[0][j] + left[i][1] * right[1][j] + left[i][2] * right[2][j];
      }
    }
  }

  //配列が縦向きの時の3行3列の掛け算(各配列は縦向きに値を格納している)
  public static void multiplication33type2(double[][] left, double[][] right, double[][] result)
  {
    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        result[i][j] = left[0][i] * right[j][0] + left[1][i] * right[j][1] + left[2][i] * right[j][2];
      }
    }
  }

  //配列が横向きの時の3行3列の行列と列ベクトルの掛け算
  public static void multiplication31type1(double[][] left, double[] right, double[] result)
  {
    for (int i = 0; i < 3; i++)
    {
      result[i] = left[i][0] * right[0] + left[i][1] * right[1] + left[i][2] * right[2];
    }
  }


  //配列が縦向きの時の3行3列の行列(縦配置)と列ベクトルの掛け算
  public static void multiplication31type2(double[][] left, double[] right, double[] result)
  {
    for (int i = 0; i < 3; i++)
    {
      result[i] = left[0][i] * right[0] + left[1][i] * right[1] + left[2][i] * right[2];
    }
  }
}
