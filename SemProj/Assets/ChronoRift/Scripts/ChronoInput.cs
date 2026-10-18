using UnityEngine;

public static class ChronoInput
{
    public static float GetHorizontal()
    {
        float h = 0f;
        // Legacy input check (works reliably in Unity 6 when activeInputHandler is Both or legacy)
        try
        {
            h = Input.GetAxisRaw("Horizontal");
        }
        catch { }

        // Also check direct KeyCode fallback
        if (Mathf.Approximately(h, 0f))
        {
            try
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            }
            catch { }
        }

        // New Input System fallback if available
        #if ENABLE_INPUT_SYSTEM
        if (Mathf.Approximately(h, 0f))
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            }
        }
        #endif

        return Mathf.Clamp(h, -1f, 1f);
    }

    public static bool GetJumpDown()
    {
        try
        {
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                return true;
        }
        catch
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    return true;
            }
            catch { }
        }

        #if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                return true;
        }
        #endif

        return false;
    }

    public static bool GetJumpUp()
    {
        try
        {
            if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                return true;
        }
        catch
        {
            try
            {
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                    return true;
            }
            catch { }
        }

        #if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame)
                return true;
        }
        #endif

        return false;
    }

    public static bool GetInteractDown()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.E)) return true;
        }
        catch { }

        #if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.eKey.wasPressedThisFrame) return true;
        }
        #endif

        return false;
    }

    public static bool GetPauseDown()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.Escape)) return true;
        }
        catch { }

        #if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.escapeKey.wasPressedThisFrame) return true;
        }
        #endif

        return false;
    }
}
