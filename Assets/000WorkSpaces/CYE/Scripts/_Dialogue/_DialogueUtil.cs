// using System;
// using System.Collections;
// using System.Collections.Generic;
// using KYS.DialogueSystem;
// using UnityEngine;

// namespace GameDialogue
// {
//     public struct DialogueSentence
//     {
//         /// <summary>
//         /// 발화자(키랙터) ID
//         /// </summary>
//         public string SpeakerId { get; private set; }
//         /// <summary>
//         /// 대사 타입
//         /// </summary>
//         public NodeType DialogueType { get; private set; }
//         /// <summary>
//         /// 캐릭터 이미지 위치(false: 왼쪽, true: 오른쪽)
//         /// </summary>
//         public bool IsRightCharacter { get; private set; }
//         /// <summary>
//         /// 대화 내용
//         /// </summary>
//         public string Sentence { get; private set; }

//         public DialogueSentence(string speakerId, NodeType dialogueType, bool isRightCharacter, string sentence)
//         {
//             this.SpeakerId = speakerId;
//             this.DialogueType = dialogueType;
//             this.IsRightCharacter = isRightCharacter;
//             this.Sentence = sentence;
//         }
//     }
//     public class DialogueTree
//     {
//         /// <summary>
//         /// (전체)대화 ID
//         /// </summary>
//         public string Id;
//         /// <summary>
//         /// 트리 시작
//         /// </summary>
//         public DialogueTreeNode Parent { get; set; } = new();
//     }
//     public class DialogueTreeNode
//     {
//         /// <summary>
//         /// 대화 노드 ID
//         /// </summary>
//         public string Id { get; set; }
//         public List<DialogueSentence> Sentences { get; set; } = new();
//         /// <summary>
//         /// 해당 대화로 접근할 수 있는 선택지 버튼 문구(미지정시 그냥 출력)
//         /// </summary>
//         public string OptionText { get; set; }
//         /// <summary>
//         /// 자식 노드(선택지) 목록
//         /// </summary>
//         public List<DialogueTreeNode> Children { get; set; } = new();
//     }

//     public class DialogueUtil
//     { 
//         public static string[] ConvertStringToArray(string rawText)
//         {
//             return Array.FindAll(rawText.Split(','), val => !String.IsNullOrEmpty(val));
//         }
        
//         public static DialogueTree MakeSampleDataTree()
//         {
//             // Depth 3
//             DialogueTreeNode node_2_1_1 = new DialogueTreeNode();
//             node_2_1_1.OptionText = "선택지 2-1-1";
//             node_2_1_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2-1-1"));
//             DialogueTreeNode node_2_1_2 = new DialogueTreeNode();
//             node_2_1_2.OptionText = "선택지 2-1-2";
//             node_2_1_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2-1-2"));

//             // Depth 2
//             DialogueTreeNode node_1_1 = new DialogueTreeNode();
//             node_1_1.OptionText = "선택지 1-1";
//             node_1_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 1-1"));
//             DialogueTreeNode node_1_2 = new DialogueTreeNode();
//             node_1_2.OptionText = "선택지 1-2";
//             node_1_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 1-2"));

//             DialogueTreeNode node_2_1 = new DialogueTreeNode();
//             node_2_1.OptionText = "선택지 2-1";
//             node_2_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2-1 (1)"));
//             node_2_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2-1 (2)"));
//             node_2_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2-1 (3)"));
//             node_2_1.Children.Add(node_2_1_1);
//             node_2_1.Children.Add(node_2_1_2);

//             DialogueTreeNode node_3_1 = new DialogueTreeNode();
//             node_3_1.OptionText = "선택지 3-1";
//             node_3_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 3-1"));
//             DialogueTreeNode node_3_2 = new DialogueTreeNode();
//             node_3_2.OptionText = "선택지 3-2";
//             node_3_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 3-2"));
//             DialogueTreeNode node_3_3 = new DialogueTreeNode();
//             node_3_3.OptionText = "선택지 3-3";
//             node_3_3.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 3-3"));

//             // Depth 1
//             DialogueTreeNode node_1 = new DialogueTreeNode();
//             node_1.OptionText = "선택지 1";
//             node_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 1 (1)"));
//             node_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 1 (2)"));
//             node_1.Children.Add(node_1_1);
//             node_1.Children.Add(node_1_2);

//             DialogueTreeNode node_2 = new DialogueTreeNode();
//             node_2.OptionText = "선택지 2";
//             node_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2 (1)"));
//             node_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 2 (2)"));
//             node_2.Children.Add(node_2_1);

//             DialogueTreeNode node_3 = new DialogueTreeNode();
//             node_3.OptionText = "선택지 3";
//             node_3.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "대사 3"));
//             node_3.Children.Add(node_3_1);
//             node_3.Children.Add(node_3_2);
//             node_3.Children.Add(node_3_3);

//             DialogueTreeNode start_node_1 = new DialogueTreeNode();
//             start_node_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 1 (1)"));
//             start_node_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 1 (2)"));
//             start_node_1.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 1 (3)"));
//             start_node_1.Children.Add(node_1);
//             start_node_1.Children.Add(node_2);
//             start_node_1.Children.Add(node_3);

//             DialogueTreeNode start_node_2 = new DialogueTreeNode();
//             start_node_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 2 (1)"));
//             start_node_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 2 (2)"));
//             start_node_2.Sentences.Add(new DialogueSentence("test", NodeType.Dialogue, true, "시작 대사 2 (3)"));
//             start_node_2.Children.Add(node_1);
//             start_node_2.Children.Add(node_2);
//             start_node_2.Children.Add(node_3);

//             // Root
//             DialogueTree result = new();
//             result.Parent = start_node_1;

//             return result;
//         }
//     }
// }