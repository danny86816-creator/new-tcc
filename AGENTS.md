# TCC Repository Operating Contract

本文件是本 repository 內所有 Codex／agent 的持久作業契約。若來源衝突，必須依下列權威順序判定，不得以 agent 假設覆蓋較高順位來源。

## Source Authority

1. Approved Questionnaire
2. Explicit later user requirements
3. Approved Product Constitution
4. Approved UX Architecture
5. Approved System Architecture
6. Approved Theme Architecture
7. Current approved phase/task specification
8. Agent assumptions
9. Legacy TCC reference material only

正式標示為 `APPROVED`、`FINAL`、`LOCKED` 或 `FROZEN` 的文件優先於草稿、候選版本與審查中間產物。

## Mandatory Rules

- 未經明確授權，絕不修改任何 `APPROVED`／`FROZEN` artifact。
- 絕不靜默重新解釋產品需求；衝突或歧義必須停止並回報。
- 目前 phase gate 未通過前，絕不開始後續 implementation phase。
- 絕不新增 broker、exchange 或 prop-firm write／execution API。
- Future connectors 僅限 read-only sync、reconciliation 與 monitoring。
- GPT 必須保持 optional；GPT score 不是 official score。
- Theme Package 不得改變 Core business semantics。
- Gu Qinghan 必須保持為下游 Theme Package，不得硬編碼進 Core。
- 不得削弱 Q93 accessibility。
- 必須維持 Windows Installer／Portable parity。
- 必須維持 Home Safety Core semantics。
- 不得建立未授權的 dependency direction。
- Architecture tests 是強制 gate，不得移除、弱化或規避。
- Validation 失敗時，禁止 commit 並禁止進入下一 phase。
- `docs/CODEX_PROJECT_STATUS.md` 必須保持真實且反映目前狀態。
- 修改 architecture boundary 前必須停止並回報，取得明確授權後才能繼續。
- 專案實作中如呼叫 SKILL，必須列出所使用的 SKILL。
- 不得自行 commit、push、publish、deploy 或建立 release。

## Phase 1 Project Dependency Rules

Phase 1 architecture tests 必須直接解析所有 repository `.csproj` 的 `ProjectReference`，使用 Windows canonical path 與不區分大小寫的比較方式驗證下列 allowlist。未列出的 project 或 direct dependency 一律失敗。

| Project | Allowed direct `ProjectReference` dependencies |
|---|---|
| `Tcc.Presentation.Contracts` | none |
| `Tcc.Themes` | `Tcc.Presentation.Contracts` |
| `Tcc.Features.Themes` | `Tcc.Themes`, `Tcc.Presentation.Contracts` |
| `Tcc.Windows` | none |
| `Tcc.DesktopHost` | `Tcc.Features.Themes`, `Tcc.Themes`, `Tcc.Presentation.Contracts`, `Tcc.Windows` |
| `Tcc.Architecture.Tests` | `Tcc.Features.Themes`, `Tcc.Themes`, `Tcc.Presentation.Contracts`, `Tcc.Windows` |

特別禁止：

- `Tcc.Themes -> Tcc.Features.Themes`
- Theme runtime 或 Theme feature 直接依賴 Domain、Persistence、Connector、AI、Security、Recovery 或其他未核准 implementation assembly
- 任何 feature/runtime package 成為替代 composition root；`Tcc.DesktopHost` 是唯一 composition root

## Required Gate Discipline

開始工作前必須讀取本文件、`README.md`、`docs/CODEX_PROJECT_STATUS.md`、`docs/CODEX_DECISIONS.md` 與適用的正式規格／ADR，並檢查 Git branch、status 與既有未提交變更。

完成修改後，至少執行與變更直接相關的測試、locked restore、Release x64 build、全部測試、Frozen hash 驗證、scope scan 與 Git hygiene 檢查。只有需求完成、所有 gate 通過、狀態文件更新且沒有未揭露阻擋問題時，才能將 commit 或下一 phase 標為允許。
