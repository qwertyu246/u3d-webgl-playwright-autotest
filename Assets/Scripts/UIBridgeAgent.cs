using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class UIBridgeAgent : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SendToBrowser(string json);
#endif

    // Playwright: window.myUnityInstance.SendMessage('UIBridgeAgent', 'ExportUI');
    public void ExportUI()
    {
        var nodes = new List<Node>();
        int id = 0;

        foreach (var doc in FindObjectsByType<UIDocument>())
        {
            if (doc.rootVisualElement != null)
                Scan(doc.rootVisualElement, -1, nodes, ref id);
        }

        string json = ToJson(nodes);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendToBrowser(json);
#else
        Debug.Log("[UIBridgeAgent] " + json);
#endif
    }

    void Scan(VisualElement el, int parentId, List<Node> list, ref int id)
    {
        if (el.resolvedStyle.display == DisplayStyle.None) return;

        Rect b = el.worldBound;
        int myId = id++;

        // Label 和 Button 都繼承 TextElement，可以讀 text
        string text = (el is TextElement te) ? te.text : "";

        list.Add(new Node
        {
            id       = myId,
            parentId = parentId,
            type     = el.GetType().Name,
            name     = el.name ?? "",
            text     = text,
            x        = b.x,
            y        = b.y,
            width    = b.width,
            height   = b.height
        });

        // rec scan
        foreach (var child in el.Children())
            Scan(child, myId, list, ref id);
    }

    string ToJson(List<Node> nodes)
    {
        var sb = new StringBuilder("{\"nodes\":[");
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var n = nodes[i];
            sb.Append($"{{\"id\":{n.id},\"parentId\":{n.parentId}," +
                      $"\"type\":\"{Esc(n.type)}\",\"name\":\"{Esc(n.name)}\"," +
                      $"\"text\":\"{Esc(n.text)}\"," +
                      $"\"x\":{F(n.x)},\"y\":{F(n.y)}," +
                      $"\"width\":{F(n.width)},\"height\":{F(n.height)}}}");
        }
        sb.Append("]}");
        return sb.ToString();
    }

    static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"':  sb.Append("\\\""); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                default:
                    if (c < 0x20) sb.Append($"\\u{(int)c:x4}");
                    else sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }
    static string F(float v) => (float.IsNaN(v) || float.IsInfinity(v))
        ? "0" : v.ToString("F2", CultureInfo.InvariantCulture);

    class Node
    {
        public int id, parentId;
        public string type, name, text;
        public float x, y, width, height;
    }
}
