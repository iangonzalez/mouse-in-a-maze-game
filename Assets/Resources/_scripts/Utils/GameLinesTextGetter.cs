using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class GameLinesTextGetter {
    private static string linesDirectory = Application.dataPath + "/GameLines/";

    private static string GetLinesByPath(string subPath) {
        return File.ReadAllText(linesDirectory + subPath);
    }

    public static string OpeningMonologue() {
        return GetLinesByPath("opening_monologue/opening.txt");
    }
}

