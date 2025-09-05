using System.Net;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Items;
using Server.Object;
using ServerCore;

namespace Server {
    /// <summary>
    /// 클라이언트와 연결된 Session
    /// </summary>
    public class ClientSession : PacketSession {
        const int GachaPrice = 1000;
        object _lock = new object();
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

        public int SessionID { get; set; }
        public int AccountDBId { get; set; }
        public int CharacterDBId { get; set; }
        public RoomType roomType { get; set; }
        public Character MyCharacter { get; set; }
        public long lastPartyRequestTick = 0;
        bool _disconnected = false;
        bool _firstPing = true;
        long _lastSendTick = 0;
        long _pingpongTick = 0;
        int _immediate = 0;

        public void Ping() {
            if (!_firstPing) {
                long delta = Environment.TickCount64 - _pingpongTick;
                if (delta > 30 * 1000) {
                    Disconnect();
                    return;
                }
            }

            _firstPing = false;

            S_Ping pingPacket = new S_Ping();
            SendPacket(pingPacket);

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public void HandlePing() {
            _pingpongTick = Environment.TickCount64;
        }

        public void HandleLogout() {
            if (MyCharacter != null) {
                GameRoom room = GameLogic.Instance.GetRoom(RoomType.Login);
                LoginRoom loginRoom = room as LoginRoom;
                loginRoom.Push(loginRoom.LogoutAccount, MyCharacter.CharacterDBId);
                if (MyCharacter.Room != null && MyCharacter.party != null) {
                    GameLogic.Instance.Push(MyCharacter.Room.ExitParty, MyCharacter, MyCharacter.party[0]);
                }
            }
            GameLogic.Instance.Push(() => {
                if (MyCharacter == null) {
                    return;
                }
                if (MyCharacter.Room == null) {
                    return;
                }
                MyCharacter.Room.Push(MyCharacter.Room.LeaveGame, MyCharacter, this);
            });
        }

        public void HandleEnterGame(RoomType roomType, bool finishSessionGame = false) {
            this.roomType = roomType;
            if (MyCharacter == null || _disconnected == true) {
                return;
            }

            if (!finishSessionGame) {
                using (AppDbContext db = new AppDbContext()) {
                    List<ItemDB> items = db.Items.AsNoTracking().Where(i => i.OwnerDBId == CharacterDBId).ToList();

                    foreach (ItemDB itemDb in items) {
                        Item item = Item.MakeItem(itemDb);
                        if (item != null) {
                            MyCharacter.Inven.Add(item);
                        }
                    }
                }
            }

            GameRoom room = GameLogic.Instance.GetRoom(RoomType.MainIsland);
            MyCharacter.Room = room;
            room.Push(room.EnterGame, MyCharacter);
        }

        public void GachaItem() {
            S_GachaItem gachaPacket = new S_GachaItem() {
                CharacterDBId = CharacterDBId,
            };
            if (MyCharacter.Money < GachaPrice) {
                //구매 못함
                gachaPacket.Ok = false;
                gachaPacket.ErrorCode = 1;
                SendPacket(gachaPacket);
                return;
            }
            List<int> ownedItem = new List<int>();
            foreach (Item item in MyCharacter.Inven.GetAll()) {
                ownedItem.Add(item.TemplateId);
            }
            int itemId = Gacha.Instance.GachaItem(ownedItem);
            if (itemId == -1) {
                gachaPacket.Ok = false;
                gachaPacket.ErrorCode = 5;
                SendPacket(gachaPacket);
                return;
            }

            DbTransaction.GachaItem(MyCharacter, itemId, GachaPrice);
        }

        public void GetInven() {
            if (MyCharacter == null) {
                return;
            }
            MyCharacter.GetInventory();
        }

        public void HandleEquipItem(int characterDBId, int itemDbId, bool equipped) {
            if (MyCharacter.CharacterDBId != characterDBId) {
                return;
            }

            if (MyCharacter == null || MyCharacter.Room == null) {
                return;
            }
            MyCharacter.Room.Push(MyCharacter.Room.HandleEquipItem, MyCharacter, itemDbId, equipped);
        }

        public void HandleAcceptParty(C_AcceptParty acceptPartyPacket) {
            if (Environment.TickCount64 - lastPartyRequestTick > 60 * 1000) {
                S_AcceptParty sAcceptPartyPacket = new S_AcceptParty();
                sAcceptPartyPacket.Ok = false;
                sAcceptPartyPacket.ErrorMessage = "유효시간이 지난 요청입니다";
                sAcceptPartyPacket.ErrorCode = 5;
                sAcceptPartyPacket.CharacterDBId = CharacterDBId;
                SendPacket(sAcceptPartyPacket);
                return;
            }

            GameRoom room = MyCharacter.Room;
            if (acceptPartyPacket.Answer) {
                room.Push(room.AcceptParty, MyCharacter, acceptPartyPacket.PartyId);
            }
            else {
                room.Push(room.DeclineParty, acceptPartyPacket.PartyId);
            }
        }

        #region Network
        /// <summary>
        /// 클라이언트로 패킷 보내기 위해 배열에 넣기
        /// </summary>
        /// <param name="packet">보낼 패킷</param>
        /// <param name="immediate">0이면 400ms마다 전송, 1이면 다음 프레임에 바로 전송</param>
        public void SendPacket(IPacket packet, int immediate = 0) {
            if (_disconnected) {
                return;
            }
            string packetName = packet.GetType().Name;

            //Debug
            if (ConfigManager.Config.Deploy == false) {
                if (packetName != "S_Ping" && packetName != "S_CheckTime" && packetName != "S_DespawnObject" && packetName != "S_NPCMove") {
                    Console.WriteLine($"Packet Name: {packetName}");
                }
            }

            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            byte[] serialize = MessagePackSerializer.Serialize(packet);

            ushort size = (ushort)serialize.Length;
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(serialize, 0, sendBuffer, 4, size);

            Interlocked.CompareExchange(ref _immediate, immediate, 0);
            
            lock (_lock) {
                _reserveQueue.Add(sendBuffer);
            }
        }

        /// <summary>
        /// 매 프레임마다 실행이 되어 SendPacket의 immediate 값에 따라 전송
        /// </summary>
        public void FlushSend() {
            List<ArraySegment<byte>> sendList = null;

            lock (_lock) {
                long delta = Environment.TickCount64 - _lastSendTick;
                if (_immediate > 0) {
                    delta = 500;
                }
                if (delta < 400) {
                    return;
                }
                if (_reserveQueue.Count == 0) {
                    return;
                }
                _lastSendTick = Environment.TickCount64;
                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }
            Interlocked.Exchange(ref _immediate, 0);

            Send(sendList);
        }

        /// <summary>
        /// 클라이언트와 연결이 된 직후 실행할 명령어들
        /// </summary>
        /// <param name="endPoint">접속한 대상의 IP</param>
        public override void OnConnected(EndPoint endPoint) {
            GameLogic.Instance.PushAfter(5000, Ping);
        }

        /// <summary>
        /// 클라이언트와 연결이 끊어진 직후 실행할 명령어들
        /// </summary>
        /// <param name="endPoint">대상의 IP</param>
        public override void OnDisconnected(EndPoint endPoint) {
            if (MyCharacter != null) {
                GameRoom room = GameLogic.Instance.GetRoom(RoomType.Login);
                LoginRoom loginRoom = room as LoginRoom;
                loginRoom.Push(loginRoom.LogoutAccount, MyCharacter.CharacterDBId);
                if (MyCharacter.Room != null && MyCharacter.party != null) {
                    MyCharacter.Room.Push(MyCharacter.Room.ExitParty, MyCharacter, MyCharacter.party[0]);
                }
            }
            if (MyCharacter != null && MyCharacter.Room != null) {
                MyCharacter.Room.Push(MyCharacter.Room.LeaveGame, MyCharacter, this);
            }
            _disconnected = true;
            SessionManager.Instance.Remove(this);
        }

        /// <summary>
        /// 패킷을 Receive한 직후 받은 패킷에 따른 함수가 있는 PacketManager 명령어 실행
        /// </summary>
        /// <param name="buffer"패킷 모음></param>
        public override bool OnRecvPacket(ArraySegment<byte> buffer) {
            if (!PacketManager.Instance.OnRecvPacket(this, buffer)) {
                Disconnect(true);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 클라이언트에게 패킷을 보낸 직후 실행할 명령어들
        /// </summary>
        /// <param name="numOfBytes">보낸 패킷들의 용량</param>
        public override void OnSend(int numOfBytes) {

        }
        #endregion
    }
}
