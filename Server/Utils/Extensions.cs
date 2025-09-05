using Server.DB;

namespace Server {
    public static class Extensions {
        public static bool SaveChangesEx(this AppDbContext db) {
            try {
                db.SaveChanges();
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
