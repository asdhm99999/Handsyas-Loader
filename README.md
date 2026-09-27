# Handsyas Loader (Conquer Online 5517)

Original Windows loader for the Handsyas 5517 server. It does NOT modify
client files (it only reads them and creates a one-time `Server.dat.loader_bak`
backup). No passwords, keys or tokens are stored in this repository.

## How the client connects

- The login server listens on TCP `9959` and `9960`
  (`Albetros.Login/Kernel.cs`), game world on `5816`
  (`Albetros/Common.cs: GamePort`).
- The client itself reads its encrypted `Server.dat` for the server entry,
  so the loader only checks reachability of `IP:Port` and launches the
  client exe with `WorkingDirectory` set to the client folder.
- Default `loader.ini` points at `E:\Client\Conquer_modified.exe`
  (the pre-patched exe). `play.exe` / `Conquer.exe` work too —
  just change `ClientExe` in `loader.ini`.

## Run

1. Start the servers (`RESTART_SERVERS.bat`).
2. Run `HandsyasLoader.exe` (next to `loader.ini`).
3. Green "Server Online" → click "Start Game".

If the server is offline you get a Yes/No prompt before launching.

## Build (Windows)

```
dotnet build HandsyasLoader.csproj -c Release
```

Output: `bin\Release\HandsyasLoader.exe`. Requires .NET Framework 4.x
(preinstalled on Windows 10/11). Copy `HandsyasLoader.exe` + `loader.ini`
anywhere and edit the ini for your IP/paths.

## Files

- `Program.cs` — entry point.
- `LoaderForm.cs` — window, TCP status check, launch, `loader.log`, backup.
- `loader.ini` — server name, IP, port, client exe/dir (no secrets).
- `.gitignore` — excludes `bin/`, `obj/`, logs and backups.

## Log

Runtime errors append to `loader.log` next to the exe.
