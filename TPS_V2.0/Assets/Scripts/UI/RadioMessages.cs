using System.Collections.Generic;

public enum MessageKey
{
    /* Instruction */
    AI_parse_fail,

    /* Building */
    Building_Generator_low_energy,
    Building_isDestroied,
    Building_isAttacked,
    /* BuildingSystem */
    Build_success,
    Build_no_resource,
    Building_FullHealth,

    /* Zombie */
    Zombie_wave,

    /* Command */
    /* Ping */
    PingMove_success,
    PingMove_unsuccess,
    Ping_illegal,
    Ping_tooclose,

    Command_Follow,
    Command_Collect,

    Command_GoHeal,
    Command_ReplenishAmmo,

    /* Interact */
    Interact_foodSupply,
    Interact_NotEnoughResources,
    Interact_NotEnoughBackpackCapacity,

    /* Player */
    Player_EnterTreatmentArea,
    Player_lowSatiety,
    Player_zeroSatiety,

    /* AI */
    AI_EnterTreatmentArea,
}

public static class RadioMessages
{
    public static readonly Dictionary<MessageKey, string> MessageTable = new Dictionary<MessageKey, string>
    {
        { MessageKey.AI_parse_fail,      "AI instruction parsing failed! Clear command." },
        { MessageKey.Building_Generator_low_energy,  "Insufficient power!" },
        { MessageKey.Build_success,      "Construction completed!" },
        { MessageKey.Build_no_resource,  "Insufficient resources to build!" },
        { MessageKey.Zombie_wave,        "Our base is under attack!" },
        { MessageKey.PingMove_success,       "Moving to the marked location" },
        { MessageKey.PingMove_unsuccess,      "Illegal marked location" },
        { MessageKey.Ping_illegal,      "Illegal mark" },
        { MessageKey.Ping_tooclose,      "Mark too close" },
        { MessageKey.Building_FullHealth,      "Building is full health" },
        { MessageKey.Interact_foodSupply,      "Replenishing satiety" },
        { MessageKey.Player_EnterTreatmentArea,      "You are receiving treatment" },
        { MessageKey.AI_EnterTreatmentArea,      "Your teammate is receiving treatment" },
        { MessageKey.Building_isDestroied,      "Building is destroied!" },
        { MessageKey.Building_isAttacked,      "Our base is under attack!" },
        { MessageKey.Player_lowSatiety,      "Low satiety! Your stamina has dropped significantly." },
        { MessageKey.Player_zeroSatiety,      "You're starving! Health is draining over time. Find food immediately." },
        { MessageKey.Command_Follow,      "AI is tactically moving closer to players." },
        { MessageKey.Command_Collect,      "AI is collecting resources." },
        { MessageKey.Command_GoHeal,      "AI is heading to the treatment area!" },
        { MessageKey.Command_ReplenishAmmo,      "AI is heading to the ammo supply location." },
        { MessageKey.Interact_NotEnoughBackpackCapacity,      "You don't have enough Backpack Capacity." },
        { MessageKey.Interact_NotEnoughResources,      "You don't have enough resources." },
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