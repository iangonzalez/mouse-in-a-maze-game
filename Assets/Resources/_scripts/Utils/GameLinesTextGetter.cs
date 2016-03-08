using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class GameLinesTextGetter {
    private static string linesDirectory = Application.dataPath + "/GameLines/";

    private static string GetAllTextByPath(string subPath) {
        return File.ReadAllText(linesDirectory + subPath);
    }

    private static string[] GetAllLinesByPath(string subPath) {
        return File.ReadAllLines(linesDirectory + subPath);
    }

    public static string OpeningMonologue() {
        return GetAllTextByPath("beginning/opening_monologue.txt");
    }

    public static string FirstRequest() {
        return GetAllTextByPath("requests/first_request.txt");
    }

    public static string FirstResponse(bool isPositive = true) {
        return GetAllTextByPath( "responses/" +  (isPositive ? "positive" : "negative") + "/first_response.txt");
    }

    public static string RandomPositiveResponse() {
        var lines = GetAllLinesByPath("responses/positive/responses.txt");
        return lines[UnityEngine.Random.Range(0, lines.Length)];
    }
    
}

