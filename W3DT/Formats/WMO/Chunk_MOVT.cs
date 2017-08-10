﻿using System;
using W3DT._3D;

namespace W3DT.Formats.WMO
{
    public class Chunk_MOVT : Chunk_Base, IChunkSoup
    {
        // MOVT WMO Chunk
        // Define vertices

        public const UInt32 Magic = 0x4D4F5654;
        public Position[] vertices { get; private set; }

        public Chunk_MOVT(WMOFile file) : base(file, "MOVT", Magic)
        {
            int vertCount = (int)ChunkSize / 12;
            vertices = new Position[vertCount];

            for (int i = 0; i < vertCount; i++)
            {
                float x = file.readFloat();
                float z = file.readFloat() * -1;
                float y = file.readFloat();

                vertices[i] = new Position(x, y, z);
            }

            LogWrite("Loaded " + vertCount + " verticies.");
        }
    }
}
