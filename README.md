# 🌳 RecollectViewer Lite

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-blue)](https://www.yasui-kamo.com/labo/recollectviewer/)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2+-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Latest release](https://img.shields.io/github/v/release/y-tomioka/RecollectViewerLite?label=release)](https://github.com/y-tomioka/RecollectViewerLite/releases)

**A lightweight, local Gemini chat history viewer.**  
Visualize your Gemini chat history in a clear, tree-structured format without complex setups.

---

## 💎 What is this?
**RecollectViewer Lite** is a simplified, open-source tool for exploring **Gemini** chat history offline. It parses your local Takeout data and displays it in an intuitive tree view so you can quickly navigate past conversations.

This tool is a lightweight companion to the full-featured **[RecollectViewer](https://github.com/y-tomioka/RecollectViewer)**.

---

## 📸 Screenshots

<table>
  <tr>
    <td align="center" width="50%"><b>Tree view</b></td>
    <td align="center" width="50%"><b>Conversation view</b></td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <img src="https://github.com/user-attachments/assets/8ad7c090-7cf4-4b6c-b6b7-ae8db93c4946" alt="RecollectViewer Lite tree view" width="100%" />
    </td>
    <td align="center" width="50%">
      <img src="https://github.com/user-attachments/assets/1f8aa719-411c-4cfe-8ac6-a65239b45c2e" alt="RecollectViewer Lite conversation view" width="100%" />
    </td>
  </tr>
</table>

---

## 🚀 Key Features
* **🌳 Tree View Visualization:** Navigate Gemini history by date hierarchy.
* **🔒 100% Local & Private:** Your data stays on your machine. No cloud uploads.
* **🌍 Region-friendly:** Stable path handling across different Windows region settings.
* **⚡ Lightweight:** Fast local JSON parsing with a small footprint.

---

## ⚖️ Lite vs. Full Version
RecollectViewer Lite focuses on viewing Gemini logs. For ChatGPT / Claude support and advanced search, see the **[full RecollectViewer](https://github.com/y-tomioka/RecollectViewer)**.

| Feature | Lite Version (This) | Full Version |
| :--- | :---: | :---: |
| **Gemini Support** | ✅ | ✅ |
| **Tree View** | ✅ | ✅ |
| **ChatGPT/Claude Support** | ❌ | ✅ |
| **Advanced Search** | ❌ | ✅ |
| **License** | Free (Open Source / MIT) | Free (Closed source) |

---

## 📥 How to Use
1. Download the latest ZIP from the [Releases page](https://github.com/y-tomioka/RecollectViewerLite/releases).
2. Extract the folder and run `RecollectViewerLite.exe`.
3. Select your Google Takeout Gemini JSON (Takeout → My Activity → Gemini app → JSON).
4. Browse your chat history in the tree view.

> **Requirements:** Windows 10 / 11, .NET Framework 4.7.2+, [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).

---

## 🛠️ Technical Details
* **Language:** C# (.NET Framework 4.7.2+)
* **Scope:** Offline parsing and tree display of local Gemini JSON exports
* **Compatibility:** Works across common Windows region / path settings

---

## 🔗 Official Links
Looking for the full-featured version with ChatGPT, Claude, and unified search?

**[👉 Get the Full RecollectViewer on GitHub](https://github.com/y-tomioka/RecollectViewer)**

🌐 **[Official website (yasui-kamo.com)](https://www.yasui-kamo.com/labo/recollectviewer/)** — tutorials and related tools

---

## ⚖️ License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

*Note: RecollectViewer Lite is a standalone open-source edition. For the full multi-AI product, see [RecollectViewer](https://github.com/y-tomioka/RecollectViewer).*
