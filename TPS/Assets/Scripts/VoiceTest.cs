using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceTest : MonoBehaviour
{
    private DictationRecognizer dictationRecognizer;
    private StringBuilder textSoFar;

    void Start()
    {
        textSoFar = new StringBuilder();

        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.Log("识别结果: " + text);
            textSoFar.Append(text + " ");

            // 这里你可以直接调用 ChatGPT
            // ChatGPTManager.Instance.AskChatGPT(text);
        };

        dictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.Log("正在识别: " + text);
        };

        dictationRecognizer.DictationComplete += (completionCause) =>
        {
            Debug.Log("识别完成: " + completionCause.ToString());
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError("识别出错: " + error);
        };

        dictationRecognizer.Start();
    }

    void OnDestroy()
    {
        dictationRecognizer.Stop();
        dictationRecognizer.Dispose();
    }

}
