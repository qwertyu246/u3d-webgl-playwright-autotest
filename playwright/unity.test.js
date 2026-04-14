const { test, expect } = require("@playwright/test");

const UNITY_URL = "http://localhost:3000";

async function getUITree(page) {
  // 重置 Signal Flag
  await page.evaluate(() => { window.isReady = false; });

  // 觸發掃描
  await page.evaluate(() => {
    window.myUnityInstance.SendMessage("UIBridgeAgent", "ExportUI");
  });

  // 等 Unity 把資料寫入全域變數
  await page.waitForFunction(() => window.isReady === true, { timeout: 10_000 });

  // 取出並 parse
  const raw = await page.evaluate(() => window.unityUIState);
  return JSON.parse(raw).nodes;
}

async function clickNode(page, node, rootNode) {
  const rect = await page.evaluate(() => {
    const r = document.getElementById("unity-canvas").getBoundingClientRect();
    return { left: r.left, top: r.top, width: r.width, height: r.height };
  });
  const sx = rect.width  / rootNode.width;
  const sy = rect.height / rootNode.height;
  const x  = rect.left + node.x * sx + (node.width  * sx) / 2;
  const y  = rect.top  + node.y * sy + (node.height * sy) / 2;
  await page.mouse.click(x, y);
}

// ── 前置:等 Unity 載入完並讓 layout 穩定 ──────────────────
test.beforeEach(async ({ page }) => {
  await page.goto(UNITY_URL, { waitUntil: "domcontentloaded" });
  await page.waitForFunction(
    () => typeof window.myUnityInstance !== "undefined",
    { timeout: 90_000 }
  );
  // 等 Unity 渲染幾幀，確保 layout 計算完成
  await page.waitForTimeout(3000);
});

// ── 測試 1:掃描 ────────────────────────────────────────────
test("掃描 UI Tree 並找到 test-button", async ({ page }) => {
  const nodes = await getUITree(page);
  console.log("節點數:", nodes.length);

  const btn = nodes.find(n => n.name === "test-button");
  expect(btn).toBeDefined();
  expect(btn.type).toBe("Button");
  expect(btn.width).toBeGreaterThan(0);
  console.log("Button:", btn);
});

// ── 測試 2:點擊 ────────────────────────────────────────────
test("點擊 test-button", async ({ page }) => {
  const nodes = await getUITree(page);
  const root  = nodes[0];
  const btn   = nodes.find(n => n.name === "test-button");
  expect(btn).toBeDefined();

  await clickNode(page, btn, root);
  await page.waitForTimeout(500);
  console.log("點擊完成!");
});

// ── 測試 3:點 10 下，確認 Label 顯示 "Clicked 10 times!" ───
test("連點 10 下 Label 正確累計", async ({ page }) => {
  let nodes = await getUITree(page);
  const root = nodes[0];
  const btn  = nodes.find(n => n.name === "test-button");
  expect(btn).toBeDefined();

  // 連點 10 下
  for (let i = 1; i <= 10; i++) {
    await clickNode(page, btn, root);
    await page.waitForTimeout(100);
  }

  // 點完後重新掃描，讀取 Label 的 text
  nodes = await getUITree(page);
  const label = nodes.find(n => n.name === "result-label");

  console.log("Label 內容:", label.text);
  expect(label.text).toBe("Clicked 10 times!");
});
