﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace RtmpSharp2.Abstract
{
    
    public class AmfWriter
    {
        private MemoryStream memory;
        private EndianBinaryWriter writer;

        public AmfWriter()
        {
            memory = new MemoryStream();
            writer = new EndianBinaryWriter(EndianBitConverter.Big, memory);
        }

        public void WriteNumber(double number)
        {
            writer.Write((byte) Globals.Amf0Types.Number);
            writer.Write(number);
        }

        public void WriteBoolean(bool boolean)
        {
            writer.Write((byte) Globals.Amf0Types.Boolean);
            writer.Write(boolean);
        }

        public void WriteString(string str, bool objectStart = false)
        {
            if(objectStart == false)
                writer.Write((byte) Globals.Amf0Types.String);

            writer.Write((ushort) str.Length);
            
            for (var i = 0; i < str.Length; i++)
            {
                writer.Write(str[i]);
            }
        }

        public void WriteNull()
        {
            writer.Write((byte) Globals.Amf0Types.Null);
        }

        public void WriteObject(AmfObject amfObject, bool isArray = false)
        {
            if(!isArray)
            writer.Write((byte) Globals.Amf0Types.Object);
            else
            {
                writer.Write((byte) Globals.Amf0Types.Array);
                writer.Write(
                    (int)
                    (amfObject.Booleans.Count + amfObject.Numbers.Count + amfObject.Strings.Count + amfObject.Nulls));
            }
            foreach (var s in amfObject.Strings)
            {
                WriteString(s.Key, true);
                WriteString(s.Value);
            }

            foreach (var s in amfObject.Numbers)
            {
                WriteString(s.Key, true);
                WriteNumber(s.Value);
            }

            foreach (var s in amfObject.Booleans)
            {
                WriteString(s.Key, true);
                WriteBoolean(s.Value);
            }
            //objects end with 0x00,0x00, (oject end identifier [0x09 in this case])
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);
            writer.Write((byte) Globals.Amf0Types.ObjectEnd);
        }

        public byte[] GetByteArray()
        {
            return memory.ToArray();
        }
    }
}
