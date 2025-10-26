using System.Runtime.InteropServices;

namespace P5RPC.Fnaf2.Persona5;

[StructLayout(LayoutKind.Explicit, Size = 0x000000C0)]
internal unsafe struct FieldTask {
  [FieldOffset(0x00000048)] public FieldWorkData *taskInternal;
}
