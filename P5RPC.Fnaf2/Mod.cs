using P5RPC.Fnaf2.Configuration;
using P5RPC.Fnaf2.Effect;
using P5RPC.Fnaf2.Template;

using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Mod.Interfaces;

using System.Diagnostics;
using System.Drawing;

namespace P5RPC.Fnaf2;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
  [Function(CallingConventions.Microsoft)]
  private delegate void *AddMeshToGlobalAttachmentList(
    void *param_1,
    void *param_2, string? name, byte param_3);

  [Function(CallingConventions.Microsoft)]
  private delegate EPL *LoadEPLFromFilename(string param_1);

  [Function(CallingConventions.Microsoft)]
  private delegate void PlayFromSystemACB(int param_1);

  [Function(CallingConventions.Microsoft)]
  private delegate void RestartEPLPlayback(void *param_1);

  /// <summary>
  /// Provides access to the mod loader API.
  /// </summary>
  private readonly IModLoader _modLoader;

	/// <summary>
	/// Stores the contents of your mod's configuration. Automatically updated by template.
	/// </summary>
	private Config _configuration = null!;

  /// <summary>
  /// Provides access to the Reloaded.Hooks API.
  /// </summary>
  /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
  private readonly IReloadedHooks _hooks;

  /// <summary>
  /// Provides access to the Reloaded logger.
  /// </summary>
  private readonly ILogger _logger;

  /// <summary>
  /// Entry point into the mod, instance that created this class.
  /// </summary>
  private readonly IMod _owner;

  /// <summary>
  /// The configuration of the currently executing mod.
  /// </summary>
  private readonly IModConfig _modConfig;

  private readonly AddMeshToGlobalAttachmentList? _addMeshToGlobalAttachmentList;
  private readonly bool                          *_pauseScreenVisible;
  private readonly LoadEPLFromFilename?           _loadEplFromFilename;
  private readonly PlayFromSystemACB?             _playFromSystemACB;
  private readonly RestartEPLPlayback?            _restartEplPlayback;
  private readonly nuint                         *_titleResProcInstance;
  private readonly void                         **_rootGfdGlobalScene;

  public Mod(ModContext context) {
    Process process = Process.GetCurrentProcess();
    using Scanner scanner = new Scanner(process);

    _modLoader = context.ModLoader;
    _hooks = context.Hooks!;
    _logger = context.Logger;
    _owner = context.Owner;
    _modConfig = context.ModConfig;
    _configuration = context.Configuration;

    PatternScanResult addResult = scanner.FindPattern("48 89 5C 24 08 48 89 6C " +
      "24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57 48 81 EC 20 01 00 00 45");

    if (!addResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `AddMeshToGlobalAttachmentList`! The mod will not work...",
        Color.Red);

      return;
    }

    _addMeshToGlobalAttachmentList = _hooks.CreateWrapper<AddMeshToGlobalAttachmentList>(
      (process.MainModule!.BaseAddress + addResult.Offset), out _);

    PatternScanResult pauseResult = scanner.FindPattern("80 3D ?? ?? ?? ?? ?? 0F " +
      "84 A1 00 00 00");

    if (!pauseResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `PauseScreenVisible`! The mod will be unable to work.",
        Color.Red);

      return;
    }

    _pauseScreenVisible = (bool *)(CmpInstructionToAbsoluteAddress(
      (byte *)(process.MainModule!.BaseAddress + pauseResult.Offset), 7));

    PatternScanResult loadResult = scanner.FindPattern("48 89 5C 24 08 57 48 83 " +
      "EC 60 48 8B D9 E8 ?? ?? ?? ?? 45 33 C9 45 33 C0 33 D2 48 8B CB 48 8B F8 81 48");

    if (!loadResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `LoadEPLFromFilename`! The mod will not be functioning...",
        Color.Red);

      return;
    }

    _loadEplFromFilename = _hooks.CreateWrapper<LoadEPLFromFilename>(
      (process.MainModule!.BaseAddress + loadResult.Offset), out _);

    PatternScanResult playResult = scanner.FindPattern(
      "40 53 48 83 EC 30 48 8B 1D ?? ?? ?? ?? 48 85 DB 74 25 8B 53 08 41");

    if (!playResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `PlayFromSystemACB`! The mod will not be functioning...",
        Color.Red);

      return;
    }

    _playFromSystemACB = _hooks.CreateWrapper<PlayFromSystemACB>(
      (process.MainModule!.BaseAddress + playResult.Offset), out _);

    PatternScanResult deleteResult = scanner.FindPattern("48 89 6C 24 18 57 48 " +
      "83 EC 20 48 8B 79 50");

    if (!deleteResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `RestartEPLPlayback`! The mod will be unable to function...",
        Color.Red);

      return;
    }

    _restartEplPlayback = _hooks.CreateWrapper<RestartEPLPlayback>(
      (process.MainModule!.BaseAddress + deleteResult.Offset), out _);

    PatternScanResult titleResult = scanner.FindPattern("48 8B 0D ?? ?? ?? ?? 48 " +
      "85 C9 74 0F 48 8B 41 48 66 44 39 78 02 0F 82 6B 07 00 00");

    if (!titleResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `TitleResProcInstance`! The mod will be unable to work.",
        Color.Red);

      return;
    }

    _titleResProcInstance = (nuint *)(MovInstructionToAbsoluteAddress(
      (byte *)(process.MainModule!.BaseAddress + titleResult.Offset), 7));

    PatternScanResult rootResult = scanner.FindPattern("48 8B 0D ?? ?? ?? ?? 48 " +
      "85 C9 74 05 E8 ?? ?? ?? ?? 48 8B 05");

    if (!rootResult.Found) {
      _logger.PrintMessage(
        "Failed to locate `RootGfdGlobalScene`! The mod will be unable to function.",
        Color.Red);

      return;
    }

    _rootGfdGlobalScene = (void **)(MovInstructionToAbsoluteAddress(
      (byte *)(process.MainModule!.BaseAddress + rootResult.Offset), 7));

    /* TODO: Look into a better way of executing code, potentially via a `Present`
       hook?
    */
    Task.Factory.StartNew(TaskMain, null);
  }

  /* https://reloaded-project.github.io/Reloaded-II/CheatSheet/SignatureScanning/ */
  private byte *CmpInstructionToAbsoluteAddress(byte* instructionAddress,
    int instructionLength)
  {
    byte *nextInstructionAddress = instructionAddress + instructionLength;
    var offset =
      (*((uint *)(instructionAddress + 2)));
    return (nextInstructionAddress + offset);
  }

  private byte *MovInstructionToAbsoluteAddress(byte* instructionAddress,
    int instructionLength)
  {
    byte *nextInstructionAddress = instructionAddress + instructionLength;
    var offset =
      (*((uint *)(instructionAddress + 3)));
    return (nextInstructionAddress + offset);
  }

  private bool HasEPLAnimationFinished(EPL *epl) {
    if ((epl->eplFlags & EPLFlags.LoopPlayback) is not 0)
      return false;

    if (epl->eplAnimation is null)
      return true;

    float duration;
    if (epl->eplAnimation->eplAnimStart is null)
      duration = epl->eplAnimation->Animation->Duration;
    else
      duration = epl->eplAnimation->eplAnimStart->Duration;

    return (epl->timeElapsed >= duration);
  }

  private void TaskMain(object? state) {
    /* We have a guarantee that our state is not null, so we can directly cast it
       to `IP5RLib`.
    */
    var parameters = (object[])(state!);

    Stopwatch watch = Stopwatch.StartNew();
    Random random = new();

    /* Wait until we pass the initial loading screen before executing the loop. */
    while ((*_titleResProcInstance) is 0)
      Thread.Yield();

    EPL *epl = _loadEplFromFilename!("FIELD/EFFECT/BANK/FB803.EPL");
    void *mesh = null;

    while (true) {
      if (watch.ElapsedMilliseconds < _configuration.Interval)
        continue;

      /* Don't `Restart` the timer and instead postpone the `Random.Next` call. */
      if ((*_pauseScreenVisible))
        continue;

      /* Should be equivalent to a 1 in 10,000 chance. */
      int number = random.Next(_configuration.Begin, _configuration.End);

      if (_configuration.RandomizationDebug) {
        _logger.PrintMessage($"[{_modConfig.ModName}] Random Number: {number}",
          Color.Pink);
      }

      /* If the random number does not match the set beginning of the range, there
         should not be a jumpscare. 
      */
      if (number != _configuration.Begin) {
        watch.Restart();
        continue;
      }

      /* Jumpscare implementation. */
      if (mesh is null)
        mesh = _addMeshToGlobalAttachmentList!((*_rootGfdGlobalScene), epl, null, 0);

      _playFromSystemACB!(9801);
      _restartEplPlayback!(epl);

      while (!HasEPLAnimationFinished(epl))
        Thread.Yield();

      /* Once one second has elapsed, we'll need to `Restart` to measure again. */
      watch.Restart();
    }
  }

  #region Standard Overrides
  public override void ConfigurationUpdated(Config configuration)
  {
  	// Apply settings from configuration.
  	// ... your code here.
  	_configuration = configuration;
  	_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
  }
	#endregion

  #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public Mod() { }
#pragma warning restore CS8618
  #endregion
}
