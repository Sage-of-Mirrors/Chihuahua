using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Chihuahua
{
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

        public Event()
        {
            Commands = new List<Command>();

            m_CommandTemplates = JsonConvert.DeserializeObject<List<Command>>(Encoding.ASCII.GetString(Properties.Resources.commands));
        }

        /// <summary>
        /// Loads event data from the .bev files found in Luigi's Mansion 3D.
        /// </summary>
        /// <param name="file_data">Byte array representing the .bev file</param>
        public void LoadBinary(byte[] file_data)
        {
            Stack<long> branch_read_positions = new Stack<long>();

            using (MemoryStream mem = new MemoryStream(file_data))
            {
                while (mem.Position != mem.Length)
                {
                    ReadCommand(mem, branch_read_positions);
                }
            }
        }

        private void ReadCommand(Stream strm, Stack<long> read_positions)
        {
            byte[] buf = new byte[2];
            int bytes_red = strm.Read(buf, 0, 2);

            ushort command_id = BitConverter.ToUInt16(buf);

            Command cmd = new Command(m_CommandTemplates.Find(x => x.ID == command_id));
            cmd.ReadBinary(strm);
            Commands.Add(cmd);
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
