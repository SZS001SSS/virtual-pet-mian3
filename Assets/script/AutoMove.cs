using UnityEngine;

public class AutoMove : MonoBehaviour
{
    // 移动速度和转向速度
    public float moveSpeed = 2f;
    public float rotateSpeed = 5f;

    // 目标位置
    private Vector3 targetPosition;
    // 是否正在移动
    private bool isMoving = true;

    void Start()
    {
        // 在游戏开始时，设置一个初始目标位置
        // 这里示例为当前朝向前方5个单位的位置
        targetPosition = transform.position + transform.forward * 5f;
    }

    void Update()
    {
        if (isMoving)
        {
            // 计算当前位置与目标位置之间的距离
            float distance = Vector3.Distance(transform.position, targetPosition);

            // 如果还没到目标位置，则向目标位置移动
            if (distance > 0.1f)
            {
                // 计算朝向目标的方向
                Vector3 direction = (targetPosition - transform.position).normalized;
                // 让物体平滑转向目标方向
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                // 移动物体
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                Debug.Log("Moving towards " + targetPosition);

            }
            else
            {
                // 到达目标后，可以选择停止移动，或者更新目标位置
                // 这里举例为更新目标位置为当前前方5个单位，使宠物不断向前移动
                targetPosition = transform.position + transform.forward * 5f;
            }
        }
    }
}
