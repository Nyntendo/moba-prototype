using UnityEngine;
using System.Collections;

public class CursorController : MonoBehaviour
{

    public Texture2D normalCursor;
    public Texture2D normalCursorClicked;

    public Texture2D targetCursor;
    public Vector2 hotSpot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

	void Start ()
    {
        Cursor.SetCursor(normalCursor, hotSpot, cursorMode);
	}

    void Update()
    {
        if (Input.GetButtonDown("Move") || Input.GetButtonDown("Target"))
        {
            Cursor.SetCursor(normalCursorClicked, hotSpot, cursorMode);
        }

        if (Input.GetButtonUp("Move") || Input.GetButtonUp("Target"))
        {
            Cursor.SetCursor(normalCursor, hotSpot, cursorMode);
        }
    }
}
