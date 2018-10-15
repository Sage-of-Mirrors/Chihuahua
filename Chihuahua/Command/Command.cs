using System;
using System.IO;
using System.Text;

namespace Chihuahua
{
    public struct Command
    {
        public string Token { get; set; }
        public ushort ID { get; set; }
        public int ParameterCount { get; set; }
        public bool AppendNewline { get; set; }

        public Parameter[] Parameters { get; set; }

        public Command(Command src)
        {
            Token = src.Token;
            ID = src.ID;
            ParameterCount = src.ParameterCount;
            AppendNewline = src.AppendNewline;

            Parameters = new Parameter[ParameterCount];
            for (int i = 0; i < ParameterCount; i++)
            {
                Parameters[i] = new Parameter(src.Parameters[i]);
            }
        }

        public void ReadBinary(Stream strm)
        {
            for (int i = 0; i < ParameterCount; i++)
            {
                Parameters[i].ReadBinary(strm);
            }
        }

        public void WriteToken(Stream strm)
        {
            string token_str = $"<{ Token }>";
            strm.Write(Encoding.ASCII.GetBytes(token_str), (int)strm.Position, token_str.Length);

            for (int i = 0; i < ParameterCount; i++)
            {
                Parameters[i].WriteToken(strm);
            }

            if (AppendNewline)
            {
                strm.WriteByte((byte)'\n');
            }
        }

        public void WriteBinary(Stream strm)
        {
            strm.Write(BitConverter.GetBytes(ID), (int)strm.Position, 4);

            for (int i = 0; i < ParameterCount; i++)
            {
                Parameters[i].WriteBinary(strm);
            }
        }

        public override string ToString()
        {
            return $"Token: { Token } - ID: { ID }";
        }
    }
}
