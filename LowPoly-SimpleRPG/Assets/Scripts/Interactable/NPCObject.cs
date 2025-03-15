using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCObject : InteractableObject
{
    public string NPCName;
    public string[] contentList;

    //public DialogueUI dialogueUI;

    protected override void Interact()
    {
        //dialogueUI.Show(NPCName, contentList);
        //����ģʽ��д�������д���
        DialogueUI.Instance.Show(NPCName, contentList);
    }
}
