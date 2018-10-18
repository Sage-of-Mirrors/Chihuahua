using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Chihuahua
{
    public struct Branch
    {
        public List<long> BranchAddresses;
        public List<int> ParameterMap;
        public List<string> Labels;

        public Command Source;
    }

    public class Event
    {
        /// <summary>
        /// The command data that makes up the event.
        /// </summary>
        public List<Command> Commands { get; set; }

        /// <summary>
        /// The templates used to create new commands as they are loaded from file.
        /// </summary>
        private List<Command> m_CommandTemplates;
        private SortedDictionary<long, Command> raw_commands;

        public Event()
        {
            raw_commands = new SortedDictionary<long, Command>();
            Commands = new List<Command>();

            m_CommandTemplates = JsonConvert.DeserializeObject<List<Command>>(Encoding.ASCII.GetString(Properties.Resources.commands));
        }

        /// <summary>
        /// Loads event data from the .bev files found in Luigi's Mansion 3D.
        /// </summary>
        /// <param name="file_data">Byte array representing the .bev file</param>
        public void LoadBinary(byte[] file_data)
        {
            List<Branch> branches = new List<Branch>();

            using (MemoryStream mem = new MemoryStream(file_data))
            {
                while (mem.Position != mem.Length)
                {
                    ReadCommand(mem, branches);
                }
            }

            foreach (Branch br in branches)
            {
                for (int i = 0; i < br.BranchAddresses.Count; i++)
                {
                    if (raw_commands.ContainsKey(br.BranchAddresses[i] - 1))
                    {
                        Command old_case = raw_commands[br.BranchAddresses[i] - 1];
                        br.Source.Parameters[br.ParameterMap[i]].Value = old_case.Parameters[0].Value;
                    }
                    else
                    {
                        Command new_case = new Command(m_CommandTemplates.Find(x => x.Token == "CASE"));
                        new_case.Parameters[0].Value = br.Labels[i];

                        raw_commands.Add(br.BranchAddresses[i] - 1, new_case);
                    }
                }
            }

            long[] keys = new long[raw_commands.Count];
            raw_commands.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                Commands.Add(raw_commands[keys[i]]);
            }
        }

        private void ReadCommand(Stream strm, List<Branch> branches)
        {
            long cur_pos = strm.Position;

            byte[] buf = new byte[2];
            int bytes_red = strm.Read(buf, 0, 2);

            ushort command_id = BitConverter.ToUInt16(buf);

            Command cmd = new Command(m_CommandTemplates.Find(x => x.ID == command_id));
            cmd.ReadBinary(strm);
            raw_commands.Add(cur_pos, cmd);

            if (cmd.Token == "CHECKFLAG")
            {
                long base_offset = strm.Position - 8;

                Branch brnch = new Branch();

                brnch.BranchAddresses = new List<long>() { (int)cmd.Parameters[1].Value + base_offset, (int)cmd.Parameters[2].Value + base_offset };
                brnch.Labels = new List<string>() { $"{ cmd.Parameters[0].Value }on", $"{ cmd.Parameters[0].Value }off" };

                brnch.ParameterMap = new List<int>() { 1, 2 };

                cmd.ParameterCount = 3;
                cmd.Parameters = new Parameter[] { new Parameter() { Name = "Flag", Value = cmd.Parameters[0].Value, Type = ParameterType.Int },
                                                   new Parameter() { Name = "TrueLabel", Value = brnch.Labels[0], Type = ParameterType.String },
                                                   new Parameter() { Name = "FalseLabel", Value = brnch.Labels[1], Type = ParameterType.String } };

                brnch.Source = cmd;
                branches.Add(brnch);
            }
            else if (cmd.Token == "RAMDOMJMP")
            {
                long base_offset = strm.Position - (4 * (int)cmd.Parameters[0].Value);

                Branch brnch = new Branch();

                brnch.BranchAddresses = new List<long>();
                brnch.Labels = new List<string>();
                brnch.ParameterMap = new List<int>();
                List<Parameter> new_params = new List<Parameter>();

                List<int> original_branches = (List<int>)cmd.Parameters[1].Value;

                for (int i = 0; i < (int)cmd.Parameters[0].Value; i++)
                {
                    brnch.BranchAddresses.Add(base_offset + original_branches[i]);

                    string label = $"rjmp_{ base_offset }_{ i }";
                    brnch.Labels.Add(label);

                    brnch.ParameterMap.Add(i);

                    Parameter p = new Parameter() { Name = $"Jump_{ i }", Value = label, Type = ParameterType.String };
                    new_params.Add(p);
                }

                cmd.Parameters = new_params.ToArray();
                cmd.ParameterCount = brnch.BranchAddresses.Count;

                brnch.Source = cmd;
                branches.Add(brnch);
            }
        }

        /// <summary>
        /// NOT IMPLEMENTED!
        /// Loads event data from the .txt files found in Luigi's Mansion.
        /// </summary>
        /// <param name="file_data">Byte array representing the .txt file</param>
        public void LoadText(byte[] file_data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the currently loaded event data to the .bev format found in Luigi's Mansion 3D.
        /// </summary>
        /// <returns>File data as a byte array</returns>
        public byte[] WriteBinary()
        {
            if (Commands.Count == 0)
            {
                return new byte[1];
            }

            byte[] data;

            using (MemoryStream mem = new MemoryStream())
            {
                foreach (Command cmd in Commands)
                {
                    cmd.WriteBinary(mem);
                }

                data = mem.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Writes the currently loaded event data to the .txt format found in Luigi's Mansion.
        /// </summary>
        /// <returns>File data as a byte array</returns>
        public byte[] WriteText()
        {
            if (Commands.Count == 0)
            {
                return new byte[1];
            }

            byte[] data;

            using (MemoryStream mem = new MemoryStream())
            {
                foreach (Command cmd in Commands)
                {
                    cmd.WriteToken(mem);
                }

                data = mem.ToArray();
            }

            return data;
        }
    }
}
