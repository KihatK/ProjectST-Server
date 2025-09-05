using MessagePack;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient {
    class ServerSession : PacketSession{

        public int DummmyID { get; set; }
        public int AccountID { get; set; }
        public int CharacterDBId { get; set; }
        public int skillId { get; set; }
        public int partyId { get; set; }
        public List<int> party = new List<int>();
        public float posX = 0;
        public float posY = 0;
        public float posZ = 0;

        public void SendPacket(IPacket packet) {
            string packetName = packet.GetType().Name;
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            byte[] serialize = MessagePackSerializer.Serialize(packet);

            ushort size = (ushort)serialize.Length;
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(serialize, 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint) {
            //계정생성 과정
            C_Login loginPacket = new C_Login();
            //loginPacket.Username = Console.ReadLine();
            //loginPacket.Password = Console.ReadLine();
            loginPacket.Username = $"admin{DummmyID+10}";
            loginPacket.Password = "1234";
            SendPacket(loginPacket);

            //C_EnterGame enterPacket = new C_EnterGame();
            //SendPacket(enterPacket);
        }

        public override void OnDisconnected(EndPoint endPoint) {

        }

        public override bool OnRecvPacket(ArraySegment<byte> buffer) {
            if (!PacketManager.Instance.OnRecvPacket(this, buffer)) {
                return false;
            }
            return true;
        }

        public override void OnSend(int numOfBytes) {

        }
    }
}
