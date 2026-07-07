public struct Block
{
	public byte id;         // 0 = Air, 1 = Dirt, 2 = Stone, etc.
	public byte flags;      // Bit 0: IsSolid. Bit 1: IsTransparent. Bits 2-7: Orientation/States
	public byte lightLevel; // 0-15
	public byte metadata;   // Health, variant, or moisture

	private readonly static byte IsSolidMask = 1; // Bit 0
	// Helper property to check solidity fast using bitwise operations
	public bool IsSolid
	{
		get { return (flags & IsSolidMask) == IsSolidMask; }
		set { flags = (byte)(value ? flags | IsSolidMask : flags & ~IsSolidMask); }
	}
}