using UnityEngine;
using UnityEngine.UIElements;

// 掛在場景的 UIDocument GameObject 上
[RequireComponent(typeof(UIDocument))]
public class TestUIController : MonoBehaviour
{
    private Label m_ResultLabel;
    private int m_ClickCount = 0;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var button = root.Q<Button>("test-button");
        m_ResultLabel = root.Q<Label>("result-label");

        button.clicked += OnButtonClicked;
    }

    void OnButtonClicked()
    {
        m_ClickCount++;
        m_ResultLabel.text = $"Clicked {m_ClickCount} times!";
        Debug.Log($"[TestUI] Button clicked! Count: {m_ClickCount}");
    }
}
