using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TimedPathCommChannel : PathCommuncationChannel {
    public override bool IsResponseReceived() {
        //TODO: Add timing code
        return true;
    }
}