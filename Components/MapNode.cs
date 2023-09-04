using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    internal string NodeType;
    internal int index;
    private RawImage? rawImage;

    internal void SetSprite(Texture texture)
    {
        rawImage ??= GetComponent<RawImage>();

        if (rawImage == null)
        {
            Log("No Image was defined.");
            return;
        }

        rawImage.texture = texture;
    }
}