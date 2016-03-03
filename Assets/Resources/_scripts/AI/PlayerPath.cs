using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerPath {
    public List<Vector3> pointList;
    public Dictionary<Vector3, int> pointOrder;

    private int pointsAlreadyTraversed;
    
    public PlayerPath(List<Vector3> ptList, bool initWithListOrder) {
        pointList = ptList;
        pointOrder = new Dictionary<Vector3, int>();

        for (var i  = 0; i < pointList.Count; i++) {
            pointOrder[pointList[i]] = (initWithListOrder ? i : -1);
        }

        pointsAlreadyTraversed = (initWithListOrder ? pointList.Count : 0);
    }

    public void TraversePointInPath(Vector3 point) {
        if (pointsAlreadyTraversed >= pointList.Count) {
            return;
        }

        pointOrder[point] = pointsAlreadyTraversed;
        pointsAlreadyTraversed++;
    }

    public bool ArePointsInCorrectOrder() {
        var retValue = true;
        for (var i = 0; i < pointList.Count; i++) {
            retValue = retValue && (pointOrder[pointList[i]] == i);
        }
        return retValue;
    }

    public bool WereAllPointsTraversed() {
        var retValue = true;
        for (var i = 0; i < pointList.Count; i++) {
            retValue = retValue && (pointOrder[pointList[i]] != -1);
        }
        return retValue;
    }
}
