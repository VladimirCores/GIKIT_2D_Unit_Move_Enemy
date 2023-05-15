using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public UnityEvent<Collider2D> onTriggerEnter;

    public GameObject bulletPrefab;
    public float speed = 5.0f;

    private LineRenderer _lineRenderer;

    public void Start()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.gray;
        _lineRenderer.endColor = Color.red;
    }

     private Vector3 _initialPosition;
     private Vector3 _currentPosition;
     private Vector3 _mousePosition;
     private Vector3 _direction;

     public void Update()
     {
        _mousePosition = GetCurrentMousePosition().GetValueOrDefault();
        _initialPosition = this.transform.position;
        _initialPosition.z = 0;
        _mousePosition.z = 0;
        Vector3[] positions = new Vector3[2];
        positions[0] = _initialPosition;
        positions[1] = _mousePosition;
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);

        if (Input.GetKeyDown("space"))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            Vector2 myPos = new Vector2(transform.position.x, transform.position.y + 1);
            Vector2 direction = _mousePosition - _initialPosition;
            direction.Normalize();
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            GameObject bullet = (GameObject)Instantiate(bulletPrefab, _initialPosition, rotation);
            bullet.GetComponent<Rigidbody2D>().velocity = direction * speed;
            bullet.GetComponent<Bullet>().onTriggerEnter.AddListener(OnTriggerEnter2D);
        }
     }

     private Vector3? GetCurrentMousePosition()
     {
        Vector3 screenPoint = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(screenPoint) ;
     }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("> Unit -> OnTriggerEnter2D: " + collision);
        if(onTriggerEnter != null) {
            onTriggerEnter.Invoke(collision);
        }
    }
}
