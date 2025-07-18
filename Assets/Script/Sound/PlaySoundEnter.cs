//Author: Small Hedge Games
//Updated: 13/06/2024

using UnityEngine;

namespace SmallHedge.SoundManager
{
    public class PlaySoundEnter : StateMachineBehaviour
    {
        [SerializeField] private SoundType sound;
        [SerializeField, Range(0, 1)] private float volume = 1;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var netObj = animator.GetComponent<Fusion.NetworkObject>();
            if (netObj != null && netObj.HasInputAuthority)
            {
                // Gọi RPC phát âm thanh cho tất cả client
                var soundRpc = animator.GetComponent<SmallHedge.SoundManager.PlaySoundRpcHelper>();
                if (soundRpc != null)
                {
                    soundRpc.RPC_PlaySound(sound, volume, animator.transform.position);
                }
            }
        }
    }
}