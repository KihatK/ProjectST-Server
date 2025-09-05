using System;
using System.IO;

namespace PacketGenerator {
    class Program {
        static string resultString = "";
        static string resultStringClient = "";
        static List<string> packetNameList = new List<string>();

        static void Main(string[] args) {
            string file = "../../..//PacketProtocol.cs";
            if (args.Length >= 1) {
                file = args[0];
            }

            bool startPacketParsing= false;
            bool startEnumParsing = false;
            int order = 0;
            string keyString = "";
            string packetString = "";
            string unionString = "";
            string packetIDString = "";
            string enumString = "";

            string packetName = "";
            foreach (string line in File.ReadAllLines(file)) {
                if (!startPacketParsing && !startEnumParsing) {
                    if (line.Contains("class")) {
                        startPacketParsing= true;
                        packetName = line.Split(" ")[2];
                    }
                    else if (line.Contains("enum")) {
                        enumString += line + "\n";
                        startEnumParsing = true;
                    }
                    continue;
                }

                if (line == "}") {
                    if (startPacketParsing) {
                        packetString += string.Format(PacketFormat.packetFormat, packetName, keyString);
                        packetNameList.Add(packetName);
                        startPacketParsing = false;
                        order = 0;
                        keyString = "";
                    }
                    else if (startEnumParsing) {
                        enumString += line + "\n";
                        startEnumParsing= false;
                    }
                    continue;
                }

                //Key
                if (line == "") {
                    //No Key
                    continue;
                }
                if (startPacketParsing) {
                    keyString += string.Format(PacketFormat.keyFormat, order++, line.Trim());
                }
                else if (startEnumParsing) {
                    enumString += line + "\n";
                }
            }

            for (int i=0; i<packetNameList.Count; i++) {
                packetIDString += string.Format(PacketFormat.packetIDFormat, packetNameList[i], i);
                unionString += string.Format(PacketFormat.unionFormat, packetNameList[i], i);
            }

            resultString += string.Format(PacketFormat.packetPackageFormat, packetIDString, unionString, packetString);
            resultStringClient += string.Format(PacketFormat.packetPackageFormatClient, packetIDString, unionString, packetString);
            File.WriteAllText("../../../../Server/Packet/PacketGenerated.cs", resultString + enumString);
            File.WriteAllText("../../../../DummyClient/Packet/PacketGenerated.cs", resultString + enumString);
            File.WriteAllText("../../../../../Client/Assets/Scripts/Packet/PacketGenerated.cs", resultStringClient + enumString);
            File.WriteAllText("PacketGenerated.cs", resultString + enumString);

            HandlerGenerator.Generate();
        }
    }
}