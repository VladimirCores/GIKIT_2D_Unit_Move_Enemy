using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    public UnityEvent<Collider2D> onTriggerEnter;

    public bool hasListeners {
        get { return onTriggerEnter != null; }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("> Bullet -> OnTriggerEnter2D: " + collision);
        if (hasListeners) onTriggerEnter.Invoke(this.gameObject.GetComponent<Collider2D>());
        Destroy(collision.gameObject);
    }

    void OnBecameInvisible()
    {
        Debug.Log("> Bullet -> OnBecameInvisible");
        Destroy(this.gameObject);
    }

    void OnDestroy() {
        Debug.Log("> Bullet -> OnDestroy");
        if (hasListeners) onTriggerEnter.RemoveAllListeners();
    }
}
