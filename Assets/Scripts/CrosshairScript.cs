using UnityEngine;

public class CrosshairScript : MonoBehaviour
{
    public Vector2 mouseWorldPosition { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouseWorldPosition;
    }
}
