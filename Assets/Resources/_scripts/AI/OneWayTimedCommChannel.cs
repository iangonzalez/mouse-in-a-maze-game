using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class OneWayTimedCommChannel : TimedCommChannel {
    public override PlayerResponse GetResponse() {
        return new PlayerResponse();
    }
}
