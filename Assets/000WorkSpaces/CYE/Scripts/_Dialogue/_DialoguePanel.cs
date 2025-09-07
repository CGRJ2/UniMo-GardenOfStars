// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using KYS;
// using TMPro;
// using UnityEngine.UI;
// using GameNpc;
// using System.Linq;

// namespace GameDialogue
// { 
//     public class DialoguePanel : BaseUI
//     {
//         [Header("Npc Dialogue UI Object Settings")]
//         [SerializeField] private TMP_Text _nameArea;
//         [SerializeField] private TMP_Text _contextArea;
//         [SerializeField] private Button[] _choiceOptions = new Button[3];
//         [SerializeField] private Button _MenuButton;
//         private DialogueTree _dialogueData = new();
//         private DialogueTreeNode _currentDialogueNode = new();
//         private Stack<DialogueTreeNode> _dialogueStack = new();

//         public void InitDialoguePanel()
//         {
//             // this._dialogueData = Manager.npc.CurrentNpc._dialogueScript;
//             ResetDialogueProgress();
//         }

//         public void ShowDialoguePanel()
//         {
//             _nameArea.text = Manager.npc.CurrentNpc._name;
//             // _contextArea.text = _dialogueData.Parent;
//             ResetDialogueProgress();
//             SetOptionButtons();

//             this.gameObject.SetActive(true);
//         }

//         private void SetOptionButtons()
//         {
//             if (_currentDialogueNode == default)
//             {
//                 // _choiceOptions[0].GetComponentInChildren<TMP_Text>().text = _dialogueData.Children[0].OptionText;
//                 _choiceOptions[0].gameObject.SetActive(true);

//                 _MenuButton.GetComponentInChildren<TMP_Text>().text = "그만두기";
//             }
//             else
//             {
//                 foreach (var node in _currentDialogueNode.Children.Select((val, idx) => (val, idx)))
//                 {
//                     _choiceOptions[node.idx].GetComponentInChildren<TMP_Text>().text = node.val.OptionText;
//                     _choiceOptions[node.idx].onClick.AddListener(() => MoveNextNode(node.idx));
//                     _choiceOptions[node.idx].gameObject.SetActive(true);
//                 }
//             }
//         }

//         private void ResetDialogueProgress()
//         {
//             _currentDialogueNode = new();
//             _dialogueStack = new();
//         }

//         private void MoveNextNode(int nodeIdx)
//         { 
            
//         }
//     }
// }
