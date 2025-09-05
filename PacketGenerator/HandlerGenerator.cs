using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator {
    class HandlerGenerator {
        static string clientRegister;
        static string serverRegister;
        public static void Generate() {
            string file = "../../../../Server/Packet/PacketGenerated.cs";

            int validPacketId = -1;
            bool startParsing = false;

            foreach (string line in File.ReadAllLines(file)) {
                if (!startParsing && line.Contains("enum PacketID")) {
                    startParsing = true;
                    continue;
                }
                
                if (!startParsing) {
                    continue;
                }

                if (line.Contains("}")) {
                    break;
                }

                string[] names = line.Trim().Split(" =");
                if (names.Length == 0) {
                    continue;
                }

                string name = names[0];
                if (name.StartsWith("S_")) {
                    clientRegister += string.Format(HandlerFormat.managerRegisterFormat, name);
                    validPacketId++;
                }
                else if (name.StartsWith("C_")) {
                    serverRegister += string.Format(HandlerFormat.managerRegisterFormat, name);
                    validPacketId++;
                }
            }

            string clientManagerText = string.Format(HandlerFormat.managerFormat, clientRegister, validPacketId);
            File.WriteAllText("ClientPacketManager.cs", clientManagerText);
            File.WriteAllText("../../../../DummyClient/Packet/ClientPacketManager.cs", clientManagerText);
            File.WriteAllText("../../../../../Client/Assets/Scripts/Packet/ClientPacketManager.cs", clientManagerText);
            string serverManagerText = string.Format(HandlerFormat.managerFormat, serverRegister, validPacketId);
            File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            File.WriteAllText("../../../../Server/Packet/ServerPacketManager.cs", serverManagerText);
        }
    }
}
