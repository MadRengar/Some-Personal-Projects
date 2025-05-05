using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper;
using Whisper.Utils;

public class SpeechToTextManager : MonoBehaviour
{
    public WhisperManager whisperManager;
    public MicrophoneRecord micRecord;
    public ChatGPTManager chatGPTManager;
    private PlayerInputSystem _playerInputs;
    public InputActionReference voiceInputAction;
    private bool isRecording = false;

    [SerializeField]
    private string preferredMicDeviceName = "麦克风 (Realtek(R) Audio) 1";


    private void Awake()
    {
        _playerInputs = GetComponent<PlayerInputSystem>();
        voiceInputAction.action.started += OnVoiceInputStarted;
        voiceInputAction.action.canceled += OnVoiceInputCanceled;
        voiceInputAction.action.Enable();
        micRecord.OnRecordStop += OnMicAudioReady;
    }

    private void Start()
    {
        micRecord.SelectedMicDevice = preferredMicDeviceName;
        Debug.Log($"已选择麦克风：{preferredMicDeviceName}");
    }

    private void OnDestroy()
    {
        voiceInputAction.action.started -= OnVoiceInputStarted;
        voiceInputAction.action.canceled -= OnVoiceInputCanceled;
        voiceInputAction.action.Disable();
    }

    private void OnVoiceInputStarted(InputAction.CallbackContext context)
    {
        Debug.Log("开始录音（事件绑定）");
        micRecord.StartRecord();
    }

    private void OnVoiceInputCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("结束录音（事件绑定）");
        micRecord.StopRecord();
    }


    private async void OnMicAudioReady(AudioChunk audio)
    {
        var result = await whisperManager.GetTextAsync(audio.Data, audio.Frequency, audio.Channels);

        if (result != null)
        {
            string userText = result.Result;
            Debug.Log("Whisper识别结果: " + userText);
            //if (chatGPTManager != null)
            //{
            //    chatGPTManager.AskChatGPT(userText);
            //}
        }
    }
}
