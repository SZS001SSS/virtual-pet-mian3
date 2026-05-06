using UnityEngine;
using UnityEngine.UI;

public class TalkingCatSetup : MonoBehaviour
{
    public GameObject catModel; 
    
    void Start()
    {
        if (catModel == null)
        {
            catModel = GameObject.Find("Cat Lite");
            Debug.Log("Trying to find Cat Lite...");
        }
        
        if (catModel == null)
        {
            Debug.LogError("找不到猫模型！请在Inspector中指定猫模型。");
            return;
        }
        
        Debug.Log("找到猫模型：" + catModel.name);
        SetupCat();
    }
    
    void SetupCat()
    {
        Debug.Log("开始设置猫咪...");
        
        // 1. 添加AudioSource
        if (!catModel.GetComponent<AudioSource>())
        {
            catModel.AddComponent<AudioSource>();
            Debug.Log("添加了AudioSource组件");
        }
        
        // 2. 添加LLMAssistant
        LLMAssistant llmAssistant = catModel.GetComponent<LLMAssistant>();
        if (llmAssistant == null)
        {
            llmAssistant = catModel.AddComponent<LLMAssistant>();
            Debug.Log("添加了LLMAssistant组件");
        }
        
        // 3. 创建UI组件来解决LLMAssistant的引用问题
        CreateUIComponents(llmAssistant);
        
        Debug.Log("猫咪设置完成！");
    }
    
    void CreateUIComponents(LLMAssistant llmAssistant)
    {
        Debug.Log("创建UI组件...");
        
        // 创建Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // 创建Button
        GameObject buttonGO = new GameObject("RecordButton");
        buttonGO.transform.SetParent(canvasGO.transform);
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.5f, 0.5f, 0.5f);
        Button button = buttonGO.AddComponent<Button>();
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.9f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.9f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // 为Button添加文本
        GameObject buttonTextGO = new GameObject("ButtonText");
        buttonTextGO.transform.SetParent(buttonGO.transform);
        Text buttonText = buttonTextGO.AddComponent<Text>();
        buttonText.text = "Record";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        
        RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // 创建Text
        GameObject textGO = new GameObject("MessageText");
        textGO.transform.SetParent(canvasGO.transform);
        Text messageText = textGO.AddComponent<Text>();
        messageText.text = "Waiting...";
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = 24;
        messageText.color = Color.white;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 50);
        
        // 创建Dropdown
        GameObject dropdownGO = new GameObject("Dropdown");
        dropdownGO.transform.SetParent(canvasGO.transform);
        Image dropdownImage = dropdownGO.AddComponent<Image>();
        dropdownImage.color = new Color(0.8f, 0.8f, 0.8f);
        Dropdown dropdown = dropdownGO.AddComponent<Dropdown>();
        
        RectTransform dropdownRect = dropdownGO.GetComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.5f, 0.8f);
        dropdownRect.anchorMax = new Vector2(0.5f, 0.8f);
        dropdownRect.anchoredPosition = Vector2.zero;
        dropdownRect.sizeDelta = new Vector2(200, 30);
        
        // 4. 使用反射设置LLMAssistant的私有字段
        SetLLMAssistantFields(llmAssistant, button, messageText, dropdown);
        
        Debug.Log("UI组件创建完成!");
    }
    
    void SetLLMAssistantFields(LLMAssistant llmAssistant, Button button, Text messageText, Dropdown dropdown)
    {
        Debug.Log("设置LLMAssistant的字段...");
        
        // 使用反射设置私有字段
        System.Type type = typeof(LLMAssistant);
        
        // 设置recordButton
        var recordButtonField = type.GetField("recordButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (recordButtonField != null)
        {
            recordButtonField.SetValue(llmAssistant, button);
            Debug.Log("设置recordButton成功");
        }
        
        // 设置message
        var messageField = type.GetField("message", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (messageField != null)
        {
            messageField.SetValue(llmAssistant, messageText);
            Debug.Log("设置message成功");
        }
        
        // 设置dropdown
        var dropdownField = type.GetField("dropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (dropdownField != null)
        {
            dropdownField.SetValue(llmAssistant, dropdown);
            Debug.Log("设置dropdown成功");
        }
    }
}