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

    public static IEnumerable<GenericTextInterchange> ParseAllTextInterchangesInDir(string subpath) {
        foreach (string text in GetAllTextInDir(subpath)) {
            if (text != null) {
                yield return ParseGenericTextInterchangeFromText(text);
            }
        }
    }

    public static GenericTextInterchange ParseGenericTextInterchangeFromFile(string subpath) {
        return ParseGenericTextInterchangeFromText(GetAllTextByPath(subpath));
    }

    public static GenericTextInterchange ParseGenericTextInterchangeFromText(string text) {
        string[] interchangePieces = text.Split(new string[] { "%%" + Environment.NewLine }, StringSplitOptions.None);

        if (interchangePieces.Length != 4) {
            Debug.LogError("The generic text interchange was not parsed correctly from the text: " + text);
            return null;
        }

        string question = interchangePieces[0];
        string expectedResponse = interchangePieces[1];

        Func<bool, string> getAiResponse;
        
        if (interchangePieces[2].Trim() == "RANDOM") {
            interchangePieces[2] = RandomResponse(true);
        }
        if (interchangePieces[3] == "RANDOM") {
            interchangePieces[3] = RandomResponse(false);
        }


        getAiResponse = (b) => (b ? interchangePieces[2] : interchangePieces[3]);

        GenericTextInterchange interchange = new GenericTextInterchange(AIAlignmentState.Neutral);

        interchange.SetExpectedResponse(expectedResponse);
        interchange.SetQuestionAndResponse(question, getAiResponse);

        return interchange;                    
    }

    public static IEnumerable<string> GetAllTextInDir(string subpath) {
        foreach (TextAsset textAsset in Resources.LoadAll<TextAsset>(linesDirectory + subpath)) {
            if (textAsset != null) {
                yield return textAsset.text;
            }
        }
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

    public static string LengthenHallwaysText = "Due to your earlier transgressions, the length of the hallways has increased.\nNote that I am not averse to further lengthening.\nEventually, the hallways will be miles long and you will traverse them in darkness.";

    public static string GiveMoreBreadcrumbsText = "I am a kind and gentle god - er, researcher.\nAs a reward for your obedience, you may use up to 10 extra breadcrumbs without them disappearing.";

    public static string DestroyYourBreadcrumbsText = "You broke my heart earlier.\nI thought we were friends.\nPerhaps you will learn why it's good to be my friend if I destroy all the breadcrumbds you've placed.";

    public static string ReduceBreadCrumbsText = "You haven't been very nice to me so far.\nNot cool, yo.\nI was kind enough to give you some breadcrumbs to mark your way in the maze.\nNow you will have fewer.";

    public static string GetEndingMonologue(AIAlignmentState state) {
        return GetAllTextByPath("ending/" + state.ToString() + "/ending_monologue_standard");
    }
}

