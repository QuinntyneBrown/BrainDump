# Pencil MCP Connection Troubleshooting Log

**Context:** While building a brain-dump UI design in `docs/user-interface-designs/1 - design system.pen`, the Pencil MCP transport dropped mid-session. Every Pencil tool call returned:

```
MCP error -32603: failed to connect to running Pencil app: visual_studio_code
after 3 retries: transport not connected to app: visual_studio_code
```

This document records every recovery attempt and what each one revealed.

## Architecture (what I learned about how Pencil MCP works)

Three pieces:

1. **Pencil VS Code extension** — `highagency.pencildev` at `~/.vscode/extensions/highagency.pencildev-0.6.48`. Hosts a WebSocket server inside VS Code's extension host.
2. **Pencil MCP server** — a native binary at `~/.pencil/mcp/visual_studio_code/out/mcp-server-windows-x64.exe`. Spawned by Claude Code's MCP runtime. Connects out to the extension's WS server.
3. **Port broker file** — `~/.pencil/apps/visual_studio_code` is a single integer (e.g. `50518`). The extension writes its WS port here on startup; the MCP server reads it to know where to connect.

The chain: Claude Code → MCP server (stdio) → WS client → VS Code extension WS server. If link 4 dies, every tool call fails fast at the MCP server with the transport error.

## Things tried

### 1. Re-issue the failing call (no-op)

```
mcp__pencil__get_editor_state()
```

**Result:** Same transport error. Confirmed deterministic failure, not a flake.

### 2. Different `path` argument shapes

```
mcp__pencil__open_document()                         # no path
mcp__pencil__open_document(path="/c:/projects/...")  # leading slash
mcp__pencil__open_document(path="C:\\projects\\...") # backslashes
```

**Result:** All fail with the same transport error. Path format is irrelevant — the failure happens before any file lookup.

### 3. Different Pencil tools

```
get_guidelines, batch_get, get_screenshot, snapshot_layout
```

**Result:** Every tool fails identically. The transport, not any tool, is the problem.

### 4. Open the file via VS Code CLI

```powershell
code --reuse-window "C:\projects\brain-dump\docs\user-interface-designs\1 - design system.pen"
```

**Result:** File loads in VS Code, but Pencil tools still fail. Opening the file isn't enough to start the extension's WS server.

### 5. Inspect process state

```powershell
Get-Process -Name 'mcp-server-windows*'
# → 2 instances running (PIDs 29088, 33180)
Get-NetTCPConnection -State Listen | Where Name='Code'
# → Only port 15629 (VS Code IPC), nothing on Pencil's expected port
```

**Finding:** MCP servers are alive but the VS Code side is silent. **Two Pencil MCP servers running is suspicious** — likely a stale orphan plus a current one.

### 6. Read the port broker file

```powershell
Get-Content "$env:USERPROFILE\.pencil\apps\visual_studio_code"
# → 50518
Test-NetConnection 127.0.0.1 -Port 50518 -InformationLevel Quiet
# → False (nothing listening)
```

**Smoking gun:** The extension wrote `50518` as its port, but isn't actually listening on it. The extension host either crashed or never started its WS server this session.

### 7. Trigger the extension via VS Code URI scheme

```powershell
Start-Process "vscode://highagency.pencildev/"
Start-Process "vscode://file/C:/.../1%20-%20design%20system.pen"
```

**Result:** Both URIs accepted, port 50518 still dead. URIs activate VS Code but don't force the Pencil extension to (re)bind its WS server.

### 8. Kill stale MCP servers

```powershell
Stop-Process -Id 29088,33180 -Force
```

**Result:** Worse outcome — Claude Code didn't respawn them, and the deferred Pencil tools became completely unavailable in the session. **Don't do this.**

## Root cause

The Pencil VS Code extension's WebSocket server is not running, even though the extension is installed and the `.pen` file is open. The MCP server binary tries to connect to `127.0.0.1:50518` (the port the extension claims), gets RST, retries 3 times, and surfaces the transport error. Every Pencil tool call is gated on this connection.

## What actually fixes it (untested from agent — requires user)

In priority order:

1. **`Ctrl+Shift+P` → "Developer: Reload Window"** in VS Code. This restarts the extension host and re-runs Pencil's `activate()`, which rebinds the WS server.
2. **`Ctrl+Shift+P` → "Extensions: Restart Extension Host"** — same effect, lighter weight.
3. **Restart VS Code entirely.**
4. **Restart the Claude Code session** so the MCP host respawns the Pencil MCP server process. Only useful if the MCP server got into a bad state on its end (less likely than the extension dying).

Since killing the MCP servers from outside removes the tools from the session entirely (item 8), the user-side reload is the only reliable path.

## Things that did NOT work — summary table

| Attempt | Outcome |
|---|---|
| Retry the failing call | Same error |
| Vary the `path` argument | Same error |
| Try other Pencil tools | Same error |
| `code --reuse-window <file>` | File opens, transport still down |
| `vscode://highagency.pencildev/` URI | No effect on WS port |
| `vscode://file/<path>` URI | No effect on WS port |
| Kill stale MCP server processes | Tools removed from session — strictly worse |

### 9. Spawn a fresh VS Code window with a new Pencil doc

User suggestion: maybe a clean instance would activate the extension cleanly.

```powershell
code --new-window "$env:TEMP\pencil-warmup.pen"
# and
code --new-window "C:\projects\brain-dump\docs\user-interface-designs\1 - design system.pen"
```

**Result:** New windows opened, several new VS Code processes started listening on fresh ports (31998, 40543, 41522, 58627, 60343, 60350, 60351), but **the broker port (`~/.pencil/apps/visual_studio_code` = `50518`) was never updated and nothing ever bound to it**. The Pencil extension's `activate()` is not running its WS-server bring-up — possibly disabled in user settings, or throwing during activation, or the `.pen` viewer is opening without that code path.

This narrows the diagnosis: the extension itself is stuck, not the host.

## Things that DID work

Nothing that I (the agent) could do from outside VS Code restored the connection. The fix lives inside VS Code's extension host and requires a UI command the agent can't send. Spawning new windows with `.pen` files is **not** sufficient — the extension's WS server bring-up is not being triggered by file-open alone.

## Detection checklist for next time

If Pencil tools start failing with this error:

```powershell
# Is the broker port actually listening?
Test-NetConnection 127.0.0.1 -Port (Get-Content "$env:USERPROFILE\.pencil\apps\visual_studio_code") -InformationLevel Quiet
```

- `False` → extension WS server is dead → reload VS Code window (fix #1).
- `True` → extension is fine; MCP server side is the problem → restart Claude Code session (fix #4).

## Open questions / future ideas

- Could the Pencil extension expose a `pencil.restart` command that's invokable via VS Code CLI? Right now `package.json` lists open-document commands only.
- Is there a way for the MCP server to emit a clearer error like "WS port 50518 not listening — please reload VS Code window" instead of the generic transport error? That would skip steps 1–7 above.
