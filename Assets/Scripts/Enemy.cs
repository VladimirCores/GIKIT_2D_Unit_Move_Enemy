using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Vector2 _targetPosition;
    public float Damage
    {
        get { return 0.25f; }
    }

    // Start is called before the first frame update
    void Start()
    {
        generateTargetPosition();
    }

    void generateTargetPosition() {
        _targetPosition = Main.findRandomPositionOnScreen(Camera.main);
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoveCompleted = Vector2.Distance(this.transform.position, _targetPosition) < 0.1;

        if (isMoveCompleted) {
            // Debug.Log("> isMoveCompleted = " + isMoveCompleted);
            generateTargetPosition();
        }

        this.transform.position = Vector2.Lerp(this.transform.position, _targetPosition, Time.deltaTime / 2);
    }
}
