using UnityEngine;


public static class GameLinesTextGetter {
    private static string linesDirectory = "GameLines/";

    private static string GetAllTextByPath(string subPath) {
        TextAsset textAsset = Resources.Load(linesDirectory + subPath) as TextAsset;
        return textAsset.text;
    }

    private static string[] GetAllLinesByPath(string subPath) {
        TextAsset responses = Resources.Load(linesDirectory + subPath) as TextAsset;
        string[] lines = responses.text.Split(new char[] { '\n' });
        return lines;
    }

    public static string OpeningMonologue() {
        return GetAllTextByPath("beginning/opening_monologue");
    }

    public static string FirstRequest() {
        return GetAllTextByPath("beginning/first_request");
    }

    public static string FirstResponse(bool isPositive = true) {
        string locationStr = "responses/" + (isPositive ? "positive" : "negative") + "/first_response";
        return GetAllTextByPath(locationStr);
    }

    public static string RandomResponse(bool isPositive = true) {
        string locationStr = "responses/" + (isPositive ? "positive" : "negative") + "/responses";
        string[] lines = GetAllLinesByPath(locationStr);
        return lines[UnityEngine.Random.Range(0, lines.Length)];
    }

    public static string RandomRequestIntro() {
        string locationStr = "requests/request_intros";
        string[] lines = GetAllLinesByPath(locationStr);
        return lines[UnityEngine.Random.Range(0, lines.Length)];
    }

    public static string RandomTextRequest(AIAlignmentState state = AIAlignmentState.Neutral) {
        string[] lines = GetAllLinesByPath("requests/text_requests/" + state.ToString() + "/requests");
        return lines[UnityEngine.Random.Range(0, lines.Length)];
    }

    public static string FalseHintText = "Have a signpost. It might point in the right direction.\nIt might not.";
    public static string SpinMazeText = "Let's play a fun game! It's called 'I spin the maze, you get lost.'";
    public static string BeastIsNearText = "When I get angry, strange things start to happen.\nBeasts appear where before there were none.";

    public static string ShortcutText(bool shortcutPossible) {
        if (shortcutPossible) {
            return "You've been such a good test subject so far.\nHere, this new passage is a shortcut to the exit.";
        }
        else {
            return "You've been such a good test subject so far.\nI tried to find a shortcut from this room, but you're already on the right track, my friend.";
        }
    }

    public static string LongcutText(bool longcutPossible) {
        if (longcutPossible) {
            return "You were getting too close.\nThis new passage will make your route take longer. Maybe you'll have time to think about what you've done.";
        }
        else {
            return "I was going to lengthen your path to the exit.\nBut it looks like you're already pretty lost on your own. Ha!";
        }
    }
    
}

