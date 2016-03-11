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

}

