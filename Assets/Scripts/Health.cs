using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Color hoverColor;
    private Color _originalColor;

    public float Heal
    {
        get { return 0.5f; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _originalColor = this.GetComponent<SpriteRenderer>().color;
    }

    void OnMouseOver()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        Debug.Log("Mouse is over GameObject.");
        this.GetComponent<SpriteRenderer>().color = hoverColor;
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        Debug.Log("Mouse is no longer on GameObject.");
        this.GetComponent<SpriteRenderer>().color = _originalColor;
    }
}
