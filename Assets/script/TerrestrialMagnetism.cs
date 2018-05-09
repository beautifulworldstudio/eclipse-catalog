using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrestrialMagnetism
 {
  private const float coequationA = 7 + (57.0f * 60 + 0.201f * 60) / 3600f;
  private const float coequationB = (18 * 60 + 0.750f * 60) / 3600f;
  private const float coequationC = (6 * 60 + 0.761f * 60) / 3600f;
  private const float coequationD = (0.059f * 60) / 3600f;
  private const float coequationE = (0.014f * 60) / 3600f;
  private const float coequationF = (0.579f * 60) / 3600f;

  public static float getMagneticDeclination(float longitude, float latitude)
   {
    float deltaphai = latitude - 37.0f;
    float deltarhamda = longitude - 138f;

    return coequationA + coequationB * deltaphai - coequationC * deltarhamda - coequationD * deltaphai * deltaphai - coequationE * deltaphai * deltarhamda - coequationF * deltarhamda * deltarhamda;
   }
 }
