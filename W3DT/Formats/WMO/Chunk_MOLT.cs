﻿using System;

namespace W3DT.Formats.WMO
{
    public class Chunk_MOLT : Chunk_Base
    {
        // MOLT WMO Chunk
        // Defines lightning data.

        public const UInt32 Magic = 0x4D4F4C54;

        public Chunk_MOLT(WMOFile file) : base(file, "MOLT", Magic)
        {
            int entryCount = (int)ChunkSize / 48;
            LogWrite(entryCount + " lightning entries parsed, but ignored.");
        }
    }
}
