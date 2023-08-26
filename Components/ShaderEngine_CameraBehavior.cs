using UnityEngine;

namespace BTDAdventure.Components;

// https://github.com/emeryllium/ShaderEngine/blob/master/ShaderEngine_CameraBehavior.cs W
public class ShaderEngine_CameraBehavior : MonoBehaviour
{
    public float radius = 0.69f;
    public int qualityIterations = 3;
    public int filter = 0;

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (BTDAdventure.blurMat == null)
            BTDAdventure.blurMat = Instantiate(LoadAsset<Material>("BlurMat"));

        if (BTDAdventure.blurMat == null)
        {
            Graphics.Blit(src, dst);
            return;
        }

        float widthModification = 1.0f / (1.0f * (1 << filter));

        BTDAdventure.blurMat.SetVector("_Param", new Vector4(radius * widthModification, -radius * widthModification, 0f, 0f));
        src.filterMode = FilterMode.Bilinear;

        int rendertextureWidth = src.width >> filter;
        int rendertextureHeight = src.height >> filter;

        RenderTexture rendertexture = RenderTexture.GetTemporary(rendertextureWidth, rendertextureHeight, 0, src.format);

        rendertexture.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, rendertexture, BTDAdventure.blurMat, 0);

        for (int i = 0; i < qualityIterations; i++)
        {
            float iterationOffset = i * 1.0f;
            BTDAdventure.blurMat.SetVector("_Param", new Vector4(radius * widthModification + iterationOffset, -radius * widthModification - iterationOffset, 0.0f, 0.0f));
            RenderTexture rendertexturetemp = RenderTexture.GetTemporary(rendertextureWidth, rendertextureHeight, 0, src.format);
            rendertexturetemp.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rendertexture, rendertexturetemp, BTDAdventure.blurMat, 1);
            RenderTexture.ReleaseTemporary(rendertexture);
            rendertexture = rendertexturetemp;
            rendertexturetemp = RenderTexture.GetTemporary(rendertextureWidth, rendertextureHeight, 0, src.format);
            rendertexturetemp.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rendertexture, rendertexturetemp, BTDAdventure.blurMat, 2);
            RenderTexture.ReleaseTemporary(rendertexture);
            rendertexture = rendertexturetemp;
        }
        Graphics.Blit(rendertexture, dst);
        RenderTexture.ReleaseTemporary(rendertexture);
    }
}
