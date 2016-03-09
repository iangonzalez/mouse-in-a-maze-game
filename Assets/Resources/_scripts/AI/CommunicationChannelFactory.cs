using System;
using UnityEngine;

/// <summary>
/// Factory class for making all different types of communcation channels.
/// Based on unity's resource.load function + prefab instantiation
/// </summary>
public static class CommunicationChannelFactory {
    private static TextCommunicationChannel twoWayCommChannelPrefab;
    private static OneWayTextCommunication oneWayCommChannelPrefab;

    private static GameObject InstantiatePrefabAtPath(string path) {
        return UnityEngine.Object.Instantiate(Resources.Load(path)) as GameObject;
    }

    public static CommunicationChannel Make2WayTextChannel() {
        var commObj = InstantiatePrefabAtPath("prefabs/TextCommChannel");
        if (commObj == null) {
            Debug.Log("prefab instantiation in factory is null");
            return null;
        }
        else {
            return commObj.GetComponent<TextCommunicationChannel>();
        }
    }

    public static CommunicationChannel MakeOneWayTextChannel() {
        var commObj = InstantiatePrefabAtPath("prefabs/OneWayCommChannel");
        if (commObj == null) {
            Debug.Log("prefab instantiation in factory is null");
            return null;
        }
        else {
            return commObj.GetComponent<OneWayTextCommunication>();
        }
    }

    public static CommunicationChannel MakeRoomExitPathChannel() {
        var commObj = InstantiatePrefabAtPath("prefabs/RoomExitPathCommChannel");
        if (commObj == null) {
            Debug.Log("prefab instantiation in factory is null");
            return null;
        }
        else {
            return commObj.GetComponent<RoomExitPathCommChannel>();
        }
    }

    public static CommunicationChannel MakeOneWayTimedChannel() {
        var commObj = InstantiatePrefabAtPath("prefabs/OneWayTimedComm");
        if (commObj == null) {
            Debug.Log("prefab instantiation in factory is null");
            return null;
        }
        else {
            return commObj.GetComponent<OneWayTimedCommChannel>();
        }
    }
}

