using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct PlayerResponse {
    public string responseStr;
    public PlayerPath playerPath;
    public bool pathInOrder;

    public PlayerResponse(string response) {
        responseStr = response;
        playerPath = null;
        pathInOrder = false;
    }

    public PlayerResponse(PlayerPath path) {
        playerPath = path;
        responseStr = string.Empty;
        pathInOrder = false;
    }

    public PlayerResponse(PlayerPath path, bool inOrder) {
        playerPath = path;
        responseStr = string.Empty;
        pathInOrder = inOrder;
    }
}