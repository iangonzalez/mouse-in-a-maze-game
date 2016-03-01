using System;
using UnityEngine;

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
        else
            return commObj.GetComponent<TextCommunicationChannel>();
    }

    public static CommunicationChannel MakeOneWayTextChannel() {
        var commObj = InstantiatePrefabAtPath("prefabs/OneWayCommChannel");
        if (commObj == null) {
            Debug.Log("prefab instantiation in factory is null");
            return null;
        }
        else
            return commObj.GetComponent<OneWayTextCommunication>();
    }
}

