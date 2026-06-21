using UnityEngine;

public class ReloadStateBehaviour : StateMachineBehaviour
{
    // 当状态退出时调用（即换弹动画播放完成时）
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 直接从 Animator 所在的物体上获取 PlayerObject 组件
        PlayerObject player = animator.GetComponent<PlayerObject>();

        if (player != null)
        {
            // 直接调用原来那个 ReloadEvent 方法
            player.ReloadEvent();
        }
    }
}
