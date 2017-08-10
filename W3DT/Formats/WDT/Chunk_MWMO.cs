﻿using System;

namespace W3DT.Formats.WDT
{
    public class Chunk_MWMO : Chunk_Base
    {
        // MWMO WDT Chunk
        // Filename of the global WMO object.

        public const UInt32 Magic = 0x4D574D4F;
        public string fileName { get; private set; }

        public Chunk_MWMO(WDTFile file) : base(file, "MWMO", Magic)
        {
            fileName = file.readString(); // Zero-terminated, this should be safe.
            LogValue("Map WMO", fileName);
        }
    }
}
