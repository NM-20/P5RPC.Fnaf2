using System.Runtime.InteropServices;

namespace P5RPC.Fnaf2.Effect;

[StructLayout(LayoutKind.Explicit, Size = 0x00000048)]
internal struct Animation {
  [FieldOffset(0x00000004)] public float Duration;
}
