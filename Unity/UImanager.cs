using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // 需要用到 EventSystem
using System.Collections.Generic; // 需要用到 Queue
public class UImanager : MonoBehaviour
{
    public InputField inputField;
    public Button sendButton;
    public Button repeatButton;
    public Manager manager;

    public Text textBox;
    public int maxLines = 5; // 最大行数
    private Queue<string> lines = new Queue<string>();

    public bool isInputFieldActive = false;
    private void Start()
    {
        // 添加按钮点击事件
        sendButton.onClick.AddListener(SubmitInput);
        repeatButton.onClick.AddListener(Reapeat);

        inputField.gameObject.SetActive(false);  // 初始时隐藏输入框
    }
    public void AddText(string text)
    {
        if (lines.Count >= maxLines)
        {
            lines.Dequeue(); // 移除最旧的一行
        }

        lines.Enqueue(text); // 添加新的一行

        textBox.text = string.Join("\n", lines.ToArray()); // 更新文本框内容
    }
    private void Update()
    {
        // 监听回车键（Return）或者 Enter 键
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleInputField();
        }
    }

    private void ToggleInputField()
    {
        // 切换输入框的激活状态
        inputField.gameObject.SetActive(!inputField.gameObject.activeSelf);
        isInputFieldActive = inputField.gameObject.activeSelf; // 更新输入框状态
        if (inputField.gameObject.activeSelf)
        {
            // 激活输入框时，将焦点放到输入框上，并显示鼠标
            inputField.Select();
            inputField.ActivateInputField();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标

        }
        else
        {
            // 隐藏输入框时，隐藏鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
        }
    }

    private void SubmitInput()
    {
        // 处理提交操作
        string userInput = inputField.text;
        Debug.Log("User Input: " + userInput);
        AddText("You: " + userInput); // 将用户输入添加到文本框
        inputField.text = "";  // 清空输入框
        ToggleInputField();    // 关闭输入框并隐藏鼠标
        manager.GotUserInput(userInput); // 调用 Manager 中的 GetLLMResponse 方法
  }

    public void Reapeat()
    {
        manager.ReapeatMotion();
    }
}
