using System.Runtime.InteropServices;

namespace P5RPC.Fnaf2.Effect;

[StructLayout(LayoutKind.Explicit, Size = 0x00000070)]
internal unsafe struct EPL {
  [FieldOffset(0x00000030)] public EPLFlags      eplFlags;
  [FieldOffset(0x00000038)] public float         timeElapsed;
  [FieldOffset(0x00000048)] public EPLAnimation *eplAnimation;
}
