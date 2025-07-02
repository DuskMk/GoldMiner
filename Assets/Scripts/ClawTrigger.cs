using UnityEngine;

public class ClawTrigger : MonoBehaviour
{
    // 这个脚本需要知道它的“大脑”是谁，也就是ClawController
    public ClawController clawController;

    void Start()
    {
        // 如果在Inspector里没有手动指定，就自动往父级寻找
        if (clawController == null)
        {
            clawController = GetComponentInParent<ClawController>();
        }
    }

    // 这个脚本的核心任务就是监听碰撞
    void OnTriggerEnter2D(Collider2D other)
    {
        // 一旦发生碰撞，就把碰到的物体(other)信息，
        // 传递给ClawController去处理。
        clawController.HandleCollision(other);
    }
}