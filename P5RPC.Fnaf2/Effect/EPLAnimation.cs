using System.Runtime.InteropServices;

namespace P5RPC.Fnaf2.Effect;

[StructLayout(LayoutKind.Explicit, Size = 0x00000038)]
internal unsafe struct EPLAnimation {
  [FieldOffset(0x00000010)] public Animation              *Animation;
  [FieldOffset(0x00000020)] public EPLAnimationController *eplAnimEnd;
  [FieldOffset(0x00000028)] public EPLAnimationController *eplAnimStart;
}
