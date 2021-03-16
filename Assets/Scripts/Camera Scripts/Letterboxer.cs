using UnityEngine;

public class Letterboxer : MonoBehaviour
{
    public Vector2 landscapeMinAspectRatio = new Vector2(4.0f, 3.0f);
    public Vector2 landscapeMaxAspectRatio = new Vector2(4.0f, 3.0f);
    public Vector2 portraitMinAspectRatio = new Vector2(3.0f, 4.0f);
    public Vector2 portraitMaxAspectRatio = new Vector2(3.0f, 4.0f);

    Camera cam = null;
    int screenWidth = 0;        // 0 forces camAspect to be set correctly on Awake.
    int screenHeight = 0;

    int effectiveScreenWidth = 0;
    int effectiveScreenHeight = 0;

    float camAspect = 1.0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Update();
    }

    void OnValidate()
    {
        // Just to avoid divide by zero and non-zero cam aspect ratios.
        if (landscapeMinAspectRatio.x < 0.01f) landscapeMinAspectRatio.x = 0.01f;
        if (landscapeMinAspectRatio.y < 0.01f) landscapeMinAspectRatio.y = 0.01f;
        if (landscapeMaxAspectRatio.x < 0.01f) landscapeMaxAspectRatio.x = 0.01f;
        if (landscapeMaxAspectRatio.y < 0.01f) landscapeMaxAspectRatio.y = 0.01f;

        if (portraitMinAspectRatio.x < 0.01f) portraitMinAspectRatio.x = 0.01f;
        if (portraitMinAspectRatio.y < 0.01f) portraitMinAspectRatio.y = 0.01f;
        if (portraitMaxAspectRatio.x < 0.01f) portraitMaxAspectRatio.x = 0.01f;
        if (portraitMaxAspectRatio.y < 0.01f) portraitMaxAspectRatio.y = 0.01f;
    }

    public float GetAspect()
    {
        return camAspect;
    }

    public float GetScreenWidth()
    {
        return effectiveScreenWidth;
    }

    public float GetScreenHeight()
    {
        return effectiveScreenHeight;
    }

    void Update()
    {
        if ((screenWidth != Screen.width) || (screenHeight != Screen.height))
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            if ((screenWidth > 0) && (screenHeight > 0))
                OnAspectChanged();
        }
    }

    void ComputeCameraAspect(float screenAspect)
    {
        bool landscape = screenAspect > 1.0f;
        if (landscape)
        {
            float landscapeMinAspect = landscapeMinAspectRatio.x / landscapeMinAspectRatio.y;
            float landscapeMaxAspect = landscapeMaxAspectRatio.x / landscapeMaxAspectRatio.y;
            camAspect = Mathf.Clamp(screenAspect, landscapeMinAspect, landscapeMaxAspect);
        }
        else
        {
            float portraitMinAspect = portraitMinAspectRatio.x / portraitMinAspectRatio.y;
            float portraitMaxAspect = portraitMaxAspectRatio.x / portraitMaxAspectRatio.y;
            camAspect = Mathf.Clamp(screenAspect, portraitMinAspect, portraitMaxAspect);
        }
    }

    void OnAspectChanged()
    {
        float screenAspect = (float)screenWidth / (float)screenHeight;
        ComputeCameraAspect(screenAspect);

        // If vertBars is false there will be unused horizontal space.
        bool vertBars = screenAspect > camAspect;
        Rect rect = cam.rect;
        float camWH = vertBars ? camAspect / screenAspect : screenAspect / camAspect;
        rect.width = vertBars ? camWH : 1.0f;
        rect.height = vertBars ? 1.0f : camWH;
        rect.x = vertBars ? (1.0f - camWH) * 0.5f : 0.0f;
        rect.y = vertBars ? 0.0f : (1.0f - camWH) * 0.5f;
        effectiveScreenWidth = (int)(rect.width * (float)screenWidth);
        effectiveScreenHeight = (int)(rect.height * (float)screenHeight);
        cam.rect = rect;
    }
}