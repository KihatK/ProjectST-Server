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
		_handler.Add((ushort)PacketID.S_CreateAccount, PacketHandler.S_CreateAccountHandler);		
		_handler.Add((ushort)PacketID.S_CreateCharacter, PacketHandler.S_CreateCharacterHandler);		
		_handler.Add((ushort)PacketID.S_Login, PacketHandler.S_LoginHandler);		
		_handler.Add((ushort)PacketID.S_Logout, PacketHandler.S_LogoutHandler);		
		_handler.Add((ushort)PacketID.S_EnterGame, PacketHandler.S_EnterGameHandler);		
		_handler.Add((ushort)PacketID.S_Move, PacketHandler.S_MoveHandler);		
		_handler.Add((ushort)PacketID.S_MoveEnd, PacketHandler.S_MoveEndHandler);		
		_handler.Add((ushort)PacketID.S_Spawn, PacketHandler.S_SpawnHandler);		
		_handler.Add((ushort)PacketID.S_SpawnObject, PacketHandler.S_SpawnObjectHandler);		
		_handler.Add((ushort)PacketID.S_Despawn, PacketHandler.S_DespawnHandler);		
		_handler.Add((ushort)PacketID.S_DespawnObject, PacketHandler.S_DespawnObjectHandler);		
		_handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);		
		_handler.Add((ushort)PacketID.S_MakeParty, PacketHandler.S_MakePartyHandler);		
		_handler.Add((ushort)PacketID.S_InviteParty, PacketHandler.S_InvitePartyHandler);		
		_handler.Add((ushort)PacketID.S_GotPartyRequest, PacketHandler.S_GotPartyRequestHandler);		
		_handler.Add((ushort)PacketID.S_AcceptParty, PacketHandler.S_AcceptPartyHandler);		
		_handler.Add((ushort)PacketID.S_BanishParty, PacketHandler.S_BanishPartyHandler);		
		_handler.Add((ushort)PacketID.S_ExitParty, PacketHandler.S_ExitPartyHandler);		
		_handler.Add((ushort)PacketID.S_EnterSessionGameRequest, PacketHandler.S_EnterSessionGameRequestHandler);		
		_handler.Add((ushort)PacketID.S_EnterSessionGame, PacketHandler.S_EnterSessionGameHandler);		
		_handler.Add((ushort)PacketID.S_StartInteract, PacketHandler.S_StartInteractHandler);		
		_handler.Add((ushort)PacketID.S_FinishInteract, PacketHandler.S_FinishInteractHandler);		
		_handler.Add((ushort)PacketID.S_TimeOver, PacketHandler.S_TimeOverHandler);		
		_handler.Add((ushort)PacketID.S_FinishSessionGame, PacketHandler.S_FinishSessionGameHandler);		
		_handler.Add((ushort)PacketID.S_MoveRoom, PacketHandler.S_MoveRoomHandler);		
		_handler.Add((ushort)PacketID.S_Ping, PacketHandler.S_PingHandler);		
		_handler.Add((ushort)PacketID.S_ActiveSkill, PacketHandler.S_ActiveSkillHandler);		
		_handler.Add((ushort)PacketID.S_RemoveScarecrow, PacketHandler.S_RemoveScarecrowHandler);		
		_handler.Add((ushort)PacketID.S_ChangeSeason, PacketHandler.S_ChangeSeasonHandler);		
		_handler.Add((ushort)PacketID.S_AnimalMove, PacketHandler.S_AnimalMoveHandler);		
		_handler.Add((ushort)PacketID.S_AnimalMoveEnd, PacketHandler.S_AnimalMoveEndHandler);		
		_handler.Add((ushort)PacketID.S_RemoveFieldObjectByAnimal, PacketHandler.S_RemoveFieldObjectByAnimalHandler);		
		_handler.Add((ushort)PacketID.S_CheckTime, PacketHandler.S_CheckTimeHandler);		
		_handler.Add((ushort)PacketID.S_ChangeObjectHp, PacketHandler.S_ChangeObjectHpHandler);		
		_handler.Add((ushort)PacketID.S_ChangeObject, PacketHandler.S_ChangeObjectHandler);		
		_handler.Add((ushort)PacketID.S_ChangeHp, PacketHandler.S_ChangeHpHandler);		
		_handler.Add((ushort)PacketID.S_Exhaust, PacketHandler.S_ExhaustHandler);		
		_handler.Add((ushort)PacketID.S_SenseSeeds, PacketHandler.S_SenseSeedsHandler);		
		_handler.Add((ushort)PacketID.S_EquipItem, PacketHandler.S_EquipItemHandler);		
		_handler.Add((ushort)PacketID.S_GetInventory, PacketHandler.S_GetInventoryHandler);		
		_handler.Add((ushort)PacketID.S_GetItem, PacketHandler.S_GetItemHandler);		
		_handler.Add((ushort)PacketID.S_ThrowItem, PacketHandler.S_ThrowItemHandler);		
		_handler.Add((ushort)PacketID.S_PurchaseItem, PacketHandler.S_PurchaseItemHandler);		
		_handler.Add((ushort)PacketID.S_SellItem, PacketHandler.S_SellItemHandler);		
		_handler.Add((ushort)PacketID.S_GachaItem, PacketHandler.S_GachaItemHandler);		
		_handler.Add((ushort)PacketID.S_Emotion, PacketHandler.S_EmotionHandler);		
		_handler.Add((ushort)PacketID.S_NPCMove, PacketHandler.S_NPCMoveHandler);		
		_handler.Add((ushort)PacketID.S_PayOffDept, PacketHandler.S_PayOffDeptHandler);		
		_handler.Add((ushort)PacketID.S_EnhanceSkill, PacketHandler.S_EnhanceSkillHandler);		
		_handler.Add((ushort)PacketID.S_ChooseSkill, PacketHandler.S_ChooseSkillHandler);		
		_handler.Add((ushort)PacketID.S_PartyMemberReady, PacketHandler.S_PartyMemberReadyHandler);		
		_handler.Add((ushort)PacketID.S_GetRanking, PacketHandler.S_GetRankingHandler);		
		_handler.Add((ushort)PacketID.S_LoadingComplete, PacketHandler.S_LoadingCompleteHandler);
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