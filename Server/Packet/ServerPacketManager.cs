using MessagePack;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager {
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }

	PacketManager() {
		Register();
	}

	Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

	public Action<PacketSession, IPacket, ushort> CustomHandler { get; set; }
		
	public void Register() {		
		_handler.Add((ushort)PacketID.C_CreateAccount, PacketHandler.C_CreateAccountHandler);		
		_handler.Add((ushort)PacketID.C_CreateCharacter, PacketHandler.C_CreateCharacterHandler);		
		_handler.Add((ushort)PacketID.C_Login, PacketHandler.C_LoginHandler);		
		_handler.Add((ushort)PacketID.C_Logout, PacketHandler.C_LogoutHandler);		
		_handler.Add((ushort)PacketID.C_EnterGame, PacketHandler.C_EnterGameHandler);		
		_handler.Add((ushort)PacketID.C_Move, PacketHandler.C_MoveHandler);		
		_handler.Add((ushort)PacketID.C_MoveEnd, PacketHandler.C_MoveEndHandler);		
		_handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler);		
		_handler.Add((ushort)PacketID.C_MakeParty, PacketHandler.C_MakePartyHandler);		
		_handler.Add((ushort)PacketID.C_InviteParty, PacketHandler.C_InvitePartyHandler);		
		_handler.Add((ushort)PacketID.C_AcceptParty, PacketHandler.C_AcceptPartyHandler);		
		_handler.Add((ushort)PacketID.C_BanishParty, PacketHandler.C_BanishPartyHandler);		
		_handler.Add((ushort)PacketID.C_ExitParty, PacketHandler.C_ExitPartyHandler);		
		_handler.Add((ushort)PacketID.C_EnterSessionGameRequest, PacketHandler.C_EnterSessionGameRequestHandler);		
		_handler.Add((ushort)PacketID.C_EnterSessionGame, PacketHandler.C_EnterSessionGameHandler);		
		_handler.Add((ushort)PacketID.C_StartInteract, PacketHandler.C_StartInteractHandler);		
		_handler.Add((ushort)PacketID.C_FinishInteract, PacketHandler.C_FinishInteractHandler);		
		_handler.Add((ushort)PacketID.C_FinishSessionGame, PacketHandler.C_FinishSessionGameHandler);		
		_handler.Add((ushort)PacketID.C_MoveRoom, PacketHandler.C_MoveRoomHandler);		
		_handler.Add((ushort)PacketID.C_Ping, PacketHandler.C_PingHandler);		
		_handler.Add((ushort)PacketID.C_ActiveSkill, PacketHandler.C_ActiveSkillHandler);		
		_handler.Add((ushort)PacketID.C_ChangeSeasonByButton, PacketHandler.C_ChangeSeasonByButtonHandler);		
		_handler.Add((ushort)PacketID.C_EquipItem, PacketHandler.C_EquipItemHandler);		
		_handler.Add((ushort)PacketID.C_GetInventory, PacketHandler.C_GetInventoryHandler);		
		_handler.Add((ushort)PacketID.C_GetItem, PacketHandler.C_GetItemHandler);		
		_handler.Add((ushort)PacketID.C_ThrowItem, PacketHandler.C_ThrowItemHandler);		
		_handler.Add((ushort)PacketID.C_PurchaseItem, PacketHandler.C_PurchaseItemHandler);		
		_handler.Add((ushort)PacketID.C_SellItem, PacketHandler.C_SellItemHandler);		
		_handler.Add((ushort)PacketID.C_GachaItem, PacketHandler.C_GachaItemHandler);		
		_handler.Add((ushort)PacketID.C_Emotion, PacketHandler.C_EmotionHandler);		
		_handler.Add((ushort)PacketID.C_PayOffDept, PacketHandler.C_PayOffDeptHandler);		
		_handler.Add((ushort)PacketID.C_EnhanceSkill, PacketHandler.C_EnhanceSkillHandler);		
		_handler.Add((ushort)PacketID.C_ChooseSkill, PacketHandler.C_ChooseSkillHandler);		
		_handler.Add((ushort)PacketID.C_GetRanking, PacketHandler.C_GetRankingHandler);		
		_handler.Add((ushort)PacketID.C_LoadingComplete, PacketHandler.C_LoadingCompleteHandler);
	}

	public bool OnRecvPacket(PacketSession session, ArraySegment<byte> buffer) {
		ushort count = 0;

		//최소한 4바이트(2바이트씩 2번) 읽을 수 있는지 확인
        if (buffer.Array.Length == null || buffer.Array.Length < buffer.Offset + 4) {
			return false;
		}

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		if (size <= 0 || size > 65535 || id < 0 || id > 87) {
			//이상한 요청
			return false;
		}

		// 추가 검사: 버퍼에 size 만큼의 데이터가 들어 있는지 확인
        // size 값에 따라 버퍼의 길이가 충분한지 확인하는 추가 검증
        if (buffer.Array.Length < buffer.Offset + size) {
            // 버퍼에 예상되는 패킷 크기만큼의 데이터가 없으면 잘못된 요청으로 처리
            return false;
        }

		if (!MakePacket(session, buffer, id, size)) {
			return false;
		}
		return true;
	}

	bool MakePacket(PacketSession session, ArraySegment<byte> buffer, ushort id, ushort size) {
		// size가 최소한 4보다 큰지 검사 (헤더를 포함한 최소 크기)
		if (size <= 4 || buffer.Array == null || buffer.Array.Length < buffer.Offset + size) {
			// 잘못된 패킷 크기이거나, 버퍼 크기가 충분하지 않으면 반환
			Console.WriteLine("잘못된 패킷 또는 버퍼 크기 부족");
			return false;
		}
		
		byte[] pktWithoutHeader = new byte[size - 4];
		Array.Copy(buffer.Array, buffer.Offset + 4, pktWithoutHeader, 0, size - 4);
        IPacket pkt;
		try {
			pkt = MessagePackSerializer.Deserialize<IPacket>(pktWithoutHeader);
		}
		catch (Exception ex) {
			Console.WriteLine($"패킷 역직렬화 중 오류 발생: {ex.Message}");
			return false;
		}

		if (CustomHandler != null) {
			CustomHandler.Invoke(session, pkt, id);
		}
		else {
			Action<PacketSession, IPacket> action = null;
			if (_handler.TryGetValue(id, out action)) {
				action.Invoke(session, pkt);
			}
		}
		return true;
	}
	
	public Action<PacketSession, IPacket> GetPacketHandler(ushort id) {
		Action<PacketSession, IPacket> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}