public class RoomExitPathCommChannel : PathCommuncationChannel { 

    public override bool IsResponseReceived() {
        commComplete = !player.InCell && safeToCheckResponse;
        return commComplete;    
    }
}
