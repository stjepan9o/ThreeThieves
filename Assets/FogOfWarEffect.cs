using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FogOfWarEffect : MonoBehaviour
{
    public Shader fogShader;
    public Transform player1;
    public Transform player2;
    public float visionRadius = 8f;
    public float edgeSoftness = 2f;

    Material mat;
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
        mat = new Material(fogShader);
    }

   void OnRenderImage(RenderTexture src, RenderTexture dst)
{
    Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
    Matrix4x4 vp = gpuProj * cam.worldToCameraMatrix;
    mat.SetMatrix("_InverseVP", vp.inverse);

    mat.SetVector("_PlayerPos1", player1 ? player1.position : Vector3.one * 99999f);
    mat.SetFloat("_Radius1", player1 ? visionRadius : 0f);

    mat.SetVector("_PlayerPos2", player2 ? player2.position : Vector3.one * 99999f);
    mat.SetFloat("_Radius2", player2 ? visionRadius : 0f);

    mat.SetFloat("_EdgeSoftness", edgeSoftness);

    Graphics.Blit(src, dst, mat);
}
}