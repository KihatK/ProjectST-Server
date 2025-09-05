using Server.Game;
using Server.Job;
using Server.Items;
using Server.Object;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Redis;
using Oracle.ManagedDataAccess.Client;

namespace Server.DB {
    public partial class DbTransaction : JobSerializer {
        public static void GetItem(Character character, int templateId, int count = 1) {
            if (character == null || character.Room == null) {
                return;
            }
            S_GetItem itemPacket = new S_GetItem() {
                CharacterDBId = character.CharacterDBId,
            };

            int slot = character.Inven.GetEmptySlot();
            if (slot == null) {
                //인벤토리 창이 꽉 참
                itemPacket.Ok = false;
                itemPacket.ErrorCode = 1;
                character.Session.SendPacket(itemPacket);
                return;
            }

            ItemData itemData = null;
            if (!DataManager.ItemDict.TryGetValue(templateId, out itemData)) {
                itemPacket.Ok = false;
                itemPacket.ErrorCode = 2;
                character.Session.SendPacket(itemPacket);
                return;
            }

            ItemDB itemDB = new ItemDB() {
                TemplateId = templateId,
                Count = count,
                Slot = slot,
                Equipped = false,
                OwnerDBId = character.CharacterDBId,
            };

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    try {
                        db.Items.Add(itemDB);
                        bool success = db.SaveChangesEx();
                        if (success) {
                            Item newItem = Item.MakeItem(itemDB);
                            character.Inven.Add(newItem);

                            itemPacket.Ok = true;
                            itemPacket.ErrorCode = 0;
                            itemPacket.ItemInfoData = new ItemInfoData() {
                                ItemDBId = itemDB.ItemDBId,
                                TemplateId = itemDB.TemplateId,
                                Count = count,
                                Slot = slot,
                                Equip = itemDB.Equipped,
                                ItemType = itemData.itemType,
                            };
                            character.Session.SendPacket(itemPacket);
                        }
                        else {
                            itemPacket.Ok = false;
                            itemPacket.ErrorCode = 3;
                            character.Session.SendPacket(itemPacket);
                        }
                    }
                    catch (OracleException ex) {
                        S_GetItem itemPacket = new S_GetItem() {
                            CharacterDBId = character.CharacterDBId,
                        };
                        itemPacket.Ok = false;
                        itemPacket.ErrorCode = 100;
                        character.Session.SendPacket(itemPacket);
                        Console.WriteLine($"GetItem(OracleException) : {ex.ToString()}");
                    }
                    catch (Exception ex) {
                        S_GetItem itemPacket = new S_GetItem() {
                            CharacterDBId = character.CharacterDBId,
                        };
                        itemPacket.Ok = false;
                        itemPacket.ErrorCode = 200;
                        character.Session.SendPacket(itemPacket);
                        Console.WriteLine($"GetItem(Exception) : {ex.ToString()}");

                    }
                }
            });
        }

        public static void ThrowItem(Character character, int itemDBId, int count = 1) {
            if (character == null || character.Room == null) {
                return;
            }

            S_ThrowItem itemPacket = new S_ThrowItem() {
                CharacterDBId = character.CharacterDBId,
            };
            Item existItem = character.Inven.Find(i => (i.ItemDBId == itemDBId));
            if (existItem == null) {
                //인벤토리에 존재하지 않는 아이템
                itemPacket.Ok = false;
                itemPacket.ErrorCode = 1;
                character.Session.SendPacket(itemPacket);
                return;
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    try {
                        ItemDB findItem = db.Items.Find(itemDBId);

                        if (findItem == null || findItem.Count < count) {
                            //인벤토리에 해당 아이템이 없거나 수량이 부족합니다
                            itemPacket.Ok = false;
                            itemPacket.ErrorCode = 1;
                            character.Session.SendPacket(itemPacket);
                            return;
                        }
                        if (findItem.OwnerDBId != character.CharacterDBId) {
                            //소유주가 아닙니다
                            itemPacket.Ok = false;
                            itemPacket.ErrorCode = 2;
                            character.Session.SendPacket(itemPacket);
                            return;
                        }


                        findItem.Count -= count;
                        if (findItem.Count == 0) {
                            db.Items.Remove(findItem);
                        }

                        bool success = db.SaveChangesEx();
                        if (success) {
                            character.Inven.Remove(itemDBId, findItem.Count);
                            itemPacket.Ok = true;
                            itemPacket.ErrorCode = 0;
                            itemPacket.ItemInfoData = new ItemInfoData() {
                                ItemDBId = findItem.ItemDBId,
                                TemplateId = findItem.TemplateId,
                                Count = findItem.Count,
                                Slot = findItem.Slot,
                                Equip = findItem.Equipped,
                            };
                            character.Session.SendPacket(itemPacket);
                        }
                        else {
                            itemPacket.Ok = false;
                            itemPacket.ErrorCode = 3;
                            character.Session.SendPacket(itemPacket);
                        }
                    }
                    catch (OracleException ex) {
                        S_ThrowItem itemPacket = new S_ThrowItem() {
                            CharacterDBId = character.CharacterDBId,
                            Ok = false,
                            ErrorCode = 200,
                        };
                        character.Session.SendPacket(itemPacket);
                        Console.WriteLine($"ThrowItem(OracleException) : {ex.ToString()}");

                    }
                    catch (Exception ex) {
                        S_ThrowItem itemPacket = new S_ThrowItem() {
                            CharacterDBId = character.CharacterDBId,
                            Ok = false,
                            ErrorCode = 200,
                        };
                        character.Session.SendPacket(itemPacket);
                        Console.WriteLine($"ThrowItem(Exception) : {ex.ToString()}");
                    }

                }
            });
        }

        public static void GachaItem(Character character, int templateId, int price) {
            if (character == null || character.Room == null) {
                return;
            }
            S_GachaItem gachaPacket = new S_GachaItem() {
                CharacterDBId = character.CharacterDBId,
            };

            if (character.Money < price) {
                //구매 못함
                gachaPacket.Ok = false;
                gachaPacket.ErrorCode = 1;
                character.Session.SendPacket(gachaPacket);
                return;
            }
            int slot = character.Inven.GetEmptySlot();
            if (slot == -1) {
                //아이템 창이 다 참
                gachaPacket.Ok = false;
                gachaPacket.ErrorCode = 2;
                character.Session.SendPacket(gachaPacket);
                return;
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                        if (characterDB == null) {
                            gachaPacket.Ok = false;
                            gachaPacket.ErrorCode = 3;
                            character.Session.SendPacket(gachaPacket);
                            return;
                        }

                        if (characterDB.Money < price) {
                            gachaPacket.Ok = false;
                            gachaPacket.ErrorCode = 1;
                            character.Session.SendPacket(gachaPacket);
                            return;
                        }

                        characterDB.Money -= price;

                        ItemDB itemDB = new ItemDB() {
                            TemplateId = templateId,
                            Count = 1,
                            Slot = slot,
                            Equipped = false,
                            OwnerDBId = character.CharacterDBId,
                        };

                        db.Items.Add(itemDB);

                        db.SaveChanges();

                        transaction.Commit();

                        //Character Inven에 아이템 추가 및 클라로 결과 보내기
                        Item item = Item.MakeItem(itemDB);
                        character.Inven.Add(item);
                        character.Money = characterDB.Money;
                        gachaPacket.Money = characterDB.Money;
                        gachaPacket.Ok = true;
                        gachaPacket.ErrorCode = 0;
                        gachaPacket.ItemInfoData = new ItemInfoData() {
                            ItemDBId = item.ItemDBId,
                            TemplateId = item.TemplateId,
                            Count = item.Count,
                            Slot = item.Slot,
                            Equip = item.Equipped,
                            ItemType = item.ItemType,
                        };
                        character.Session.SendPacket(gachaPacket);
                        RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, characterDB.Nickname, characterDB.Money);
                    }
                    catch (OracleException ex) {
                        transaction.Rollback();
                        //구매 중 오류로 구매 실패했습니다
                        gachaPacket.Ok = false;
                        gachaPacket.ErrorCode = 200;
                        character.Session.SendPacket(gachaPacket);
                        Console.WriteLine($"GachaItem(OracleException) : {ex.ToString()}");
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        //구매 중 오류로 구매 실패했습니다
                        gachaPacket.Ok = false;
                        gachaPacket.ErrorCode = 4;
                        character.Session.SendPacket(gachaPacket);
                        Console.WriteLine($"GachaItem(Exception) : {ex.ToString()}");
                    }
                }
            });
        }

        public static void PurchaseItem(Character character, int templateId, int price, int count = 1) {
            if (character.Money < price) {
                //구매 못함
                S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                    CharacterDBId = character.CharacterDBId,
                    Ok = false,
                    ErrorCode = 1,
                };
                character.Session.SendPacket(purchasePacket);
                return;
            }
            int slot = character.Inven.GetEmptySlot();
            if (slot == -1) {
                //아이템 창이 다 참
                S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                    CharacterDBId = character.CharacterDBId,
                    Ok = false,
                    ErrorCode = 2,
                };
                character.Session.SendPacket(purchasePacket);
                return;
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                        if (characterDB == null) {
                            S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                                CharacterDBId = character.CharacterDBId,
                                Ok = false,
                                ErrorCode = 3,
                            };
                            character.Session.SendPacket(purchasePacket);
                            return;
                        }

                        if (characterDB.Money < price) {
                            S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                                CharacterDBId = character.CharacterDBId,
                                Ok = false,
                                ErrorCode = 1,
                            };
                            character.Session.SendPacket(purchasePacket);
                            return;
                        }

                        characterDB.Money -= price;

                        ItemDB itemDB = new ItemDB() {
                            TemplateId = templateId,
                            Count = count,
                            Slot = slot,
                            Equipped = false,
                            OwnerDBId = character.CharacterDBId,
                        };

                        db.Items.Add(itemDB);

                        db.SaveChanges();

                        transaction.Commit();

                        //Character Inven에 아이템 추가 및 클라로 결과 보내기
                        Item item = Item.MakeItem(itemDB);
                        PurchaseItemResponse(character, item, characterDB.Money);
                    }
                    catch (OracleException ex) {
                        transaction.Rollback();
                        //구매 중 오류로 구매 실패했습니다
                        S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                            CharacterDBId = character.CharacterDBId,
                            Ok = false,
                            ErrorCode = 200,
                        };
                        character.Session.SendPacket(purchasePacket);
                        Console.WriteLine($"PurchaseItem(OracleException) : {ex.ToString()}");
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        //구매 중 오류로 구매 실패했습니다
                        S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                            CharacterDBId = character.CharacterDBId,
                            Ok = false,
                            ErrorCode = 4,
                        };
                        character.Session.SendPacket(purchasePacket);
                        Console.WriteLine($"PurchaseItem(Exception) : {ex.ToString()}");
                    }
                }
            });
        }

        static void PurchaseItemResponse(Character character, Item item, int curMoney) {
            character.Inven.Add(item);
            character.Money = curMoney;
            S_PurchaseItem purchasePacket = new S_PurchaseItem() {
                CharacterDBId = character.CharacterDBId,
                Money = curMoney,
                Ok = true,
                ErrorCode = 0,
                ItemInfoData = new ItemInfoData() {
                    ItemDBId = item.ItemDBId,
                    TemplateId = item.TemplateId,
                    Count = item.Count,
                    Slot = item.Slot,
                    Equip = item.Equipped,
                },
            };
            character.Session.SendPacket(purchasePacket);
            RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, character.Nickname, character.Money);
        }

        public static void SellItem(Character character, int itemDBId, int price, int count = 1) {
            Item item = character.Inven.Get(itemDBId);
            if (item == null || item.Count < count) {
                S_SellItem sellPacket = new S_SellItem() {
                    CharacterDBId = character.CharacterDBId,
                    Ok = false,
                    ErrorCode = 1,
                };
                character.Session.SendPacket(sellPacket);
                return;
            }

            using (AppDbContext db = new AppDbContext())
            using (var transaction = db.Database.BeginTransaction()) {
                try {
                    S_SellItem sellPacket = new S_SellItem() {
                        CharacterDBId = character.CharacterDBId,
                    };
                    CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                    if (characterDB == null) {
                        //캐릭터를 찾을 수 없습니다
                        sellPacket.Ok = false;
                        sellPacket.ErrorCode = 3;
                        character.Session.SendPacket(sellPacket);
                        return;
                    }

                    ItemDB findItem = db.Items.Find(itemDBId);
                    if (findItem == null || findItem.Count < count) {
                        //인벤토리에 해당 아이템이 없거나 수량이 부족합니다
                        sellPacket.Ok = false;
                        sellPacket.ErrorCode = 1;
                        character.Session.SendPacket(sellPacket);
                        return;
                    }
                    if (findItem.OwnerDBId != character.CharacterDBId) {
                        //소유주가 아닙니다
                        sellPacket.Ok = false;
                        sellPacket.ErrorCode = 2;
                        character.Session.SendPacket(sellPacket);
                        return;
                    }

                    //판매시 50%
                    int totalPrice = price * count / 2;

                    characterDB.Money += totalPrice;
                    findItem.Count -= count;
                    if (findItem.Count == 0) {
                        db.Items.Remove(findItem);
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    //Character에 반영 및 클라로 브로드캐스트
                    character.Inven.Remove(itemDBId, findItem.Count);
                    character.Money = characterDB.Money;
                    sellPacket.Ok = true;
                    sellPacket.ErrorCode = 0;
                    sellPacket.Money = characterDB.Money;
                    sellPacket.ItemInfoData = new ItemInfoData() {
                        ItemDBId = findItem.ItemDBId,
                        TemplateId = findItem.TemplateId,
                        Count = findItem.Count,
                        Slot = findItem.Slot,
                        Equip = findItem.Equipped,
                    };
                    character.Session.SendPacket(sellPacket);
                    RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, character.Nickname, character.Money);
                }
                catch (OracleException ex) {
                    transaction.Rollback();
                    //판매 실패
                    S_SellItem sellPacket = new S_SellItem() {
                        CharacterDBId = character.CharacterDBId,
                        Ok = false,
                        ErrorCode = 200,
                    };
                    character.Session.SendPacket(sellPacket);
                    Console.WriteLine($"SellItem(OracleException) : {ex.ToString()}");
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    //판매 실패
                    S_SellItem sellPacket = new S_SellItem() {
                        CharacterDBId = character.CharacterDBId,
                        Ok = false,
                        ErrorCode = 4,
                    };
                    character.Session.SendPacket(sellPacket);
                    Console.WriteLine($"SellItem(Exception) : {ex.ToString()}");
                }
            }
        }

        public static void EquipItem(Character character, Item item) {
            if (character == null || item == null) {
                return;
            }

            ItemDB itemDB = new ItemDB() {
                ItemDBId = item.ItemDBId,
                Equipped = item.Equipped
            };

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    try {
                        db.Entry(itemDB).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                        db.Entry(itemDB).Property(nameof(ItemDB.Equipped)).IsModified = true;
                        bool success = db.SaveChangesEx();
                        if (!success) {
                            //실패할 경우
                            character.Room.Push(character.HandleEquipItem, item.ItemDBId, !item.Equipped);
                        }
                    }
                    catch (OracleException ex) {
                        Console.WriteLine($"EquipItem(OracleException) : {ex.ToString()}");

                    }
                    catch (Exception ex) {
                    Console.WriteLine($"EquipItem(Exception) : {ex.ToString()}");

                    }
                }
            });
        }
    }
}
