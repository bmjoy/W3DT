﻿using System;
using W3DT.Formats;

namespace W3DT._3D
{
    public class FogInfo
    {
        public UInt32 flags { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float smallRadius { get; set; }
        public float largeRadius { get; set; }
        public float fogEnd { get; set; }
        public float fogMultiplier { get; set; }
        public Colour4 colour1 { get; set; }
        public float unk1 { get; set; } // 222.222
        public float unk2 { get; set; }
        public Colour4 colour2 { get; set; }

        public static FogInfo Read(FormatBase input)
        {
            FogInfo temp = new FogInfo();
            Stuffer.Stuff(temp, input, "FogInfo", true);
            return temp;
        }
    }
}
