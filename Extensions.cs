using Il2Cpp;
using UnityEngine;

namespace BTDAdventure;

public static class Extensions
{
    public static NK_TextMeshProUGUI? UpdateText(this NK_TextMeshProUGUI text, object? content)
    {
        if (text != null)
            text.text = content == null ? "null" : content.ToString();
        else
            Log("The given text component is null.");
        return text;
    }

    public static Transform? SafeGetComponent<T>(this Transform? root, out T? component) where T : Component
    {
        if (root != null)
        {
            if (!root.TryGetComponent<T>(out component))
                Log($"No \'{typeof(T).Name}\' component was not found in the GameObject \'{root.gameObject.name}\'.");
        }
        else
            component = null;
        return root;
    }
}