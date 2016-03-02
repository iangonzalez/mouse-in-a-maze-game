using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct PlayerResponse {
    public string responseStr;
    public PlayerPath playerPath;

    public PlayerResponse(string response) {
        responseStr = response;
        playerPath = null;
    }

    public PlayerResponse(PlayerPath path) {
        playerPath = path;
        responseStr = string.Empty;
    }
}