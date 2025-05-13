using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using System.IO;
public class ChatGPTManager : MonoBehaviour
{
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    private string currentPrompt = "";
    [Header("Prompt")]
    public string promptFileName = "behavior_tree_prompt.txt";

    [System.Serializable]
    public class BehaviorResult
    {
        public string behavior;
    }

    void Start()
    {
        LoadPromptFromFile(promptFileName);
        //AskChatGPT("Help me collect some resources.");
    }

    void Update()
    {

    }
    /// <summary>
    /// 向 ChatGPT 提交语音指令，返回结构化行为 JSON 字符串
    /// </summary>
    /// <param name="userText">Whisper 识别到的玩家语音文本</param>
    public async void AskChatGPT(string userText)
    {
        // 安全检查：如果当前 prompt 内容为空，发出警告
        if (string.IsNullOrEmpty(currentPrompt))
        {
            Debug.LogWarning("当前 Prompt 为空，请先调用 LoadPromptFromFile");
            return;
        }

        // 拼接完整输入文本（Prompt + 玩家语音）
        string fullInput = $"{currentPrompt}\n\nInput: \"{userText}\"\nOutput:";

        // 创建 ChatGPT 对话消息，角色是 user
        ChatMessage newMessage = new ChatMessage
        {
            Content = fullInput,
            Role = "user"
        };

        // 构造请求体，模型使用 gpt-3.5-turbo，消息列表只包含 prompt+输入
        var request = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { newMessage }
        };

        // OpenAI API 发起请求（异步）
        var response = await openAI.CreateChatCompletion(request);

        // 如果成功收到回复，并且有内容
        if (response.Choices != null && response.Choices.Count > 0)
        {
            //获取 GPT 的回复内容（行为 JSON）
            var chatResponse = response.Choices[0].Message;

            //控制台输出结构化结果（你可以把它传给 AI 行为系统）
            Debug.Log($"GPT返回: {chatResponse.Content}");
            ProcessGPTResponse(chatResponse.Content);
        }
    }


    public void LoadPromptFromFile(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Prompts", fileName);
        if (File.Exists(path))
        {
            currentPrompt = File.ReadAllText(path);
            Debug.Log($"Successfully load Prompt: {fileName}");
        }
        else
        {
            Debug.LogWarning($"Can not find Prompt file: {path}");
            currentPrompt = "";
        }
    }

    public void ProcessGPTResponse(string content)
    {
        try
        {
            var result = JsonUtility.FromJson<BehaviorResult>(content);
            if (result != null && !string.IsNullOrEmpty(result.behavior))
            {
                Debug.Log("GPT 行为指令解析成功: " + result.behavior);
                GameManager.Instance.ReceiveAIBehaviorCommand(result.behavior);
            }
            else
            {
                Debug.LogWarning("GPT 行为字段为空");
            }
        }
        catch
        {
            Debug.LogError("无法解析 GPT 返回内容为 JSON");
        }
    }
}
