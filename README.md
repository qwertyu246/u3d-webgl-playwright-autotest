# Unity UI Toolkit Playwright 測試

讓外部測試框架(Playwright) 操作Unity WebGL 的 UI Toolkit 元素

## 架構

```
Unity (C#)  ->  .jslib  ->  Browser  ->  Playwright
  掃描 UI        橋接       全域變數      讀取 + 點擊
```

### 三個核心檔案

| 檔案 | 位置 | 說明 |
|------|------|------|
| `UIBridgeAgent.cs` | `Assets/Scripts/` | 掃描 UIDocument，序列化為 JSON |
| `UIBridge.jslib` | `Assets/Plugins/WebGL/` | 將 JSON 寫入 `window.unityUIState` |
| `unity.test.js` | `playwright/` | Playwright 測試腳本 |

### 通訊流程

```
Playwright
  -> SendMessage('UIBridgeAgent', 'ExportUI')
  -> C# 掃描所有 UIDocument 的 VisualElement
  -> 序列化成JSON (name, type, text, x, y, width, height)
  -> jslib 寫入 window.unityUIState，設 window.isReady = true
  -> Playwright waitForFunction(() => window.isReady)
  -> 取得座標，page.mouse.click(x, y)
```

## 快速開始

### 1. 建立 WebGL Build

在 Unity 開啟此專案:

- **Player Settings -> WebGL -> WebGL Template** 選 `UIBridge`
- **Publishing Settings -> Compression Format** 選 `Disabled`
- **File -> Build Settings -> Build**，輸出到 `WebGL-Build/`

### 2. 啟動本地伺服器

```bash
cd WebGL-Build
npx serve .
```

### 3. 執行 Playwright 測試

```bash
cd playwright
npm install
npx playwright test unity.test.js --headed
```

## JSON 資料結構

```json
{
  "nodes": [
    {
      "id": 0,
      "parentId": -1,
      "type": "Button",
      "name": "test-button",
      "text": "Click Me!",
      "x": 358.0,
      "y": 373.0,
      "width": 200.0,
      "height": 80.0
    }
  ]
}
```

## 測試項目

| 測試 | 內容 |
|------|------|
| 掃描 UI Tree | 確認 Button 節點存在且有座標 |
| 點擊 Button | 驗證 Playwright 真實滑鼠模擬可觸發 Unity 事件 |
| 連點 10 下 | 確認 Label 累計顯示 "Clicked 10 times!" |