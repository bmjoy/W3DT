﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace W3DT.Hashing
{
    public class Jenkins96 : HashAlgorithm
    {
        uint a, b, c;
        byte[] hash;

        uint rot(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }

        void Mix()
        {
            a -= c; a ^= rot(c, 4); c += b;
            b -= a; b ^= rot(a, 6); a += c;
            c -= b; c ^= rot(b, 8); b += a;
            a -= c; a ^= rot(c, 16); c += b;
            b -= a; b ^= rot(a, 19); a += c;
            c -= b; c ^= rot(b, 4); b += a;
        }

        void Final()
        {
            c ^= b; c -= rot(b, 14);
            a ^= c; a -= rot(c, 11);
            b ^= a; b -= rot(a, 25);
            c ^= b; c -= rot(b, 16);
            a ^= c; a -= rot(c, 4);
            b ^= a; b -= rot(a, 14);
            c ^= b; c -= rot(b, 24);
        }

        public ulong ComputeHash(string str)
        {
            var tempstr = str.Replace('/', '\\').ToUpper();
            byte[] data = Encoding.ASCII.GetBytes(tempstr);
            return BitConverter.ToUInt64(ComputeHash(data), 0);
        }

        public override void Initialize()
        {
            a = 0;
            b = 0;
            c = 0;
            hash = null;
        }

        protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int length = array.Length;
            a = b = c = 0xdeadbeef + (uint)length;

            fixed (byte* bb = array)
            {
                uint* u = (uint*)bb;

                if ((*u & 0x3) == 0)
                {
                    uint* k = u;

                    while (length > 12)
                    {
                        a += k[0];
                        b += k[1];
                        c += k[2];
                        Mix();
                        length -= 12;
                        k += 3;
                    }

                    switch (length)
                    {
                        case 12: c += k[2]; b += k[1]; a += k[0]; break;
                        case 11: c += k[2] & 0xffffff; b += k[1]; a += k[0]; break;
                        case 10: c += k[2] & 0xffff; b += k[1]; a += k[0]; break;
                        case 9: c += k[2] & 0xff; b += k[1]; a += k[0]; break;
                        case 8: b += k[1]; a += k[0]; break;
                        case 7: b += k[1] & 0xffffff; a += k[0]; break;
                        case 6: b += k[1] & 0xffff; a += k[0]; break;
                        case 5: b += k[1] & 0xff; a += k[0]; break;
                        case 4: a += k[0]; break;
                        case 3: a += k[0] & 0xffffff; break;
                        case 2: a += k[0] & 0xffff; break;
                        case 1: a += k[0] & 0xff; break;
                        case 0:
                            hash = BitConverter.GetBytes(((ulong)c << 32) | (ulong)c);
                            return;
                    }
                }
                else if ((*u & 0x1) == 0)
                {
                    ushort* k = (ushort*)u;

                    while (length > 12)
                    {
                        a += k[0] + (((uint)k[1]) << 16);
                        b += k[2] + (((uint)k[3]) << 16);
                        c += k[4] + (((uint)k[5]) << 16);
                        Mix();
                        length -= 12;
                        k += 6;
                    }

                    byte* k8 = (byte*)k;

                    switch (length)
                    {
                        case 12:
                            c += k[4] + (((uint)k[5]) << 16);
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 11:
                            c += ((uint)k8[10]) << 16;
                            goto case 10;
                        case 10:
                            c += k[4];
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 9:
                            c += k8[8];
                            goto case 8;
                        case 8:
                            b += k[2] + (((uint)k[3]) << 16);
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 7:
                            b += ((uint)k8[6]) << 16;
                            goto case 6;
                        case 6:
                            b += k[2];
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 5:
                            b += k8[4];
                            goto case 4;
                        case 4:
                            a += k[0] + (((uint)k[1]) << 16);
                            break;
                        case 3:
                            a += ((uint)k8[2]) << 16;
                            goto case 2;
                        case 2:
                            a += k[0];
                            break;
                        case 1:
                            a += k8[0];
                            break;
                        case 0:
                            hash = BitConverter.GetBytes(((ulong)c << 32) | (ulong)b);
                            return;
                    }
                }
                else
                {
                    byte* k = (byte*)u;

                    while (length > 12)
                    {
                        a += k[0];
                        a += ((uint)k[1]) << 8;
                        a += ((uint)k[2]) << 16;
                        a += ((uint)k[3]) << 24;
                        b += k[4];
                        b += ((uint)k[5]) << 8;
                        b += ((uint)k[6]) << 16;
                        b += ((uint)k[7]) << 24;
                        c += k[8];
                        c += ((uint)k[9]) << 8;
                        c += ((uint)k[10]) << 16;
                        c += ((uint)k[11]) << 24;
                        Mix();
                        length -= 12;
                        k += 12;
                    }

                    switch (length)
                    {
                        case 12:
                            c += (((uint)k[11]) << 24); goto case 11;
                        case 11:
                            c += (((uint)k[10]) << 16); goto case 10;
                        case 10:
                            c += (((uint)k[9]) << 8); goto case 9;
                        case 9:
                            c += k[8]; goto case 8;
                        case 8:
                            b += (((uint)k[7]) << 24); goto case 7;
                        case 7:
                            b += (((uint)k[6]) << 16); goto case 6;
                        case 6:
                            b += (((uint)k[5]) << 8); goto case 5;
                        case 5:
                            b += k[4]; goto case 4;
                        case 4:
                            a += (((uint)k[3]) << 24); goto case 3;
                        case 3:
                            a += (((uint)k[2]) << 16); goto case 2;
                        case 2:
                            a += (((uint)k[1]) << 8); goto case 1;
                        case 1:
                            a += k[0]; break;
                        case 0:
                            hash = BitConverter.GetBytes(((ulong)c << 32) | (ulong)b);
                            return;
                    }
                }

                Final();
                hash = BitConverter.GetBytes(((ulong)c << 32) | (ulong)b);
            }
        }

        protected override byte[] HashFinal()
        {
            return hash;
        }
    }
}
