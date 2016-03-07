using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoomExitPathCommChannel : PathCommuncationChannel { 

    public override bool IsResponseReceived() {
        commComplete = !player.InCell && safeToCheckResponse;
        return commComplete;    
    }


}
