﻿using System;

namespace W3DT.Formats.WMO
{
    public class Chunk_MOGN : Chunk_Base
    {
        public const UInt32 Magic = 0x4D4F474E;
        public StringBlock data { get; private set; }

        public Chunk_MOGN(WMOFile file) : base(file, "MOGN", Magic)
        {
            data = new StringBlock(file, (int) ChunkSize);
            LogWrite("Loaded " + data.count() + " group names.");
        }
    }
}
