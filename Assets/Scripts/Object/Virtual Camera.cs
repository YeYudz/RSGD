using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCamera : MonoBehaviour
{
    //引用虚拟摄像机
    CinemachineVirtualCamera VCamera;
    //场景上的玩家物体位置
    Transform player;

    private IEnumerator Start()
    {
        VCamera = GetComponent<CinemachineVirtualCamera>();

        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                VCamera.Follow = player;
                yield break;
            }

            yield return new WaitForSeconds(0.1f); // 每 0.1s 查一次
        }
    }
    
}
