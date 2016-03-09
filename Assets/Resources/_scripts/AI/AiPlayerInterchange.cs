using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class AiPlayerInterchange {
    public PlayerResponse expectedResponse;

    protected bool isFirst = false;
    

    public AiPlayerInterchange() {
        expectedResponse = new PlayerResponse();
    }

    public AiPlayerInterchange(PlayerResponse expected) {
        expectedResponse = expected;
    }

    public AiPlayerInterchange(PlayerResponse expected, bool firstInterchange) {
        expectedResponse = expected;
        isFirst = firstInterchange;
    }

    public abstract bool CheckIfCorrectResponse(PlayerResponse response);
    public abstract string GetQuestionText();
    public abstract string GetResponseToPlayerText(bool responseIsPositive);
}

public class TextOnlyInterchange : AiPlayerInterchange {
    private string question;

    //idea: write the lines here as tuples: (question, expected answer)
    //this can then pick a random line and know the question and expected answer

    public TextOnlyInterchange() {
        expectedResponse = new PlayerResponse();
        string[] randQuestionAnswer = GameLinesTextGetter.RandomTextRequest().Split(new char[] { '\t' });
        question = randQuestionAnswer[0];
        expectedResponse.responseStr = randQuestionAnswer[1];
    }

    public override bool CheckIfCorrectResponse(PlayerResponse response) {
        return expectedResponse.responseStr == response.responseStr;
    }

    //todo: change these two methods to be text only specific
    public override string GetQuestionText() {
        return GameLinesTextGetter.RandomRequestIntro() + "\n" + question;
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        return GameLinesTextGetter.RandomResponse(isPositive: responseIsPositive);
    }
}


public abstract class PathInterchange : AiPlayerInterchange {

    public PathInterchange() {

    }

    public PathInterchange(PlayerResponse expected) {
        expectedResponse = expected;
    }

    public PathInterchange(PlayerResponse expected, bool firstInterchange) {
        expectedResponse = expected;
        isFirst = firstInterchange;
    }

    public override bool CheckIfCorrectResponse(PlayerResponse response) {
        if (response.pathInOrder) {
            return response.playerPath.ArePointsInCorrectOrder();
        }
        else {
            return response.playerPath.WereAllPointsTraversed();
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

    public TouchCornersInterchange(PlayerResponse response, bool first) : base(response, first) {

    }

    public TouchCornersInterchange(PlayerResponse response) : base(response) {

    }

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

    public float timeLocked = 10.0f;

    public override bool CheckIfCorrectResponse(PlayerResponse response) {
        return true;
    }

    public override string GetQuestionText() {
        return "I'm going to lock you in this room for " + timeLocked + " seconds. Have fun!";
    }

    public override string GetResponseToPlayerText(bool responseIsPositive) {
        return "Thanks for waiting! And no, I won't tell you why I did that.";
    }
}
