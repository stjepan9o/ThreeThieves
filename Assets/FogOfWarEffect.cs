using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FogOfWarEffect : MonoBehaviour
{
    public Shader fogShader;
    public float visionRadius = 8f;
    public float edgeSoftness = 2f;

    [Header("Trenutno aktivan lik (postavlja se automatski)")]
    public Transform activePlayer;

    Material mat;
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
        mat = new Material(fogShader);
    }

    public void SetActivePlayer(Transform player)
    {
        activePlayer = player;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 vp = gpuProj * cam.worldToCameraMatrix;
        mat.SetMatrix("_InverseVP", vp.inverse);

        mat.SetVector("_PlayerPos1", activePlayer ? activePlayer.position : Vector3.one * 99999f);
        mat.SetFloat("_Radius1", activePlayer ? visionRadius : 0f);

        // Drugi krug ugašen — koristimo samo jedan aktivni
        mat.SetVector("_PlayerPos2", Vector3.one * 99999f);
        mat.SetFloat("_Radius2", 0f);

        mat.SetFloat("_EdgeSoftness", edgeSoftness);

        Graphics.Blit(src, dst, mat);
    }
}