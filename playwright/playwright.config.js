const { defineConfig } = require("@playwright/test");

module.exports = defineConfig({
  timeout: 120_000,        // 每個 test 最多 2 分鐘
  use: {
    actionTimeout: 30_000, // 每個 action 最多 30 秒
  },
});
