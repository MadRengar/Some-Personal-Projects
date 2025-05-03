using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
public class ChatGPTManager : MonoBehaviour
{
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatRespondes = response.Choices[0].Message;
            messages.Add(chatRespondes);

            Debug.Log(chatRespondes.Content);
        }
    }

    void Start()
    {
        AskChatGPT("你好！你是我的AI队友，现在测试你是否能回复。");
    }

    void Update()
    {
        
    }
}
