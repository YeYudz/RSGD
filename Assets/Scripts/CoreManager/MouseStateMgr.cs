using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseStateMgr : BaseManager<MouseStateMgr>
{
    private static int needMouseCount = 0;
    public bool IsMouseRequired() => needMouseCount > 0;
    public void RequireMouse()
    {
        needMouseCount++;
        Apply();
    }

    public void ReleaseMouse()
    {
        needMouseCount = Mathf.Max(0, needMouseCount - 1);
        Apply();
    }

    public void Reset()
    {
        needMouseCount = 0;
        Apply();
    }

    private void Apply()
    {
        if (needMouseCount > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
