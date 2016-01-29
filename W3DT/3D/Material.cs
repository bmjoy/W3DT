﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using W3DT.Formats;

namespace W3DT._3D
{
    public class Material
    {
        public UInt32 flags { get; set; }
        public UInt32 shader { get; set; }
        public UInt32 blendMode { get; set; }
        public MaterialTexture texture1 { get; set; }
        public MaterialTexture texture2 { get; set; }
        public MaterialTexture texture3 { get; set; }
        public UInt32 terrainType { get; set; }

        public static Material Read(FormatBase input)
        {
            Material temp = new Material();
            //Stuffer.Stuff(temp, input, null, true);
            temp.flags = input.readUInt32();
            temp.shader = input.readUInt32();
            temp.blendMode = input.readUInt32();

            temp.texture1 = MaterialTexture.Read(input);
            temp.texture2 = new MaterialTexture(input.readUInt32(), input.readUInt32(), 0);
            temp.terrainType = input.readUInt32();
            temp.texture3 = MaterialTexture.Read(input);

            Stuffer.DescribeStuffing(temp);

            input.skip(4 * 4); // runtime floats
            return temp;
        }
    }
}
