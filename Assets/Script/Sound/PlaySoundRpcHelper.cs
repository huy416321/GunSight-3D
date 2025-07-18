using UnityEngine;
using Fusion;
namespace SmallHedge.SoundManager
{
    public class PlaySoundRpcHelper : NetworkBehaviour
    {
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_PlaySound(SoundType sound, float volume, Vector3 pos)
        {
            // Tạo AudioSource tạm thời tại vị trí pos
            var go = new GameObject("TempSound");
            go.transform.position = pos;
            var source = go.AddComponent<AudioSource>();
            source.spatialBlend = 1f; // 3D sound
            SoundManager.PlaySound(sound, source, volume);
            UnityEngine.Object.Destroy(go, 3f);
        }
    }
}
