using UnityEngine;

public class PetController : MonoBehaviour
{
    public float moveSpeed = 1.5f; // 控制小猫走路的速度
    private Vector3 targetPosition; // 小猫要走向的目标位置
    private bool hasTarget = false; // 小猫是否有目标点

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true; // 设置了目标，小猫开始动
    }

    void Update()
    {
        if (hasTarget)
        {
            // 计算方向并朝目标前进
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 同时转身朝向目标方向（平滑旋转）
            if (direction != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
            }

            // 如果靠近目标点，就停下
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
                hasTarget = false;
        }
    }
}
