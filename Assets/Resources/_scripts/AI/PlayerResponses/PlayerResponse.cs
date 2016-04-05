public struct PlayerResponse {
    public string responseStr;
    public PlayerPath playerPath;
    public bool pathInOrder;
    public bool playerMoved;

    public PlayerResponse(string response) {
        responseStr = response;
        playerPath = null;
        pathInOrder = false;
        playerMoved = false;
    }

    public PlayerResponse(bool moved) {
        responseStr = string.Empty;
        playerPath = null;
        pathInOrder = false;
        playerMoved = moved;
    }

    public PlayerResponse(PlayerPath path) {
        playerPath = path;
        responseStr = string.Empty;
        pathInOrder = false;
        playerMoved = false;
    }

    public PlayerResponse(PlayerPath path, bool inOrder) {
        playerPath = path;
        responseStr = string.Empty;
        pathInOrder = inOrder;
        playerMoved = false;
    }
}