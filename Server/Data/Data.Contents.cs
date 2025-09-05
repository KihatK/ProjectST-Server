using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using Server.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data {
    #region Skill
    [Serializable]
    public class SkillData {
        public int id;
        public string name;
        public string description;
        public float cooltime;
        public Season season;
        public Dictionary<int, Dictionary<string, float>> level;
    }

    public class FarmSkillData : SkillData {

    }

    [Serializable]
    public class SkillLoader : ILoader<int, SkillData> {
        public List<FarmSkillData> farm = new List<FarmSkillData>();

        public Dictionary<int, SkillData> MakeDict() {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in farm) {
                dict.Add(skill.id, skill);
            }
            return dict;
        }
    }
    #endregion

    #region Item
    [Serializable]
    public class ItemData {
        public string Type;
        public int ID;
        //public string Name;
        //public string IconPath;
        //public string Description;
        public ItemType itemType;
        public int price;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData> {
        string _filename = "";
        public ItemLoader(string filename) {
            _filename = filename;
        }
        public Dictionary<int, ItemData> MakeDict() {
            List<ItemData> items;
            if (ConfigManager.Config.Deploy) {
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemData>>(File.ReadAllText($"{ConfigManager.Config.oracleDataPath}/{_filename}.json"));
            }
            else {
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemData>>(File.ReadAllText($"{ConfigManager.Config.dataPath}/{_filename}.json"));
            }
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in items) {
                switch (item.Type) {
                    case "Hand":
                        item.itemType = ItemType.Hand;
                        break;
                    case "Bag":
                        item.itemType = ItemType.Bag;
                        break;
                    case "Bottom":
                        item.itemType = ItemType.Bottom;
                        break;
                    case "Handwear":
                        item.itemType = ItemType.Handwear;
                        break;
                    case "Earing":
                        item.itemType = ItemType.Earing;
                        break;
                    case "Eyewear":
                        item.itemType = ItemType.Eyewear;
                        break;
                    case "Headgear":
                        item.itemType = ItemType.Headgear;
                        break;
                    case "Shoes":
                        item.itemType = ItemType.Shoes;
                        break;
                    case "Top":
                        item.itemType = ItemType.Top;
                        break;
                    case "Eye":
                        item.itemType = ItemType.Eye;
                        break;
                    case "Eyebrow":
                        item.itemType = ItemType.Eyebrow;
                        break;
                    case "Hair":
                        item.itemType = ItemType.Hair;
                        break;
                    case "Lips":
                        item.itemType = ItemType.Lips;
                        break;
                    case "Mustache":
                        item.itemType = ItemType.Mustache;
                        break;
                }
                dict.Add(item.ID, item);
            }
            return dict;
        }
    }
    #endregion
}
