public struct Block
{
	public byte id;         // 0 = Air, 1 = Dirt, 2 = Stone, etc.
	public byte flags;      // Bit 0: IsSolid. Bit 1: IsTransparent. Bits 2-7: Orientation/States
	public byte lightLevel; // 0-15
	public byte metadata;   // Health, variant, or moisture

	// Helper property to check solidity fast using bitwise operations
	public bool IsSolid => (flags & 1) == 1;
}