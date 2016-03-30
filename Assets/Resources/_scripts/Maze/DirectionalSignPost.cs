using UnityEngine;

public class DirectionalSignPost : MonoBehaviour {
    private GameObject signpost;

    void Start() {
        signpost = gameObject;
    }

    void FixedUpdate() {
        if (signpost.transform.localPosition.y < 0.5) {
            signpost.transform.localPosition += new Vector3(0, 0.05f, 0);
        }
    }
}

