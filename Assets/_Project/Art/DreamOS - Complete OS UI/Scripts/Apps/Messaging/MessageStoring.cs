using UnityEngine;
using System.IO;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Messaging/Message Storing")]
    public class MessageStoring : MonoBehaviour
    {
        [Header("Resources")]
        public MessagingManager messagingManager;

        [Header("Settings")]
        public string subPath = "Data";
        public string fileName = "StoredMessages";
        public string fileExtension = ".data";
        string fullPath;

        string currentID;
        string currentType;
        string currentAuthor;
        string currentMessage;
        string currentTime;

        void Awake()
        {
            if (messagingManager == null) { messagingManager = gameObject.GetComponent<MessagingManager>(); }
            // CheckForDataFile();
        }

        public void CheckForDataFile()
        {
#if UNITY_EDITOR
            fullPath = Application.dataPath + "/" + subPath + "/" + fileName + fileExtension;
#else
            string appPath = Application.dataPath;
            appPath = appPath.Replace(Application.productName + "_Data", "");
            fullPath = appPath + subPath + "/" + "/" + fileName + fileExtension;
#endif

            if (!File.Exists(fullPath))
            {
                FileInfo dataFile = new FileInfo(fullPath);
                dataFile.Directory.Create();
                File.WriteAllText(fullPath, "MSG_DATA");
            }
        }

        public void ReadMessageData()
        {
            CheckForDataFile();
            messagingManager.enabled = false;

            foreach (string option in File.ReadLines(fullPath))
            {
                if (option.Contains("MessageID: "))
                {
                    string tempID = option.Replace("MessageID: ", "");
                    currentID = tempID;
                }

                else if (option.Contains("[Type]"))
                {
                    string tempType = option.Replace("[Type] ", "");
                    currentType = tempType;
                }

                else if (option.Contains("[Author]"))
                {
                    string tempAuthor = option.Replace("[Author] ", "");
                    currentAuthor = tempAuthor;
                }

                else if (option.Contains("[Message]"))
                {
                    string tempMsg = option.Replace("[Message] ", "");
                    currentMessage = tempMsg;
                }

                else if (option.Contains("[Time]"))
                {
                    string tempTime = option.Replace("[Time] ", "");
                    currentTime = tempTime;
                    messagingManager.timeHelper = currentTime;

                    if (currentAuthor == "self" && currentType == "standard")
                        messagingManager.CreateStoredMessage(currentID, currentMessage, currentTime, true);
                    else if (currentAuthor == "individual" && currentType == "standard")
                        messagingManager.CreateStoredMessage(currentID, currentMessage, currentTime, false);
                }
            }

            messagingManager.enabled = true;
        }

        public void ApplyMessageData(string msgID, string msgType, string author, string message, string msgTime)
        {
            File.AppendAllText(fullPath, "\n\nMessageID: " + msgID +
                "\n{" +
                "\n[Type] " + msgType +
                "\n[Author] " + author +
                "\n[Message] " + message +
                "\n[Time] " + msgTime +
                "\n}");
        }

        public void ResetData()
        {
            File.WriteAllText(fullPath, "MSG_DATA");
        }
    }
}