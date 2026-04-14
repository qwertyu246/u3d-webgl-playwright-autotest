mergeInto(LibraryManager.library, {
    SendToBrowser: function (ptr) {
        var data = UTF8ToString(ptr);
        window.unityUIState = data;   // 原始字串 由外部自行parse
        window.isReady = true;        // Signal Flag
    }
});
