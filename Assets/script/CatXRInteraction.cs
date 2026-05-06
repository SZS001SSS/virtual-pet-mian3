using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CatXRInteraction : MonoBehaviour
{
    private LLMAssistant llmAssistant;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    
    void Start()
    {
        // 获取LLMAssistant组件
        llmAssistant = GetComponent<LLMAssistant>();
        
        // 添加XR交互组件
        interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        
        // 如果没有碰撞体，添加一个
        if (!GetComponent<Collider>())
        {
            gameObject.AddComponent<BoxCollider>();
        }
        
        // 绑定选择事件
        interactable.selectEntered.AddListener(OnCatSelected);
        
        Debug.Log("XR Interaction setup complete for cat");
    }
    
    public void OnCatSelected(SelectEnterEventArgs args)
    {
        Debug.Log("Cat selected by XR controller!");
        
        if (llmAssistant != null)
        {
            // 切换录音状态
            llmAssistant.StartRecording();
        }
    }
}