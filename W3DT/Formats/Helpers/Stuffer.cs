﻿using System;
using System.Reflection;
using W3DT._3D;

namespace W3DT.Formats
{
    class Stuffer
    {
        public static void Stuff(object target, FormatBase feed, string logPrefix = null, bool muteLogging = false)
        {
            foreach (PropertyInfo prop in target.GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    MethodInfo setter = prop.GetSetMethod();
                    if (setter == null || !setter.IsPublic)
                        continue;

                    Type type = prop.PropertyType;

                    object set = null;
                    if (type.Equals(typeof(Int16)))
                        set = feed.readInt16();
                    else if (type.Equals(typeof(Int32)))
                        set = feed.readInt32();
                    else if (type.Equals(typeof(Int64)))
                        set = feed.readInt64();
                    else if (type.Equals(typeof(UInt16)))
                        set = feed.readUInt16();
                    else if (type.Equals(typeof(byte)))
                        set = feed.readUInt8();
                    else if (type.Equals(typeof(UInt32)))
                        set = feed.readUInt32();
                    else if (type.Equals(typeof(UInt64)))
                        set = feed.readUInt64();
                    else if (type.Equals(typeof(float)))
                        set = feed.readFloat();
                    else if (type.Equals(typeof(double)))
                        set = feed.readDouble();
                    else if (type.Equals(typeof(string)))
                        set = feed.readString();
                    else if (type.Equals(typeof(Position)))
                        set = Position.Read(feed);
                    else if (type.Equals(typeof(C4Plane)))
                        set = C4Plane.Read(feed);
                    else if (type.Equals(typeof(Colour4)))
                        set = Colour4.Read(feed);
                    else if (type.Equals(typeof(Rotation)))
                        set = Rotation.Read(feed);
                    else if (type.Equals(typeof(MaterialTexture)))
                        set = MaterialTexture.Read(feed);

                    // This is some hacky-ass code, but it allows MCNK chunk to use
                    // the stuffer without code bloat. Probably could be done better.
                    if (set == null)
                    {
                        if (type.Equals(typeof(UInt32[])))
                        {
                            int size = ((UInt32[])prop.GetValue(target, null)).Length;
                            UInt32[] arr = new UInt32[size];

                            for (int i = 0; i < size; i++)
                                arr[i] = feed.readUInt32();

                            set = arr;
                        }
                    }

                    if (set != null)
                    {
                        prop.SetValue(target, set, null);

                        #if !DEBUG
                        if (!muteLogging)
                        #endif
                            Log.Write("{0}{1} -> {2}", logPrefix == null ? string.Empty : logPrefix, prop.Name, set);
                    }
                    else
                    {
                        Log.Write("WARNING: Stuffer was not prepared to handle {0}!", type.Name);
                    }
                }
            }
        }

        public static void DescribeStuffing(object target)
        {
            foreach (PropertyInfo prop in target.GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    MethodInfo setter = prop.GetSetMethod();
                    if (setter == null || !setter.IsPublic)
                        continue;

                    Log.Write("{0} -> {1}", prop.Name, prop.GetValue(target, null));
                }
            }
        }
    }
}
