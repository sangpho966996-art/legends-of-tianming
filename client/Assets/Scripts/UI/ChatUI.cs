using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class ChatUI : MonoBehaviour
    {
        [Header("Chat Display")]
        public ScrollRect chatScrollRect;
        public Transform chatContent;
        public GameObject chatMessagePrefab;
        public int maxMessages = 100;
        
        [Header("Chat Input")]
        public InputField chatInputField;
        public Button sendButton;
        
        [Header("Channel Tabs")]
        public Button allChannelButton;
        public Button systemChannelButton;
        public Button guildChannelButton;
        public Button whisperChannelButton;
        
        [Header("Channel Colors")]
        public Color allChannelColor = Color.white;
        public Color systemChannelColor = Color.yellow;
        public Color guildChannelColor = Color.green;
        public Color whisperChannelColor = Color.magenta;
        
        private List<GameObject> chatMessages = new List<GameObject>();
        private ChatChannel currentChannel = ChatChannel.All;
        private Character playerCharacter;

        private void Start()
        {
            InitializeChatUI();
            FindPlayerCharacter();
        }

        private void InitializeChatUI()
        {
            if (sendButton != null)
                sendButton.onClick.AddListener(OnSendMessage);
                
            if (chatInputField != null)
                chatInputField.onEndEdit.AddListener(OnInputEndEdit);
                
            if (allChannelButton != null)
                allChannelButton.onClick.AddListener(() => SetChannel(ChatChannel.All));
                
            if (systemChannelButton != null)
                systemChannelButton.onClick.AddListener(() => SetChannel(ChatChannel.System));
                
            if (guildChannelButton != null)
                guildChannelButton.onClick.AddListener(() => SetChannel(ChatChannel.Guild));
                
            if (whisperChannelButton != null)
                whisperChannelButton.onClick.AddListener(() => SetChannel(ChatChannel.Whisper));
                
            SetChannel(ChatChannel.All);
            
            AddSystemMessage("Welcome to Legends of Tianming!");
            AddSystemMessage("Use /help for available commands");
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (chatInputField != null && !chatInputField.isFocused)
                {
                    chatInputField.Select();
                    chatInputField.ActivateInputField();
                }
            }
        }

        private void OnSendMessage()
        {
            if (chatInputField == null || string.IsNullOrEmpty(chatInputField.text.Trim()))
                return;
                
            string message = chatInputField.text.Trim();
            ProcessMessage(message);
            
            chatInputField.text = "";
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }

        private void OnInputEndEdit(string message)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSendMessage();
            }
        }

        private void ProcessMessage(string message)
        {
            if (message.StartsWith("/"))
            {
                ProcessCommand(message);
            }
            else
            {
                SendChatMessage(message, currentChannel);
            }
        }

        private void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');
            string cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "/help":
                    ShowHelpCommands();
                    break;
                    
                case "/whisper":
                case "/w":
                    if (parts.Length >= 3)
                    {
                        string targetPlayer = parts[1];
                        string whisperMessage = string.Join(" ", parts, 2, parts.Length - 2);
                        SendWhisper(targetPlayer, whisperMessage);
                    }
                    else
                    {
                        AddSystemMessage("Usage: /whisper <player> <message>");
                    }
                    break;
                    
                case "/guild":
                case "/g":
                    if (parts.Length >= 2)
                    {
                        string guildMessage = string.Join(" ", parts, 1, parts.Length - 1);
                        SendChatMessage(guildMessage, ChatChannel.Guild);
                    }
                    break;
                    
                default:
                    AddSystemMessage($"Unknown command: {cmd}");
                    break;
            }
        }

        private void ShowHelpCommands()
        {
            AddSystemMessage("Available Commands:");
            AddSystemMessage("/whisper <player> <message> - Send private message");
            AddSystemMessage("/guild <message> - Send guild message");
            AddSystemMessage("/help - Show this help");
        }

        private void SendChatMessage(string message, ChatChannel channel)
        {
            if (playerCharacter == null) return;
            
            string playerName = playerCharacter.characterName;
            Color messageColor = GetChannelColor(channel);
            string channelPrefix = GetChannelPrefix(channel);
            
            AddChatMessage($"{channelPrefix}[{playerName}]: {message}", messageColor);
            
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendChatMessage(message, channel.ToString());
            }
        }

        private void SendWhisper(string targetPlayer, string message)
        {
            if (playerCharacter == null) return;
            
            string playerName = playerCharacter.characterName;
            AddChatMessage($"[Whisper to {targetPlayer}]: {message}", whisperChannelColor);
            
            var networkManager = GameManager.Instance?.GetNetworkManager();
            if (networkManager != null)
            {
                networkManager.SendWhisper(targetPlayer, message);
            }
        }

        public void AddChatMessage(string message, Color color)
        {
            if (chatContent == null || chatMessagePrefab == null) return;
            
            GameObject messageObj = Instantiate(chatMessagePrefab, chatContent);
            Text messageText = messageObj.GetComponent<Text>();
            
            if (messageText != null)
            {
                messageText.text = $"[{System.DateTime.Now:HH:mm}] {message}";
                messageText.color = color;
            }
            
            chatMessages.Add(messageObj);
            
            if (chatMessages.Count > maxMessages)
            {
                GameObject oldMessage = chatMessages[0];
                chatMessages.RemoveAt(0);
                Destroy(oldMessage);
            }
            
            Canvas.ForceUpdateCanvases();
            if (chatScrollRect != null)
                chatScrollRect.verticalNormalizedPosition = 0f;
        }

        public void AddSystemMessage(string message)
        {
            AddChatMessage($"[System]: {message}", systemChannelColor);
        }

        private void SetChannel(ChatChannel channel)
        {
            currentChannel = channel;
            UpdateChannelButtons();
        }

        private void UpdateChannelButtons()
        {
            if (allChannelButton != null)
                allChannelButton.interactable = currentChannel != ChatChannel.All;
                
            if (systemChannelButton != null)
                systemChannelButton.interactable = currentChannel != ChatChannel.System;
                
            if (guildChannelButton != null)
                guildChannelButton.interactable = currentChannel != ChatChannel.Guild;
                
            if (whisperChannelButton != null)
                whisperChannelButton.interactable = currentChannel != ChatChannel.Whisper;
        }

        private Color GetChannelColor(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.All: return allChannelColor;
                case ChatChannel.System: return systemChannelColor;
                case ChatChannel.Guild: return guildChannelColor;
                case ChatChannel.Whisper: return whisperChannelColor;
                default: return allChannelColor;
            }
        }

        private string GetChannelPrefix(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.Guild: return "[Guild]";
                case ChatChannel.Whisper: return "[Whisper]";
                default: return "";
            }
        }

        public void OnChatMessageReceived(string playerName, string message, string channel)
        {
            ChatChannel chatChannel = (ChatChannel)System.Enum.Parse(typeof(ChatChannel), channel);
            Color messageColor = GetChannelColor(chatChannel);
            string channelPrefix = GetChannelPrefix(chatChannel);
            
            AddChatMessage($"{channelPrefix}[{playerName}]: {message}", messageColor);
        }

        public void OnWhisperReceived(string fromPlayer, string message)
        {
            AddChatMessage($"[Whisper from {fromPlayer}]: {message}", whisperChannelColor);
        }
    }

    public enum ChatChannel
    {
        All,
        System,
        Guild,
        Whisper
    }
}
