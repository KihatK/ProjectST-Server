using Server.Data;
using System.Security.Cryptography;

namespace Server.Items {
    class ItemForGacha {
        public int id;
        public double probability;
    }
    class Gacha {
        #region Singleton
        private static Gacha instance = null;
        private Gacha() { }

        public static Gacha Instance {
            get {
                if (instance == null) {
                    instance = new Gacha();
                }
                return instance;
            }
        }
        #endregion

        public int GachaItem(List<int> ownedItem) {
            List<ItemForGacha> itemWithoutOwned = new List<ItemForGacha>();
            foreach (int id in DataManager.ItemDict.Keys) {
                if (ownedItem.Contains(id)) {
                    continue;
                }
                itemWithoutOwned.Add(new ItemForGacha() { id = id, probability = 0 });
            }
            if (itemWithoutOwned.Count == 0) {
                return -1;
            }
            return Roll(itemWithoutOwned);
        }

        public int Roll(List<ItemForGacha> itemCandidate) {
            double roll = GetSecureRandomDouble(); // 암호학적으로 안전한 난수 생성
            double cumulativeProbability = 0.0;
            double prob = (double)1 / itemCandidate.Count;

            // 각 아이템의 확률에 맞춰 선택
            foreach (var item in itemCandidate) {
                cumulativeProbability += prob;
                if (roll < cumulativeProbability) {
                    return item.id; // 확률 범위 안에 있으면 해당 아이템 반환
                }
            }

            return -1; // 만약 아이템이 선택되지 않았다면 null 반환 (오류 방지용)
        }

        double GetSecureRandomDouble() {
            // 8바이트(64비트) 난수 생성
            byte[] randomNumber = new byte[8];
            RandomNumberGenerator.Fill(randomNumber);

            // 생성된 바이트를 double로 변환 (0.0 ~ 1.0 사이의 값)
            ulong uint64 = BitConverter.ToUInt64(randomNumber, 0);
            return uint64 / (1.0 + ulong.MaxValue);
        }
    }
}
