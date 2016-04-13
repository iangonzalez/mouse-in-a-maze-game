using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class AiPlayerInterchange {
    private AIAlignmentState aiState;
    public PlayerResponse expectedResponse;

    protected bool isFirst = false;
    

    public AiPlayerInterchange(AIAlignmentState state) {
        aiState = state;
        expectedResponse = new PlayerResponse();
    }

    public AiPlayerInterchange(AIAlignmentState state, PlayerResponse expected) {
        aiState = state;
        expectedResponse = expected;
    }

    public AiPlayerInterchange(AIAlignmentState state, PlayerResponse expected, bool firstInterchange) {
        aiState = state;
        expectedResponse = expected;
        isFirst = firstInterchange;
    }

    public abstract ThreeState CheckIfCorrectResponse(PlayerResponse response);
    public abstract string GetQuestionText();
    public abstract string GetResponseToPlayerText(bool responseIsPositive);
}

public abstract class PathInterchange : AiPlayerInterchange {

    public PathInterchange(AIAlignmentState state) : base(state) {   }

    public PathInterchange(AIAlignmentState state, PlayerResponse expected) : base(state, expected) {   }

    public PathInterchange(AIAlignmentState state, PlayerResponse expected, bool firstInterchange)
        : base(state, expected, firstInterchange) {   }

    public override ThreeState CheckIfCorrectResponse(PlayerResponse response) {
        if (response.pathInOrder) {
            return response.playerPath.ArePointsInCorrectOrder().ToThreeState();
        }
        else {
            return response.playerPath.WereAllPointsTraversed().ToThreeState();
        }
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        //todo: change this to be path specific
        if (isFirst) {
            return GameLinesTextGetter.FirstResponse(isPositive: responseIsPositive);
        }
        else {
            return GameLinesTextGetter.RandomResponse(isPositive: responseIsPositive);
        }
    }
}

public class TouchCornersInterchange : PathInterchange {

    public TouchCornersInterchange(AIAlignmentState state, PlayerResponse response, bool first) 
        : base(state, response, first) {    }

    public TouchCornersInterchange(AIAlignmentState state, PlayerResponse response) : base(state, response) {   }

    public override string GetQuestionText() {
        //change this to be path specific
        if (isFirst) {
            return GameLinesTextGetter.FirstRequest();
        }
        else {
            return GameLinesTextGetter.RandomRequestIntro() + "\nTouch all 4 corners of the room before moving on.";
        }
    }
}

public class LockPlayerInRoomInterchange : AiPlayerInterchange {

    public LockPlayerInRoomInterchange(AIAlignmentState state) : base(state) {  }

    public float timeLocked = 10.0f;

    public override ThreeState CheckIfCorrectResponse(PlayerResponse response) {
        return ThreeState.Neutral;
    }

    public override string GetQuestionText() {
        return "I'm going to lock you in this room for " + timeLocked + " seconds. Have fun!";
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        return "Thanks for waiting! And no, I won't tell you why I did that.";
    }
}

public class StayStillInterchange : AiPlayerInterchange {
    public StayStillInterchange(AIAlignmentState state) : base(state) { }

    public override ThreeState CheckIfCorrectResponse(PlayerResponse response) {
        return (!response.playerMoved).ToThreeState();
    }

    public override string GetQuestionText() {
        return "Please remain still for 10 seconds.";
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        return GameLinesTextGetter.RandomResponse(isPositive: responseIsPositive);     
    }
}

public class GenericTextInterchange : AiPlayerInterchange {
    public string question;
    public string responseFromPlayer;
    public string responseToPlayer;

    public Func<bool, string> getResponseToPlayer;

    public GenericTextInterchange(AIAlignmentState state) : base(state) { }

    public override ThreeState CheckIfCorrectResponse(PlayerResponse response) {
        return (response.responseStr.ToLower().Trim() == responseFromPlayer.ToLower().Trim()).ToThreeState();
    }

    public override string GetQuestionText() {
        return question;
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        return getResponseToPlayer(responseIsPositive);
    }

    public void SetQuestionAndResponse(string question, Func<bool, string> responseFunc) {
        this.question = question;
        this.getResponseToPlayer = responseFunc;
    }

    public void SetExpectedResponse(string response) {
        responseFromPlayer = response;
    }
}
