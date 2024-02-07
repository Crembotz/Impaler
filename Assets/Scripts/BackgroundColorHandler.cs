using UnityEngine;

public class BackgroundColorHandler : MonoBehaviour
{
    public bool colorChanged = false;

    public void ColorChangedFunc()
    {
        colorChanged = true;
    }
}
