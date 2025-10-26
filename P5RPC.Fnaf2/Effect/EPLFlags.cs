namespace P5RPC.Fnaf2.Effect;

internal enum EPLFlags {
  LoopPlayback           = (1 << 0),
  PlayOnce               = (1 << 1),
  StopAtDuration         = (1 << 2),
  TimeFromWin32Timestamp = (1 << 4),
}
