using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chihuahua
{
    public enum ParameterType
    {
        Int,
        String
    }

    public struct Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public ParameterType Type { get; set; }

        public Parameter(Parameter src)
        {
            Name = src.Name;
            Value = src.Value;
            Type = src.Type;
        }

        public void ReadBinary(Stream strm)
        {
            switch (Type)
            {
                case ParameterType.Int:
                    byte[] buf = new byte[4];
                    strm.Read(buf, (int)strm.Position, 4);
                    Value = BitConverter.ToInt32(buf);

                    break;
                case ParameterType.String:
                    List<byte> str_buffer = new List<byte>();

                    byte test = 255;
                    while (test != 0)
                    {
                        test = (byte)strm.ReadByte();
                        str_buffer.Add(test);
                    }

                    Value = Encoding.ASCII.GetString(str_buffer.ToArray());

                    break;
            }
        }

        public void WriteToken(Stream strm)
        {
            switch (Type)
            {
                case ParameterType.Int:
                    string int_token = $"({ (int)Value })";
                    strm.Write(Encoding.ASCII.GetBytes(int_token), (int)strm.Position, int_token.Length);

                    break;
                case ParameterType.String:
                    string str_token = $"\"{ (string)Value }\"";
                    strm.Write(Encoding.ASCII.GetBytes(str_token), (int)strm.Position, str_token.Length);

                    break;
            }
        }

        public void WriteBinary(Stream strm)
        {
            switch (Type)
            {
                case ParameterType.Int:
                    int int_val = (int)Value;
                    strm.Write(BitConverter.GetBytes(int_val), (int)strm.Position, 4);
                    break;
                case ParameterType.String:
                    string str_val = (string)Value;
                    strm.Write(Encoding.ASCII.GetBytes(str_val), (int)strm.Position, str_val.Length);
                    strm.WriteByte(0);
                    break;
            }
        }

        public override string ToString()
        {
            return $"Name: { Name } - Type: { Type.ToString() }";
        }
    }
}
