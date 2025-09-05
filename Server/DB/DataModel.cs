using System.ComponentModel.DataAnnotations.Schema;

namespace Server.DB {
    public interface IHasTimeStamps {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
    [Table("Account")]
    public class AccountDB : IHasTimeStamps {
        public int AccountDBId { get; set; }
        public string Accountname { get; set; }
        public string Password { get; set; }
        public int LoginType { get; set; }
        public CharacterDB Character { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Token")]
    public class TokenDB : IHasTimeStamps {
        public int TokenDBId { get; set; }
        public string Tokenname { get; set; }
        public int AccountDBId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;

    }

    [Table("Character")]
    public class CharacterDB : IHasTimeStamps {
        public int CharacterDBId { get; set; }
        public string Nickname { get; set; }
        public int Money { get; set; } = 0;
        public int Dept { get; set; }
        public int Location { get; set; } = 1;
        public int FamrCoin { get; set; } = 0;
        public int SpringSkillLevel { get; set; }
        public int SummerSkillLevel { get; set; }
        public int AutumnSkillLevel { get; set; }
        public int WinterSkillLevel { get; set; }
        public ICollection<ItemDB>? Items { get; set; }
        public ICollection<TitleDB>? Titles { get; set; }
        public ICollection<FriendDB>? Friends { get; set; }
        public int AccountDBId { get; set; }
        public AccountDB Account { get; set; } = null!;
        public int? FarmDBId { get; set; }
        public FarmDB? Farm { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Friend")]
    public class FriendDB : IHasTimeStamps {
        public int CharacterDBId { get; set; }
        public CharacterDB Character { get; set; }

        public int FriendDBId { get; set; }
        public CharacterDB FriendCharacter { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Item")]
    public class ItemDB : IHasTimeStamps {
        public int ItemDBId { get; set; }
        public int TemplateId { get; set; }
        public int Count { get; set; }
        public int Slot { get; set; }
        public bool Equipped { get; set; } = false;
        [ForeignKey("Owner")]
        public int? OwnerDBId { get; set; }
        public CharacterDB Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Farm")]
    public class FarmDB : IHasTimeStamps {
        public int FarmDBId { get; set; }
        public int OwnerDBId { get; set; }
        public CharacterDB Owner { get; set; } = null!;
        public ICollection<CropDB>? Crops { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Crop")]
    public class CropDB : IHasTimeStamps {
        public int CropDBId { get; set; }
        public int TemplateId { get; set; }
        public int GrowthTime { get; set; }
        public float PosX { get; set; } = 0;
        public float PosY { get; set; } = 0;
        public float PosZ { get; set; } = 0;
        [ForeignKey("BelongingFarm")]
        public int BelongingFarmDBId { get; set; }
        public FarmDB BelongingFarm { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [Table("Title")]
    public class TitleDB : IHasTimeStamps {
        public int TitleDBId { get; set; }
        public int TemplateId { get; set; }
        [ForeignKey("Owner")]
        public int? OwnerDBId { get; set; }
        public CharacterDB Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }
}
