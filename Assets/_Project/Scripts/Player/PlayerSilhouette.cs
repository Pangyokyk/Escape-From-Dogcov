using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSilhouette : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private LayerMask occlusionLayer;
    [SerializeField] private Material silhouetteMaterial;

    [Header("장애물 반투명")]
    [SerializeField] private float obstacleAlpha = 0.3f;

    private Camera mainCamera;
    private Renderer[] playerRenderers;
    private Material[][] originalMaterials;
    private bool isShowingSilhouette = false;

    // 장애물 관리
    private List<OccludedObstacle> currentOccluders = new List<OccludedObstacle>();

    private class OccludedObstacle
    {
        public Renderer renderer;
        public Material[] originalMaterials;
    }

    private void Start()
    {
        InitializeRenderers();
        FindCamera();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCamera();
    }

    private void FindCamera()
    {
        mainCamera = Camera.main;
    }

    private void InitializeRenderers()
    {
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[playerRenderers.Length][];

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalMaterials[i] = playerRenderers[i].materials;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // 이전 프레임 장애물 복구
        RestoreObstacles();

        Vector3 direction = transform.position - mainCamera.transform.position;
        float distance = direction.magnitude;

        // 모든 장애물 감지
        RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, direction, distance, occlusionLayer);

        if (hits.Length > 0)
        {
            // 플레이어 실루엣 ON
            if (!isShowingSilhouette)
            {
                ShowSilhouette(true);
            }

            // 장애물들 반투명
            foreach (RaycastHit hit in hits)
            {
                MakeObstacleTransparent(hit.collider);
            }
        }
        else
        {
            // 플레이어 실루엣 OFF
            if (isShowingSilhouette)
            {
                ShowSilhouette(false);
            }
        }
    }

    // === 플레이어 실루엣 ===
    private void ShowSilhouette(bool show)
    {
        isShowingSilhouette = show;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] == null) continue;

            if (show)
            {
                Material[] newMats = new Material[originalMaterials[i].Length + 1];
                originalMaterials[i].CopyTo(newMats, 0);
                newMats[newMats.Length - 1] = silhouetteMaterial;
                playerRenderers[i].materials = newMats;
            }
            else
            {
                playerRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    // === 장애물 반투명 ===
    private void MakeObstacleTransparent(Collider collider)
    {
        Renderer renderer = collider.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = collider.GetComponentInChildren<Renderer>();
        }
        if (renderer == null) return;

        // 이미 처리한 렌더러인지 확인
        foreach (OccludedObstacle existing in currentOccluders)
        {
            if (existing.renderer == renderer) return;
        }

        // 원본 저장
        OccludedObstacle occluded = new OccludedObstacle();
        occluded.renderer = renderer;
        occluded.originalMaterials = renderer.materials;

        // 새 Material로 반투명 처리
        Material[] newMats = new Material[renderer.materials.Length];
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            newMats[i] = new Material(renderer.materials[i]);
            SetMaterialTransparent(newMats[i]);
        }
        renderer.materials = newMats;

        currentOccluders.Add(occluded);
    }

    private void SetMaterialTransparent(Material mat)
    {
        // Standard Shader용
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        if (mat.HasProperty("_Color"))
        {
            Color color = mat.color;
            color.a = obstacleAlpha;
            mat.color = color;
        }
    }

    private void RestoreObstacles()
    {
        foreach (OccludedObstacle occluded in currentOccluders)
        {
            if (occluded.renderer != null)
            {
                // 투명 Material 삭제
                foreach (Material mat in occluded.renderer.materials)
                {
                    Destroy(mat);
                }
                // 원본 복구
                occluded.renderer.materials = occluded.originalMaterials;
            }
        }
        currentOccluders.Clear();
    }
}