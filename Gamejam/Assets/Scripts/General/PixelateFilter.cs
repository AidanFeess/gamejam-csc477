using UnityEngine;

[ExecuteInEditMode]
public class PixelateEffect : MonoBehaviour
{
    public Material pixelMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (pixelMaterial != null)
        {
            Graphics.Blit(src, dest, pixelMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
