using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

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

    public static IEnumerable<string> GetFileNames(string subpath) {
        DirectoryInfo relevantDir = new DirectoryInfo(Application.dataPath + "/Resources/" + linesDirectory + subpath);
        foreach (FileInfo file in relevantDir.GetFiles()) {
            yield return file.Name;
        }
    }

    public static IEnumerable<GenericTextInterchange> ParseAllTextInterchangesInDir(string subpath) {
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(linesDirectory + subpath);
        foreach (var textAsset in textAssets) {
            if (textAsset != null) {
                yield return ParseGenericTextInterchangeFromText(textAsset.text);
            }
            
        }
    }

    public static GenericTextInterchange ParseGenericTextInterchangeFromFile(string subpath) {
        return ParseGenericTextInterchangeFromText(GetAllTextByPath(subpath));
    }

    public static GenericTextInterchange ParseGenericTextInterchangeFromText(string text) {
        string[] interchangePieces = text.Split(new string[] { "%%" + Environment.NewLine }, StringSplitOptions.None);

        if (interchangePieces.Length != 4) {
            return null;
        }

        string question = interchangePieces[0];
        string expectedResponse = interchangePieces[1];

        Func<bool, string> getAiResponse =
            (b) =>  (b ? interchangePieces[2] : interchangePieces[3]);


        GenericTextInterchange interchange = new GenericTextInterchange(AIAlignmentState.Neutral);

        interchange.SetExpectedResponse(expectedResponse);
        interchange.SetQuestionAndResponse(question, getAiResponse);

        return interchange;                    
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

    public static string KillChildText() {
        return GetAllTextByPath("requests/text_requests/Friendly/moral_request");
    }

    public static string KillChildResponse(bool obeyed) {
        if (obeyed) {
            return "Good. Very good.";
        }
        else {
            return "Charlie thanks you.\nI hope you aren't curious about what the reward was?\nNow you'll never know.";
        }
    }

    public static string FeedBeastText() {
        return GetAllTextByPath("requests/text_requests/Friendly/beast_request");
    }

    public static string FeedBeastResponse(bool obeyed) {
        if (obeyed) {
            return "Good. The beast's strength is growing.\nSoon these doors will not be enough to stop him.";
        }
        else {
            return "If the beast is not fed, it will die.\nAnd then what - or who? - will take its place?";
        }
    }

    public static string LengthenHallwaysText = "Due to your earlier transgressions, the length of the hallways has increased.\nNote that I am not averse to further lengthening.\nEventually, the hallways will be miles long and you will traverse them in darkness.";
}

