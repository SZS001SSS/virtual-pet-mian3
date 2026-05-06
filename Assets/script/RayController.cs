using UnityEngine;


public class RayController : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // 控制器上的射线发射器
    public PetController petController; // 控制小猫的脚本
    public LayerMask raycastLayers; // 射线检测的地面层

    void Update()
    {
        if (rayInteractor != null && petController != null)
        {
            // 检测射线是否命中物体
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                // 检查命中的物体是否在目标层（地面）
                if (((1 << hit.collider.gameObject.layer) & raycastLayers) != 0)
                {
                    // 生成一个目标点，保持和猫的高度一致（防止猫浮空）
                    Vector3 target = hit.point;
                    target.y = petController.transform.position.y;

                    // 通知小猫去目标点
                    petController.MoveTo(target);
                }
            }
        }
    }
}
