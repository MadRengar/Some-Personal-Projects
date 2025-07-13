using System.Collections.Generic;

public enum MessageKey
{
    AI_parse_fail,
    Generator_low_energy,
    Build_success,
    Build_no_resource,
    Zombie_wave
}

public static class RadioMessages
{
    public static readonly Dictionary<MessageKey, string> MessageTable = new Dictionary<MessageKey, string>
    {
        { MessageKey.AI_parse_fail,      "AI instruction parsing failed!" },
        { MessageKey.Generator_low_energy,  "Insufficient power!" },
        { MessageKey.Build_success,      "Construction completed!" },
        { MessageKey.Build_no_resource,  "Insufficient resources to build!" },
        { MessageKey.Zombie_wave,        "Our base is under attack!" },
    };

    public static string Get(MessageKey key)
    {
        if (MessageTable.TryGetValue(key, out string message))
            return message;
        return $"[{key}]"; // 默认fallback
    }

    /// <summary>
    /// 显示消息 - 主要接口
    /// </summary>
    public static void Show(MessageKey messageKey, RadioPopController.MessageType messageType)
    {
        if (RadioPopController.Instance != null)
        {
            RadioPopController.Instance.ShowMessage(messageKey, messageType);
        }
    }

    /// <summary>
    /// 便捷方法 - 显示指挥官消息
    /// </summary>
    public static void Command(MessageKey messageKey)
    {
        Show(messageKey, RadioPopController.MessageType.Command);
    }

    /// <summary>
    /// 便捷方法 - 显示警告消息
    /// </summary>
    public static void Warning(MessageKey messageKey)
    {
        Show(messageKey, RadioPopController.MessageType.Warning);
    }

    /// <summary>
    /// 便捷方法 - 显示错误消息
    /// </summary>
    public static void Error(MessageKey messageKey)
    {
        Show(messageKey, RadioPopController.MessageType.Error);
    }

    /// <summary>
    /// 便捷方法 - 显示信息消息
    /// </summary>
    public static void Info(MessageKey messageKey)
    {
        Show(messageKey, RadioPopController.MessageType.Info);
    }
}