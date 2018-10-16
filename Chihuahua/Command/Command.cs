using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
            switch (Token)
            {
                // RAMDOMJUMP can have a variable number of parameters,
                // so we have to deal with it separately from the others.
                case "RAMDOMJMP":
                    Parameters[0].ReadBinary(strm); // Load in the number of paths
                    List<int> path_offsets = new List<int>();

                    // Load the path offsets
                    for (int i = 0; i < (int)Parameters[0].Value; i++)
                    {
                        byte[] buf = new byte[4];
                        strm.Read(buf, 0, 4);
                        path_offsets.Add(BitConverter.ToInt32(buf));
                    }

                    Parameters[1].Value = path_offsets;
                    break;
                case "CHOICE":
                    Parameters[0].ReadBinary(strm); // Load in the number of choices
                    List<int> choice_text_ids = new List<int>();

                    // Load the choice text ids
                    for (int i = 0; i < (int)Parameters[0].Value; i++)
                    {
                        byte[] buf = new byte[4];
                        strm.Read(buf, 0, 4);
                        choice_text_ids.Add(BitConverter.ToInt32(buf));
                    }

                    Parameters[1].Value = choice_text_ids;

                    List<int> choice_offsets = new List<int>();

                    // Load the choice offsets
                    for (int i = 0; i < (int)Parameters[0].Value; i++)
                    {
                        byte[] buf = new byte[4];
                        strm.Read(buf, 0, 4);
                        choice_offsets.Add(BitConverter.ToInt32(buf));
                    }

                    Parameters[2].Value = choice_offsets;
                    break;
                default:
                    for (int i = 0; i < ParameterCount; i++)
                    {
                        Parameters[i].ReadBinary(strm);
                    }
                    break;
            }
      }

        public void WriteToken(Stream strm)
        {
            string token_str = $"<{ Token }>";
            strm.Write(Encoding.ASCII.GetBytes(token_str), 0, token_str.Length);

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
            strm.Write(BitConverter.GetBytes(ID), 0, 4);

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
