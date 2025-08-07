using UnityEngine;


using Fusion;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewMesh : NetworkBehaviour
{
    [Header("Field of View Settings")]
    public float viewRadius = 15f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public int meshResolution = 1; // Số lượng tia ray mỗi độ
    public LayerMask obstacleMask;
    public Material fovMaterial; // Vật liệu có alpha/mờ

    [Header("Target Detection")]
    public LayerMask targetMask; // Layer các đối tượng cần phát hiện

    MeshFilter viewMeshFilter;
    Mesh viewMesh;

    void Start()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        if (fovMaterial != null)
            GetComponent<MeshRenderer>().material = fovMaterial;
    }

    void LateUpdate()
    {
        // Chỉ hiện mesh vùng nhìn của chính mình
        if (Object.HasInputAuthority)
        {
            if (viewMeshFilter != null && viewMeshFilter.gameObject.activeSelf == false)
                viewMeshFilter.gameObject.SetActive(true);
            DrawFieldOfView();
            DetectTargets();
        }
        else
        {
            if (viewMeshFilter != null && viewMeshFilter.gameObject.activeSelf == true)
                viewMeshFilter.gameObject.SetActive(false);
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        var viewPoints = new System.Collections.Generic.List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo viewCast = ViewCast(angle);
            viewPoints.Add(viewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    void DetectTargets()
    {
        if (!Object.HasInputAuthority) return;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (var target in targetsInViewRadius)
        {
            // Kiểm tra nếu là player online
            var player = target.GetComponent<PlayerControllerRPC>();
            if (player != null && !player.Object.HasInputAuthority)
            {
                Vector3 dirToTarget = (player.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
                if (angleToTarget < viewAngle / 2f)
                {
                    float distToTarget = Vector3.Distance(transform.position, player.transform.position);
                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        player.SetVisibleForOther(true);
                    }
                    else
                    {
                        player.SetVisibleForOther(false);
                    }
                }
                else
                {
                    player.SetVisibleForOther(false);
                }
            }
            else
            {
                // Đối tượng thường
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
                if (angleToTarget < viewAngle / 2f)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        Renderer rend = target.GetComponent<Renderer>();
                        if (rend != null)
                            rend.enabled = true;
                    }
                    else
                    {
                        Renderer rend = target.GetComponent<Renderer>();
                        if (rend != null)
                            rend.enabled = false;
                    }
                }
                else
                {
                    Renderer rend = target.GetComponent<Renderer>();
                    if (rend != null)
                        rend.enabled = false;
                }
            }
        }
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }
}
