using OpenAI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TransformData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
}

[System.Serializable]
public class GameObjectData
{
    public string Name;
    public TransformData Transform;
    public Vector3 Forward;
    public List<string> Components;
}

[System.Serializable]
public class SceneData
{
    public List<GameObjectData> GameObjects;
}

[RequireComponent(typeof(AudioSource))]
public class LLMAssistant : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private Text message;
    [SerializeField] private Dropdown dropdown;
    private AudioSource audioSource;

    private readonly string fileName = "output.wav";
    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Color dftColor = new Color(0.2f, 0.6f, 0.6f, 1.0f);
    private Color speakingColor = new Color(0.8f, 0.5f, 0.6f, 1.0f);

    private AudioClip clip;
    private bool isRecording;
    public UnityEvent onAudioStop;
    private bool wasPlaying;
    private OpenAIApi openai;
    private CancellationTokenSource token = new CancellationTokenSource();
    int micIndex = 0;
    private List<string> mics = new List<string>();

    private void Awake()
    {
        var apiKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
            openai = new OpenAIApi(apiKey);
    }

    private void Start()
    {
        try
        {
            if (openai == null)
                Debug.LogError("LLMAssistant: set environment variable OPENAI_API_KEY before running (do not commit API keys).");

            audioSource = GetComponent<AudioSource>();
            
            // Find renderer
            FindRenderer();
            
            // Set initial color
            SetRendererColor(dftColor);
            
            // Setup microphones
            SetupMicrophones();
            
            // Initialize UI
            InitializeUI();
            
            // Setup events
            if (onAudioStop == null)
                onAudioStop = new UnityEvent();
                
            onAudioStop.AddListener(() =>
            {
                SetRendererColor(dftColor);
            });
            
            wasPlaying = audioSource.isPlaying;
            StartCoroutine(MonitorAudioSource());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in Start(): {e.Message}");
        }
    }
    
    private void FindRenderer()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        if (meshRenderer == null)
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    private void SetupMicrophones()
    {
        foreach (var device in Microphone.devices)
        {
            mics.Add(device);
        }
        micIndex = PlayerPrefs.GetInt("user-mic-device-index", 0);
    }
    
    private void InitializeUI()
    {
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(StartRecording);
        }
        
        if (dropdown != null)
        {
            dropdown.options.Clear();
            foreach (var device in mics)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(OnMicrophoneChanged);
            dropdown.SetValueWithoutNotify(micIndex);
        }
        
        if (message != null)
        {
            message.text = "Ready";
        }
    }
    
    private void OnMicrophoneChanged(int index)
    {
        PlayerPrefs.SetInt("user-mic-device-index", index);
        micIndex = index;
    }

    [ContextMenu("StartRecording")]
    public void StartRecording()
    {
        if (isRecording)
        {
            EndRecording();
            return;
        }
        
        isRecording = true;
        SetRendererColor(new Color(0.0f, 1.0f, 1.0f, 0.8f));
        
        if (message != null)
            message.text = "Recording...";

        if (mics.Count > 0 && micIndex < mics.Count)
        {
            clip = Microphone.Start(mics[micIndex], false, 10, 44100);
        }
    }

    [ContextMenu("EndRecording")]
    public async void EndRecording()
    {
        if (openai == null)
        {
            isRecording = false;
            SetRendererColor(dftColor);
            if (message != null)
                message.text = "Set OPENAI_API_KEY in your environment.";
            return;
        }

        isRecording = false;
        SetRendererColor(dftColor);

        if (message != null)
            message.text = "Transcripting...";

        Microphone.End(null);
        
        if (clip == null) return;
        
        byte[] data = SaveWav.Save(fileName, clip);

        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            Model = "whisper-1",
            Language = "en"
        };
        
        var res = await openai.CreateAudioTranscription(req);
        
        if (string.IsNullOrEmpty(res.Text.Trim()))
        {
            if (message != null)
                message.text = "Sorry, I didn't get that.";
            return;
        }

        var msg = new List<ChatMessage>() {
            new ChatMessage()
            {
                Role = "system",
                Content = "You are a friendly cat. Keep responses under 50 words. Use cat sounds like meow or purr."
            },
            new ChatMessage()
            {
                Role = "user",
                Content = res.Text
            }
        };

        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = msg
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var responseMessage = completionResponse.Choices[0].Message;
            TextToSpeech(responseMessage.Content.Trim());
        }
    }

    private async void TextToSpeech(string text)
    {
        if (message != null)
            message.text = text;
        
        var request = new CreateTextToSpeechRequest
        {
            Input = text,
            Model = "tts-1",
            Voice = "nova"
        };
        
        var response = await openai.CreateTextToSpeech(request);

        if (response.AudioClip)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
                
            audioSource.PlayOneShot(response.AudioClip);
            SetRendererColor(speakingColor);
        }
    }

    private void SetRendererColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
        else if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.material.color = color;
        }
    }

    private IEnumerator MonitorAudioSource()
    {
        while (true)
        {
            if (wasPlaying && !audioSource.isPlaying)
            {
                onAudioStop?.Invoke();
            }

            wasPlaying = audioSource.isPlaying;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDestroy()
    {
        token?.Cancel();
    }
}