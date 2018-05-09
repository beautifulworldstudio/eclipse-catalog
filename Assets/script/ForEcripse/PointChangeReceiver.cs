using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PointChangeReceiver
 {
  void ObservationPointChange(float longitude, float latitude);
 }
