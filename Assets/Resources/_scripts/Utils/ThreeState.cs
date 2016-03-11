public enum ThreeState {
    True,
    False,
    Neutral
}

public static class Extensions {
    public static bool ToBool(this ThreeState ts) {
        if (ts == ThreeState.True) {
            return true;            
        }
        else {
            return false;
        }
    }

    public static ThreeState ToThreeState(this bool b) {
        if (b) {
            return ThreeState.True;
        }
        else {
            return ThreeState.False;
        }
    }
}